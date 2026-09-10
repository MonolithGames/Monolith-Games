#ifndef PLATFORM_H
#define PLATFORM_H

#include <windows.h>

HWND Platform_CreateWindow(HINSTANCE instance, int showMode);
void Platform_RunMessageLoop(void);
LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

void OpenSettingsWindow(HINSTANCE instance);
LRESULT CALLBACK SettingsWindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

void OpenChatWindow(HINSTANCE instance);
LRESULT CALLBACK ChatWindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

void DrawTabBar(HDC hdc, RECT* rect);
void SetActiveTab(int index);

#endif
