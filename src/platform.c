#include "platform.h"
#include "pipeline.h"
#include <windows.h>
#include <commdlg.h>
#include <shellapi.h>
#include <math.h>
#include <wingdi.h>

#define TOPBAR_HEIGHT 56
#define TABBAR_HEIGHT 40
#define MENU_WIDTH 120
#define MENU_HEIGHT 40
#define RELOAD_TIMER 1
#define LAUNCHER_TIMER 99
#define PIPELINE_TIMER 100

HWND gAddressBar = NULL;
static HWND libraryList = NULL;
static HWND addMediaButton = NULL;
static HWND openMediaButton = NULL;
static HWND buildButton = NULL;
static HWND publishButton = NULL;
static HWND statusLabel = NULL;

#define MAX_MEDIA_ITEMS 128

typedef struct
{
    char path[MAX_PATH];
    BOOL favorite;
} MediaItem;

static MediaItem mediaItems[MAX_MEDIA_ITEMS];
static int mediaItemCount = 0;

static RECT dotsRect, chatRect, reloadRect, favRect, accRect, tabRect[3];
static int menuOpen = 0;
static int reloadSpin = 0;
static int activeTab = 0;
static int hoverIndex = -1;
static char jobStatus[128] = "Ready - select a template to begin";

BOOL launcherOpen = FALSE;
int launcherProgress = 0;

#define BG_R 30
#define BG_G 30
#define BG_B 30

static void RefreshLibrary(void)
{
    SendMessage(libraryList, LB_RESETCONTENT, 0, 0);

    if (activeTab == 0)
    {
        SendMessageA(libraryList, LB_ADDSTRING, 0, (LPARAM)"Skyline Runner | 3D action template");
        SendMessageA(libraryList, LB_ADDSTRING, 0, (LPARAM)"Neon Kart | Multiplayer racing template");
        SendMessageA(libraryList, LB_ADDSTRING, 0, (LPARAM)"Pocket Planet | Casual mobile template");
        return;
    }

    for (int i = 0; i < mediaItemCount; i++)
    {
        if (activeTab == 1 && !mediaItems[i].favorite)
            continue;

        SendMessage(libraryList, LB_ADDSTRING, 0,
                    (LPARAM)mediaItems[i].path);
    }
}

static void SetJobStatus(const char* status)
{
    lstrcpynA(jobStatus, status, sizeof(jobStatus));
    if (statusLabel)
        SetWindowTextA(statusLabel, jobStatus);
}

static void StartTemplateJob(const char* status)
{
    if (SendMessage(libraryList, LB_GETCURSEL, 0, 0) == LB_ERR)
    {
        SetJobStatus("Select a game template first");
        return;
    }

    SetJobStatus(status);
}

static const char* SelectedTemplateName(void)
{
    static char name[128];
    int selected = (int)SendMessage(libraryList, LB_GETCURSEL, 0, 0);

    if (selected == LB_ERR)
        return NULL;

    SendMessageA(libraryList, LB_GETTEXT, selected, (LPARAM)name);
    return name;
}

static void BuildSelectedTemplate(void)
{
    int selected = (int)SendMessage(libraryList, LB_GETCURSEL, 0, 0);

    if (activeTab != 0 || selected == LB_ERR)
    {
        SetJobStatus("Select a game template first");
        return;
    }

    if (!Pipeline_Start(selected))
    {
        SetJobStatus("A pipeline is already running or could not start");
        return;
    }

    SetJobStatus(Pipeline_GetStatus());
    SetTimer(GetParent(libraryList), PIPELINE_TIMER, 1200, NULL);
}

static void PreparePublishing(void)
{
    if (!SelectedTemplateName())
    {
        SetJobStatus("Select a game template first");
        return;
    }

    SetJobStatus("Publishing checklist created: credentials and store review required");
}

