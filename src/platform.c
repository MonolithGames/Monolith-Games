#include "platform.h"

// Background color (edit anytime)
#define BG_R  30
#define BG_G  30
#define BG_B  30

LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;

    // Prevent Windows spinner by drawing background here
    case WM_ERASEBKGND:
    {
        HDC hdc = (HDC)wParam;

        RECT rect;
        GetClientRect(hwnd, &rect);

        HBRUSH brush = CreateSolidBrush(RGB(BG_R, BG_G, BG_B));
        FillRect(hdc, &rect, brush);
        DeleteObject(brush);

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
    wc.hbrBackground = NULL; // we handle background manually

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
