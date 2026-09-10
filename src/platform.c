#include "platform.h"

// Top bar height
#define TOPBAR_HEIGHT 48

// Background color
#define BG_R  30
#define BG_G  30
#define BG_B  30

void DrawTopBar(HDC hdc, RECT* rect)
{
    // Top bar background
    RECT topbar = {0, 0, rect->right, TOPBAR_HEIGHT};
    HBRUSH bar = CreateSolidBrush(RGB(240, 240, 240));
    FillRect(hdc, &topbar, bar);
    DeleteObject(bar);

    SetBkMode(hdc, TRANSPARENT);
    SetTextColor(hdc, RGB(40, 40, 40));

    // Left menu button (three lines)
    TextOut(hdc, 12, 14, "≡", 3);

    // Reload icon
    TextOut(hdc, 48, 14, "⟳", 3);

    // Address bar
    RECT addr = {80, 10, rect->right - 260, 38};
    HBRUSH addrBrush = CreateSolidBrush(RGB(255, 255, 255));
    FillRect(hdc, &addr, addrBrush);
    DeleteObject(addrBrush);

    DrawText(hdc, "Search or enter address", -1, &addr,
             DT_SINGLELINE | DT_VCENTER | DT_LEFT);

    // Favorites star
    TextOut(hdc, rect->right - 220, 14, "★", 3);

    // Account icon (circle)
    TextOut(hdc, rect->right - 180, 14, "👤", 3);

    // Chat button
    TextOut(hdc, rect->right - 300, 14, "💬 Chat", 7);

    // Tabs
    TextOut(hdc, rect->right - 140, 14, "Tab 1", 5);
    TextOut(hdc, rect->right - 90, 14, "Tab 2", 5);
    TextOut(hdc, rect->right - 40, 14, "Tab 3", 5);
}

LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
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

        // Draw top bar
        DrawTopBar(hdc, &rect);

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
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
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
