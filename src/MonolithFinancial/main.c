#include <windows.h>
#include <stdio.h>
#include <tchar.h>

void GetSyncedBalanceText(TCHAR *outBuffer, int maxLen) {
    _tcscpy_s(outBuffer, maxLen, TEXT("Balance $12,450.75"));

    // Check absolute path to Monolith balance.json
    FILE *f = fopen("C:/Users/auora/StudioProjects/Monolith-Games/src/Monolith/App_Data/balance.json", "r");
    if (!f) {
        f = fopen("../Monolith/App_Data/balance.json", "r");
    }
    if (f) {
        char line[512] = {0};
        if (fgets(line, sizeof(line), f)) {
            char *p = strstr(line, "Balance");
            if (p) {
                char *colon = strchr(p, ':');
                if (colon) {
                    double val = atof(colon + 1);
                    if (val > 0) {
                        char fmt[64];
                        sprintf(fmt, "Balance $%.2f", val);
#ifdef UNICODE
                        MultiByteToWideChar(CP_ACP, 0, fmt, -1, outBuffer, maxLen);
#else
                        strcpy_s(outBuffer, maxLen, fmt);
#endif
                    }
                }
            }
        }
        fclose(f);
    }
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
    switch (uMsg) {
    case WM_CREATE:
        SetTimer(hwnd, 1, 3600000, NULL); // Hourly timer for balance sync (1 hour = 3,600,000 ms)
        return 0;
    case WM_TIMER:
        if (wParam == 1) {
            InvalidateRect(hwnd, NULL, FALSE);
        }
        return 0;
    case WM_PAINT: {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hwnd, &ps);

        RECT rect;
        GetClientRect(hwnd, &rect);
        int width = rect.right - rect.left;
        int height = rect.bottom - rect.top;

        if (width <= 0 || height <= 0) {
            EndPaint(hwnd, &ps);
            return 0;
        }

        // Downsampling / Supersampling factor (4x render target, scaling down to running resolution for 8K-level ultra-sharpness)
        int scale = 4;
        int hiWidth = width * scale;
        int hiHeight = height * scale;

        if (hiWidth > 7680) hiWidth = 7680;
        if (hiHeight > 4320) hiHeight = 4320;

        HDC memDC = CreateCompatibleDC(hdc);

        BITMAPINFO bmi = { 0 };
        bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
        bmi.bmiHeader.biWidth = hiWidth;
        bmi.bmiHeader.biHeight = -hiHeight; // top-down
        bmi.bmiHeader.biPlanes = 1;
        bmi.bmiHeader.biBitCount = 32;
        bmi.bmiHeader.biCompression = BI_RGB;

        void *pBits = NULL;
        HBITMAP hBitmap = CreateDIBSection(hdc, &bmi, DIB_RGB_COLORS, &pBits, NULL, 0);
        HBITMAP hOldBitmap = (HBITMAP)SelectObject(memDC, hBitmap);

        // Fill background with Light Pink RGB(255, 182, 193)
        HBRUSH hBrush = CreateSolidBrush(RGB(255, 182, 193));
        RECT hiRect = { 0, 0, hiWidth, hiHeight };
        FillRect(memDC, &hiRect, hBrush);
        DeleteObject(hBrush);

        SetBkMode(memDC, TRANSPARENT);
        SetTextColor(memDC, RGB(255, 255, 255)); // White text

        HFONT hFont = CreateFont(
            130 * scale, 0, 0, 0, FW_BOLD, FALSE, FALSE, FALSE,
            DEFAULT_CHARSET, OUT_OUTLINE_PRECIS, CLIP_DEFAULT_PRECIS,
            CLEARTYPE_QUALITY, VARIABLE_PITCH, TEXT("Arial")
        );
        HFONT hOldFont = (HFONT)SelectObject(memDC, hFont);

        int targetY = (int)(hiHeight * 0.20);

        TCHAR balanceText[128];
        GetSyncedBalanceText(balanceText, 128);

        RECT textRect = { 0, 0, 0, 0 };
        DrawText(memDC, balanceText, -1, &textRect, DT_CALCRECT);
        int textWidth = textRect.right - textRect.left;
        int textHeight = textRect.bottom - textRect.top;

        int targetX = (hiWidth - textWidth) / 2;

        SetRect(&textRect, targetX, targetY, targetX + textWidth, targetY + textHeight);
        DrawText(memDC, balanceText, -1, &textRect, DT_CENTER | DT_VCENTER | DT_SINGLELINE);

        SelectObject(memDC, hOldFont);
        DeleteObject(hFont);

        SetStretchBltMode(hdc, HALFTONE);
        SetBrushOrgEx(hdc, 0, 0, NULL);
        StretchBlt(hdc, 0, 0, width, height, memDC, 0, 0, hiWidth, hiHeight, SRCCOPY);

        SelectObject(memDC, hOldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(memDC);

        EndPaint(hwnd, &ps);
        return 0;
    }
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    }
    return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
    const wchar_t CLASS_NAME[] = L"MonolithFinancialWindowClass";

    WNDCLASS wc = { 0 };
    wc.lpfnWndProc = WindowProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = CLASS_NAME;
    wc.hbrBackground = CreateSolidBrush(RGB(255, 182, 193));
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(101));

    RegisterClass(&wc);

    RECT wr = { 0, 0, 1440, 900 };
    AdjustWindowRect(&wr, WS_OVERLAPPEDWINDOW, FALSE);
    int windowWidth = wr.right - wr.left;
    int windowHeight = wr.bottom - wr.top;

    HWND hwnd = CreateWindowEx(
        0,
        CLASS_NAME,
        TEXT("Monolith Financial"),
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, windowWidth, windowHeight,
        NULL, NULL, hInstance, NULL
    );

    if (hwnd == NULL) {
        return 0;
    }

    HICON hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(101));
    if (hIcon) {
        SendMessage(hwnd, WM_SETICON, ICON_BIG, (LPARAM)hIcon);
        SendMessage(hwnd, WM_SETICON, ICON_SMALL, (LPARAM)hIcon);
    }

    ShowWindow(hwnd, SW_SHOWMAXIMIZED);
    UpdateWindow(hwnd);

    MSG msg = { 0 };
    while (GetMessage(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return 0;
}
