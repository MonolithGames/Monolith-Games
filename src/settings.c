#include <windows.h>
#include "platform.h"

LRESULT CALLBACK SettingsWindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_DESTROY:
        return 0;

    case WM_PAINT:
    {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hwnd, &ps);

        SetBkMode(hdc, TRANSPARENT);
        TextOut(hdc, 20, 20, "Settings Window", 15);

        EndPaint(hwnd, &ps);
        return 0;
    }
    }

    return DefWindowProc(hwnd, msg, wParam, lParam);
}

void OpenSettingsWindow(HINSTANCE instance)
{
    const char* CLASS_NAME = "AlphabetMediaSettingsWindow";

    WNDCLASS wc = {0};
    wc.lpfnWndProc = SettingsWindowProc;
    wc.hInstance = instance;
    wc.lpszClassName = CLASS_NAME;

    RegisterClass(&wc);

    HWND hwnd = CreateWindowEx(
        0,
        CLASS_NAME,
        "Settings",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        500, 400,
        NULL,
        NULL,
        instance,
        NULL
    );

    ShowWindow(hwnd, SW_SHOW);
}
