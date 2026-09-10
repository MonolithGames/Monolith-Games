#include "platform.h"

#define TOPBAR_HEIGHT 56
#define MENU_WIDTH 120
#define MENU_HEIGHT 40

static int menuOpen = 0;
static RECT dotsRect = {0};
static RECT chatRect = {0};
static RECT reloadRect = {0};
static RECT addrRect = {0};
static RECT favRect = {0};
static RECT accRect = {0};
static RECT tabRect[3];

#define BG_R  30
#define BG_G  30
#define BG_B  30

void DrawTopBar(HDC hdc, RECT* rect)
{
    RECT topbar = {0, 0, rect->right, TOPBAR_HEIGHT};
    HBRUSH bar = CreateSolidBrush(RGB(240, 240, 240));
    FillRect(hdc, &topbar, bar);
    DeleteObject(bar);

    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(40, 40, 40));

    // Corner button
    TextOut(hdc, 12, 18, "☰", 3);

    // Reload icon
    reloadRect.left = 48; reloadRect.top = 18;
    reloadRect.right = 68; reloadRect.bottom = 38;
    TextOut(hdc, reloadRect.left, reloadRect.top, "⟳", 3);

    // Address bar
    addrRect.left = 80; addrRect.top = 12;
    addrRect.right = rect->right - 300; addrRect.bottom = 44;
    HBRUSH addrBrush = CreateSolidBrush(RGB(255, 255, 255));
    FillRect(hdc, &addrRect, addrBrush);
    DeleteObject(addrBrush);
    DrawText(hdc, "Search or enter address", -1, &addrRect,
             DT_SINGLELINE | DT_VCENTER | DT_LEFT | DT_CENTER);

    // Favorites
    favRect.left = rect->right - 220; favRect.top = 18;
    favRect.right = favRect.left + 20; favRect.bottom = 38;
    TextOut(hdc, favRect.left, favRect.top, "★", 3);

    // Account icon
    accRect.left = rect->right - 180; accRect.top = 18;
    accRect.right = accRect.left + 20; accRect.bottom = 38;
    TextOut(hdc, accRect.left, accRect.top, "👤", 3);

    // Chat icon
    chatRect.left = rect->right - 260; chatRect.top = 18;
    chatRect.right = chatRect.left + 60; chatRect.bottom = 38;
    TextOut(hdc, chatRect.left, chatRect.top, "💬 Chat", 7);

    // Tabs
    for (int i = 0; i < 3; i++)
    {
        tabRect[i].left = rect->right - (140 - i * 50);
        tabRect[i].top = 18;
        tabRect[i].right = tabRect[i].left + 40;
        tabRect[i].bottom = 38;
        char label[8];
        wsprintf(label, "Tab %d", i + 1);
        TextOut(hdc, tabRect[i].left, tabRect[i].top, label, lstrlen(label));
    }

    // Three dots
    dotsRect.left = rect->right - 330;
    dotsRect.top = 18;
    dotsRect.right = dotsRect.left + 20;
    dotsRect.bottom = 38;
    TextOut(hdc, dotsRect.left, dotsRect.top, "...", 3);
}

void DrawMenu(HDC hdc)
{
    if (!menuOpen) return;

    RECT menu = {
        dotsRect.left,
        dotsRect.bottom + 4,
        dotsRect.left + MENU_WIDTH,
        dotsRect.bottom + 4 + MENU_HEIGHT
    };

    HBRUSH b = CreateSolidBrush(RGB(255,255,255));
    FillRect(hdc, &menu, b);
    DeleteObject(b);
    Rectangle(hdc, menu.left, menu.top, menu.right, menu.bottom);
    TextOut(hdc, menu.left + 10, menu.top + 10, "Settings", 8);
}

int PointInRect(RECT* r, int x, int y)
{
    return (x >= r->left && x <= r->right &&
            y >= r->top  && y <= r->bottom);
}

LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_LBUTTONDOWN:
    {
        int x = LOWORD(lParam);
        int y = HIWORD(lParam);

        if (PointInRect(&dotsRect, x, y))
        {
            menuOpen = !menuOpen;
            InvalidateRect(hwnd, NULL, TRUE);
            return 0;
        }

        if (menuOpen)
        {
            RECT menuItem = {
                dotsRect.left + 10,
                dotsRect.bottom + 14,
                dotsRect.left + 110,
                dotsRect.bottom + 34
            };

            if (PointInRect(&menuItem, x, y))
            {
                menuOpen = 0;
                OpenSettingsWindow((HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE));
                InvalidateRect(hwnd, NULL, TRUE);
                return 0;
            }
        }

        if (PointInRect(&chatRect, x, y))
        {
            OpenChatWindow((HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE));
            return 0;
        }

        break;
    }

    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;

    case WM_ERASEBKGND:
    {
        HDC hdc = (HDC)wParam;
        RECT rect;
        GetClientRect(hwnd, &rect);

        HBRUSH bg = CreateSolidBrush(RGB(BG_R, BG_G, BG_B));
        FillRect(hdc, &rect, bg);
        DeleteObject(bg);

        DrawTopBar(hdc, &rect);
        DrawMenu(hdc);

        return 1;
    }
    }

    return DefWindowProc(hwnd, msg, wParam, lParam);
}

HWND Platform_CreateWindow(HINSTANCE instance, int showMode)
{
    const char CLASS_NAME[] = "AlphabetMediaWindowClass";

    WNDCLASS wc = {0};
    wc.lpfnWndProc   = Platform_WindowProc;
    wc.hInstance     = instance;
    wc.lpszClassName = CLASS_NAME;
    wc.hbrBackground = NULL;

    RegisterClass(&wc);

    HWND hwnd = CreateWindowEx(
        0,
        CLASS_NAME,
        "Alphabet Media",
        WS_POPUP | WS_VISIBLE,
        0, 0,
        1280, 720,
        NULL,
        NULL,
        instance,
        NULL
    );

    ShowWindow(hwnd, SW_SHOWMAXIMIZED);
    return hwnd;
}

void Platform_RunMessageLoop(void)
{
    MSG msg = {0};
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
}
