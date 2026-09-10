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

#endif
