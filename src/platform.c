#include "platform.h"
#include <windows.h>
#include <math.h>
#include <wingdi.h>

#define TOPBAR_HEIGHT 56
#define TABBAR_HEIGHT 36
#define MENU_WIDTH 120
#define MENU_HEIGHT 40
#define RELOAD_TIMER 1

HWND gAddressBar = NULL;

static RECT dotsRect, chatRect, reloadRect, favRect, accRect, tabRect[3];
static int menuOpen = 0;
static int reloadSpin = 0;
static int activeTab = 0;
static int hoverIndex = -1;

#define BG_R 30
#define BG_G 30
#define BG_B 30

// ------------------------------------------------------------
// Hover detection
// ------------------------------------------------------------
void UpdateHoverState(int x, int y)
{
    hoverIndex = -1;

    for (int i = 0; i < 3; i++)
        if (x >= tabRect[i].left && x <= tabRect[i].right &&
            y >= tabRect[i].top && y <= tabRect[i].bottom)
            hoverIndex = i;
}

// ------------------------------------------------------------
// Draw Reload Icon
// ------------------------------------------------------------
void DrawReloadIcon(HDC hdc, int x, int y, int spin)
{
    HPEN pen = CreatePen(PS_SOLID, 2, RGB(40, 40, 40));
    SelectObject(hdc, pen);

    Arc(hdc, x, y, x + 20, y + 20, x + 10, y, x + 10, y + 20);
    MoveToEx(hdc, x + 10, y, NULL);
    LineTo(hdc, x + 10, y + 5);

    DeleteObject(pen);
}

// ------------------------------------------------------------
// Draw Fluent Shadow
// ------------------------------------------------------------
void DrawFluentShadow(HDC hdc, RECT* rect)
{
    TRIVERTEX vertex[2] = {
        {0, TOPBAR_HEIGHT, RGB(240,240,240), 0, 0},
        {rect->right, TOPBAR_HEIGHT + 12, RGB(200,200,200), 0, 0}
    };

    GRADIENT_RECT gRect = {0, 1};
    GradientFill(hdc, vertex, 2, &gRect, 1, GRADIENT_FILL_RECT_V);
}

// ------------------------------------------------------------
// Draw Top Bar
// ------------------------------------------------------------
void DrawTopBar(HDC hdc, RECT* rect)
{
    HFONT font = CreateFont(18, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE,
                            DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                            DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE, "Segoe UI Symbol");
    SelectObject(hdc, font);

    RECT topbar = {0, 0, rect->right, TOPBAR_HEIGHT};
    HBRUSH bar = CreateSolidBrush(RGB(240, 240, 240));
    FillRect(hdc, &topbar, bar);
    DeleteObject(bar);

    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(40, 40, 40));

    TextOut(hdc, 12, 18, "☰", 3);

    reloadRect.left = 48; reloadRect.top = 18;
    reloadRect.right = 68; reloadRect.bottom = 38;
    DrawReloadIcon(hdc, reloadRect.left, reloadRect.top, reloadSpin);

    favRect.left = rect->right - 220; favRect.top = 18;
    accRect.left = rect->right - 180; accRect.top = 18;
    chatRect.left = rect->right - 260; chatRect.top = 18;

    TextOut(hdc, favRect.left, favRect.top, "★", 3);
    TextOut(hdc, accRect.left, accRect.top, "👤", 3);
    TextOut(hdc, chatRect.left, chatRect.top, "💬 Chat", 7);

    dotsRect.left = rect->right - 330; dotsRect.top = 18;
    TextOut(hdc, dotsRect.left, dotsRect.top, "...", 3);

    DeleteObject(font);
}