static void AddMediaFile(HWND hwnd)
{
    char path[MAX_PATH] = {0};
    OPENFILENAMEA dialog = {0};

    dialog.lStructSize = sizeof(dialog);
    dialog.hwndOwner = hwnd;
    dialog.lpstrFilter = "Media Files\0*.mp3;*.wav;*.mp4;*.avi;*.mkv;*.jpg;*.png\0All Files\0*.*\0";
    dialog.lpstrFile = path;
    dialog.nMaxFile = MAX_PATH;
    dialog.Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST;

    if (!GetOpenFileNameA(&dialog) || mediaItemCount >= MAX_MEDIA_ITEMS)
        return;

    lstrcpynA(mediaItems[mediaItemCount].path, path, MAX_PATH);
    mediaItems[mediaItemCount].favorite = FALSE;
    mediaItemCount++;
    RefreshLibrary();
}

static void OpenSelectedMedia(void)
{
    int selected = (int)SendMessage(libraryList, LB_GETCURSEL, 0, 0);
    if (selected == LB_ERR)
        return;

    char path[MAX_PATH] = {0};
    SendMessage(libraryList, LB_GETTEXT, selected, (LPARAM)path);
    ShellExecuteA(NULL, "open", path, NULL, NULL, SW_SHOWNORMAL);
}

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

    TextOut(hdc, 12, 18, "Menu", 4);

    reloadRect.left = 48; reloadRect.top = 18;
    reloadRect.right = 68; reloadRect.bottom = 38;
    DrawReloadIcon(hdc, reloadRect.left, reloadRect.top, reloadSpin);

    favRect.left = rect->right - 220; favRect.top = 18;
    accRect.left = rect->right - 180; accRect.top = 18;
    chatRect.left = rect->right - 260; chatRect.top = 18;

    TextOut(hdc, favRect.left, favRect.top, "Favorites", 9);
    TextOut(hdc, accRect.left, accRect.top, "Account", 7);
    TextOut(hdc, chatRect.left, chatRect.top, "Chat", 4);

    dotsRect.left = rect->right - 330; dotsRect.top = 18;
    TextOut(hdc, dotsRect.left, dotsRect.top, "...", 3);

    DeleteObject(font);
}

// ------------------------------------------------------------
// Draw Fluent Shadow
// ------------------------------------------------------------
void DrawFluentShadow(HDC hdc, RECT* rect)
{
    TRIVERTEX vertex[2] = {
        {0, TOPBAR_HEIGHT + TABBAR_HEIGHT, 0xF000, 0xF000, 0xF000, 0x0000},
        {rect->right, TOPBAR_HEIGHT + TABBAR_HEIGHT + 12, 0xC000, 0xC000, 0xC000, 0x0000}
    };

    GRADIENT_RECT gRect = {0, 1};
    GradientFill(hdc, vertex, 2, &gRect, 1, GRADIENT_FILL_RECT_V);
}

// ------------------------------------------------------------
// Draw Acrylic Tab Background
// ------------------------------------------------------------
void DrawAcrylicTab(HDC hdc, RECT* r, int hovered, int active)
{
    HDC memDC = CreateCompatibleDC(hdc);
    HBITMAP bmp = CreateCompatibleBitmap(hdc, r->right - r->left, r->bottom - r->top);
    SelectObject(memDC, bmp);

    HBRUSH b = CreateSolidBrush(hovered ? RGB(255,255,255) : RGB(245,245,245));
    FillRect(memDC, &(RECT){0,0,r->right - r->left, r->bottom - r->top}, b);
    DeleteObject(b);

    BLENDFUNCTION blend = {AC_SRC_OVER, 0, active ? 200 : 160, 0};
    AlphaBlend(hdc, r->left, r->top, r->right - r->left, r->bottom - r->top,
               memDC, 0, 0, r->right - r->left, r->bottom - r->top, blend);

    DeleteObject(bmp);
    DeleteDC(memDC);
}

