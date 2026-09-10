#include <windows.h>

// Change your background color here:
#define BG_R  30
#define BG_G  30
#define BG_B  30
// Example above = dark gray. Set whatever you want.

LRESULT CALLBACK WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;

    // Paint background WITHOUT triggering Windows spinner
    case WM_ERASEBKGND:
    {
        HDC hdc = (HDC)wParam;

        RECT rect;
        GetClientRect(hwnd, &rect);

        HBRUSH brush = CreateSolidBrush(RGB(BG_R, BG_G, BG_B));
        FillRect(hdc, &rect, brush);
        DeleteObject(brush);

        return 1; // tell Windows we handled background erase
    }
    }

    return DefWindowProc(hwnd, msg, wParam, lParam);
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
                   LPSTR lpCmdLine, int nCmdShow)
{
    (void)hPrevInstance;
    (void)lpCmdLine;

    const char CLASS_NAME[] = "AlphabetMediaWindowClass";

    WNDCLASS wc = {0};
    wc.lpfnWndProc   = WindowProc;
    wc.hInstance     = hInstance;
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
        hInstance,
        NULL
    );

    // Open in bordered fullscreen
    ShowWindow(hwnd, SW_SHOWMAXIMIZED);

    MSG msg = {0};
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return 0;
}
