#include "platform.h"

#define TOPBAR_HEIGHT 48
#define MENU_WIDTH 120
#define MENU_HEIGHT 40

// Track menu visibility
static int menuOpen = 0;

// Track three-dots button area
RECT dotsRect = {0};

// Background color
#define BG_R  30
#define BG_G  30
#define BG_B  30

void DrawThreeDots(HDC hdc, int right)
{
    dotsRect.left   = right - 360;
    dotsRect.top    = 14;
    dotsRect.right  = right - 330;
    dotsRect.bottom = 34;

    TextOut(hdc, dotsRect.left, dotsRect.top, "...", 3);
}

void DrawMenu(HDC hdc, int right)
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

        // If menu is open, detect click on "Settings"
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

        // Background
        HBRUSH bg = CreateSolidBrush(RGB(BG_R, BG_G, BG_B));
        FillRect(hdc, &rect, bg);
        DeleteObject(bg);

        // Top bar
        RECT topbar = {0, 0, rect.right, TOPBAR_HEIGHT};
        HBRUSH bar = CreateSolidBrush(RGB(230, 230, 230));
        FillRect(hdc, &topbar, bar);
        DeleteObject(bar);

        // Draw three dots
        DrawThreeDots(hdc, rect.right);

        // Draw menu if open
        DrawMenu(hdc, rect.right);

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