// ------------------------------------------------------------
// Draw Tab Bar
// ------------------------------------------------------------
void DrawTabBar(HDC hdc, RECT* rect)
{
    HFONT font = CreateFont(16, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE,
                            DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                            DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE, "Segoe UI");
    SelectObject(hdc, font);

    RECT tabbar = {0, TOPBAR_HEIGHT, rect->right, TOPBAR_HEIGHT + TABBAR_HEIGHT};
    HBRUSH bar = CreateSolidBrush(RGB(250, 250, 250));
    FillRect(hdc, &tabbar, bar);
    DeleteObject(bar);

    SetBkMode(hdc, TRANSPARENT);

    for (int i = 0; i < 3; i++)
    {
        tabRect[i].left = 20 + i * 100;
        tabRect[i].top = TOPBAR_HEIGHT + 8;
        tabRect[i].right = tabRect[i].left + 80;
        tabRect[i].bottom = TOPBAR_HEIGHT + 28;

        if (hoverIndex == i)
            SetTextColor(hdc, RGB(0, 120, 215));
        else
            SetTextColor(hdc, RGB(40, 40, 40));

        char label[8];
        wsprintf(label, "Tab %d", i + 1);
        DrawText(hdc, label, -1, &tabRect[i], DT_SINGLELINE | DT_VCENTER | DT_CENTER);

        if (i == activeTab)
        {
            HPEN pen = CreatePen(PS_SOLID, 2, RGB(0, 120, 215));
            SelectObject(hdc, pen);
            MoveToEx(hdc, tabRect[i].left, tabRect[i].bottom, NULL);
            LineTo(hdc, tabRect[i].right, tabRect[i].bottom);
            DeleteObject(pen);
        }
    }

    DeleteObject(font);
}

// ------------------------------------------------------------
// Draw Menu
// ------------------------------------------------------------
void DrawMenu(HDC hdc)
{
    if (!menuOpen) return;

    RECT menu = {dotsRect.left, dotsRect.bottom + 4,
                 dotsRect.left + MENU_WIDTH, dotsRect.bottom + 4 + MENU_HEIGHT};

    HBRUSH b = CreateSolidBrush(RGB(255,255,255));
    FillRect(hdc, &menu, b);
    DeleteObject(b);

    Rectangle(hdc, menu.left, menu.top, menu.right, menu.bottom);
    TextOut(hdc, menu.left + 10, menu.top + 10, "Settings", 8);
}

// ------------------------------------------------------------
// Window Procedure
// ------------------------------------------------------------
LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_MOUSEMOVE:
        UpdateHoverState(LOWORD(lParam), HIWORD(lParam));
        InvalidateRect(hwnd, NULL, FALSE);
        break;

    case WM_LBUTTONDOWN:
    {
        int x = LOWORD(lParam);
        int y = HIWORD(lParam);

        for (int i = 0; i < 3; i++)
            if (x >= tabRect[i].left && x <= tabRect[i].right &&
                y >= tabRect[i].top && y <= tabRect[i].bottom)
            {
                activeTab = i;
                InvalidateRect(hwnd, NULL, TRUE);
                return 0;
            }

        if (x >= reloadRect.left && x <= reloadRect.right &&
            y >= reloadRect.top && y <= reloadRect.bottom)
        {
            reloadSpin = 1;
            SetTimer(hwnd, RELOAD_TIMER, 100, NULL);
            InvalidateRect(hwnd, NULL, TRUE);
            return 0;
        }

        break;
    }

    case WM_TIMER:
        if (wParam == RELOAD_TIMER)
        {
            reloadSpin = 0;
            KillTimer(hwnd, RELOAD_TIMER);
            InvalidateRect(hwnd, NULL, TRUE);
        }
        return 0;

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
        DrawFluentShadow(hdc, &rect);
        DrawTabBar(hdc, &rect);
        DrawMenu(hdc);

        return 1;
    }
    }

    return DefWindowProc(hwnd, msg, wParam, lParam);
}

// ------------------------------------------------------------
// Create Window
// ------------------------------------------------------------
HWND Platform_CreateWindow(HINSTANCE instance, int showMode)
{
    const char CLASS_NAME[] = "AlphabetMediaWindowClass";

    WNDCLASS wc = {0};
    wc.lpfnWndProc   = Platform_WindowProc;
    wc.hInstance     = instance;
    wc.lpszClassName = CLASS_NAME;

    RegisterClass(&wc);

    HWND hwnd = CreateWindowEx(
        0,
        CLASS_NAME,
        "Alphabet Media",
        WS_OVERLAPPEDWINDOW | WS_MAXIMIZE,
        CW_USEDEFAULT, CW_USEDEFAULT,
        1280, 720,
        NULL,
        NULL,
        instance,
        NULL
    );

    ShowWindow(hwnd, SW_SHOWMAXIMIZED);

    gAddressBar = CreateWindowEx(
        WS_EX_CLIENTEDGE,
        "EDIT",
        "",
        WS_CHILD | WS_VISIBLE | ES_AUTOHSCROLL,
        80, 12, 400, 28,
        hwnd,
        (HMENU)1001,
        instance,
        NULL
    );

    return hwnd;
}

// ------------------------------------------------------------
// Message Loop
// ------------------------------------------------------------
void Platform_RunMessageLoop(void)
{
    MSG msg = {0};
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
}
