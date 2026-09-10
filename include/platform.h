#ifndef PLATFORM_H
#define PLATFORM_H

#include <windows.h>

// Main window
HWND Platform_CreateWindow(HINSTANCE instance, int showMode);
void Platform_RunMessageLoop(void);
LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

// Settings window
void OpenSettingsWindow(HINSTANCE instance);
LRESULT CALLBACK SettingsWindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

// Chat window
void OpenChatWindow(HINSTANCE instance);
LRESULT CALLBACK ChatWindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

// Tab bar
void DrawTabBar(HDC hdc, RECT* rect);
void SetActiveTab(int index);

// Hover
void UpdateHoverState(int x, int y);

// Address bar
extern HWND gAddressBar;

// ------------------------------------------------------------
// Launcher system (NEW)
// ------------------------------------------------------------
void DrawLauncher(HDC hdc, RECT* rect);
void UpdateLauncherClick(int x, int y, HWND hwnd);
void AnimateLauncher(HWND hwnd);

extern BOOL launcherOpen;
extern int launcherProgress;

#endif