// ------------------------------------------------------------
// Draw Tab Bar
// ------------------------------------------------------------
void DrawTabBar(HDC hdc, RECT* rect)
{
    RECT strip = {0, TOPBAR_HEIGHT, rect->right, TOPBAR_HEIGHT + TABBAR_HEIGHT};
    HBRUSH stripBrush = CreateSolidBrush(RGB(255,255,255));
    FillRect(hdc, &strip, stripBrush);
    DeleteObject(stripBrush);

    HFONT font = CreateFont(16, 0, 0, 0, FW_NORMAL, FALSE, FALSE, FALSE,
                            DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                            DEFAULT_QUALITY, DEFAULT_PITCH | FF_DONTCARE, "Segoe UI");
    SelectObject(hdc, font);

    SetBkMode(hdc, TRANSPARENT);

    for (int i = 0; i < 3; i++)
    {
        tabRect[i].left = 20 + i * 120;
        tabRect[i].top = TOPBAR_HEIGHT + 6;
        tabRect[i].right = tabRect[i].left + 100;
        tabRect[i].bottom = TOPBAR_HEIGHT + 34;

        DrawAcrylicTab(hdc, &tabRect[i], hoverIndex == i, activeTab == i);

        SetTextColor(hdc, RGB(40,40,40));
        const char* tabName = i == 0 ? "Library" : i == 1 ? "Favorites" : "Playlists";
        DrawText(hdc, tabName, -1, &tabRect[i],
                 DT_SINGLELINE | DT_VCENTER | DT_CENTER);

        if (activeTab == i)
        {
            HPEN pen = CreatePen(PS_SOLID, 2, RGB(0,120,215));
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
// Draw Launcher (center circle + animated icons)
// ------------------------------------------------------------
void DrawLauncher(HDC hdc, RECT* rect)
{
    int centerX = rect->right / 2;
    int centerY = rect->bottom - 60;

    HBRUSH white = CreateSolidBrush(RGB(255,255,255));
    SelectObject(hdc, white);
    Ellipse(hdc, centerX - 25, centerY - 25, centerX + 25, centerY + 25);

    float radius = 90.0f * (launcherProgress / 100.0f);

    float angles[5] = { 200, 230, 260, 290, 320 };

    for (int i = 0; i < 5; i++)
    {
        float rad = angles[i] * 3.14159f / 180.0f;

        int x = centerX + (int)(radius * cos(rad));

        // ⭐ UPDATED: icons open upward now
        int y = centerY + (int)(radius * sin(rad));

        int size = 20;
        int alpha = (int)(255 * (launcherProgress / 100.0f));

        HDC memDC = CreateCompatibleDC(hdc);
        HBITMAP bmp = CreateCompatibleBitmap(hdc, size, size);
        SelectObject(memDC, bmp);

        HBRUSH wb = CreateSolidBrush(RGB(255,255,255));
        SelectObject(memDC, wb);
        Ellipse(memDC, 0, 0, size, size);

        BLENDFUNCTION bf = { AC_SRC_OVER, 0, alpha, 0 };
        AlphaBlend(hdc, x - size/2, y - size/2, size, size,
                   memDC, 0, 0, size, size, bf);

        DeleteObject(wb);
        DeleteObject(bmp);
        DeleteDC(memDC);
    }

    DeleteObject(white);
}

// ------------------------------------------------------------
// Handle Launcher Click
// ------------------------------------------------------------
void UpdateLauncherClick(int x, int y, HWND hwnd)
{
    RECT rect;
    GetClientRect(hwnd, &rect);

    int centerX = rect.right / 2;
    int centerY = rect.bottom - 60;

    if (x >= centerX - 25 && x <= centerX + 25 &&
        y >= centerY - 25 && y <= centerY + 25)
    {
        launcherOpen = !launcherOpen;
        SetTimer(hwnd, LAUNCHER_TIMER, 16, NULL);
    }
}

// ------------------------------------------------------------
// Animate Launcher
// ------------------------------------------------------------
void AnimateLauncher(HWND hwnd)
{
    if (launcherOpen)
    {
        if (launcherProgress < 100)
            launcherProgress += 5;
        else
            KillTimer(hwnd, LAUNCHER_TIMER);
    }
    else
    {
        if (launcherProgress > 0)
            launcherProgress -= 5;
        else
            KillTimer(hwnd, LAUNCHER_TIMER);
    }

    InvalidateRect(hwnd, NULL, FALSE);
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

    case WM_SIZE:
        if (libraryList)
        {
            MoveWindow(libraryList, 24, TOPBAR_HEIGHT + TABBAR_HEIGHT + 30,
                       LOWORD(lParam) - 48, HIWORD(lParam) - TOPBAR_HEIGHT - TABBAR_HEIGHT - 90,
                       TRUE);
            MoveWindow(addMediaButton, 24, HIWORD(lParam) - 48, 110, 28, TRUE);
            MoveWindow(openMediaButton, 144, HIWORD(lParam) - 48, 110, 28, TRUE);
            MoveWindow(buildButton, 264, HIWORD(lParam) - 48, 110, 28, TRUE);
            MoveWindow(publishButton, 384, HIWORD(lParam) - 48, 130, 28, TRUE);
            MoveWindow(statusLabel, 530, HIWORD(lParam) - 48, LOWORD(lParam) - 554, 28, TRUE);
        }
        InvalidateRect(hwnd, NULL, TRUE);
        break;

    case WM_COMMAND:
        if (LOWORD(wParam) == 2001 && HIWORD(wParam) == BN_CLICKED)
            AddMediaFile(hwnd);
        else if (LOWORD(wParam) == 2002 && HIWORD(wParam) == BN_CLICKED)
            OpenSelectedMedia();
        else if (LOWORD(wParam) == 2003 && HIWORD(wParam) == LBN_DBLCLK)
            OpenSelectedMedia();
        else if (LOWORD(wParam) == 2004 && HIWORD(wParam) == BN_CLICKED)
            BuildSelectedTemplate();
        else if (LOWORD(wParam) == 2005 && HIWORD(wParam) == BN_CLICKED)
            PreparePublishing();
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
                RefreshLibrary();
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

        UpdateLauncherClick(x, y, hwnd);
        break;
    }

    case WM_TIMER:
        if (wParam == RELOAD_TIMER)
        {
            reloadSpin = 0;
            KillTimer(hwnd, RELOAD_TIMER);
            InvalidateRect(hwnd, NULL, TRUE);
        }
        if (wParam == LAUNCHER_TIMER)
        {
            AnimateLauncher(hwnd);
        }
        if (wParam == PIPELINE_TIMER)
        {
            Pipeline_Tick();
            SetJobStatus(Pipeline_GetStatus());
            if (!Pipeline_IsRunning())
                KillTimer(hwnd, PIPELINE_TIMER);
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
        DrawTabBar(hdc, &rect);
        DrawFluentShadow(hdc, &rect);
        DrawMenu(hdc);
        DrawLauncher(hdc, &rect);

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
        "Monolith - Game Production Client",
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

    libraryList = CreateWindowExA(
        WS_EX_CLIENTEDGE, "LISTBOX", "",
        WS_CHILD | WS_VISIBLE | WS_VSCROLL | LBS_NOTIFY | LBS_NOINTEGRALHEIGHT,
        24, TOPBAR_HEIGHT + TABBAR_HEIGHT + 30, 800, 500,
        hwnd, (HMENU)2003, instance, NULL);

    buildButton = CreateWindowExA(
        0, "BUTTON", "Build",
        WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
        264, 650, 110, 28, hwnd, (HMENU)2004, instance, NULL);

    publishButton = CreateWindowExA(
        0, "BUTTON", "Publish Prep",
        WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
        384, 650, 130, 28, hwnd, (HMENU)2005, instance, NULL);

    statusLabel = CreateWindowExA(
        0, "STATIC", jobStatus,
        WS_CHILD | WS_VISIBLE | SS_LEFT,
        530, 650, 600, 28, hwnd, NULL, instance, NULL);

    RefreshLibrary();

    addMediaButton = CreateWindowExA(
        0, "BUTTON", "Add Media",
        WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
        24, 650, 110, 28, hwnd, (HMENU)2001, instance, NULL);

    openMediaButton = CreateWindowExA(
        0, "BUTTON", "Open",
        WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
        144, 650, 110, 28, hwnd, (HMENU)2002, instance, NULL);

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
