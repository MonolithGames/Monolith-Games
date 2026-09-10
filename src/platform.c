#include "platform.h"
#include <math.h>

#define TOPBAR_HEIGHT 56
#define MENU_WIDTH 120
#define MENU_HEIGHT 40
#define RELOAD_TIMER 1

static int menuOpen = 0;
static int reloadSpin = 0;
static RECT dotsRect, chatRect, reloadRect, addrRect, favRect, accRect, tabRect[3];
#define BG_R 30
#define BG_G 30
#define BG_B 30

void DrawReloadIcon(HDC hdc, int x, int y, int spin)
{
    HPEN pen = CreatePen(PS_SOLID, 2, RGB(40, 40, 40));
    SelectObject(hdc, pen);
    Arc(hdc, x, y, x + 20, y + 20, x + 10, y, x + 10, y + 20);
    MoveToEx(hdc, x + 10, y, NULL);
    LineTo(hdc, x + 10, y + 5);
    DeleteObject(pen);
}

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

    addrRect.left = 80; addrRect.top = 12;
    addrRect.right = rect->right - 300; addrRect.bottom = 44;
    HBRUSH addrBrush = CreateSolidBrush(RGB(255, 255, 255));
    RoundRect(hdc, addrRect.left, addrRect.top, addrRect.right, addrRect.bottom, 10, 10);
    DeleteObject(addrBrush);
    DrawText(hdc, "Search or enter address", -1, &addrRect,
             DT_SINGLELINE | DT_VCENTER | DT_CENTER);

    TextOut(hdc, rect->right - 220, 18, "★", 3);
    TextOut(hdc, rect->right - 180, 18, "👤", 3);
    TextOut(hdc, rect->right - 260, 18, "💬 Chat", 7);

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

    dotsRect.left = rect->right - 330;
    dotsRect.top = 18;
    dotsRect.right = dotsRect.left + 20;
    dotsRect.bottom = 38;
    TextOut(hdc, dotsRect.left, dotsRect.top, "...", 3);

    DeleteObject(font);
}

void DrawMenu(HDC hdc)
{
    if (!menuOpen) return;
    RECT menu = {dotsRect.left, dotsRect.bottom + 4, dotsRect.left + MENU_WIDTH, dotsRect.bottom + 4 + MENU_HEIGHT};
    HBRUSH b = CreateSolidBrush(RGB(255,255,255));
    FillRect(hdc, &menu, b);
    DeleteObject(b);
    Rectangle(hdc, menu.left, menu.top, menu.right, menu.bottom);
    TextOut(hdc, menu.left + 10, menu.top + 10, "Settings", 8);
}

int PointInRect(RECT* r, int x, int y)
{
    return (x >= r->left && x <= r->right && y >= r->top && y <= r->bottom);
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
            RECT menuItem = {dotsRect.left + 10, dotsRect.bottom + 14, dotsRect.left + 110, dotsRect.bottom + 34};
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

        if (PointInRect(&reloadRect, x, y))
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
        DrawMenu(hdc);
        return 1;
    }
    }

    return DefWindowProc(hwnd, msg, wParam, lParam);
}
