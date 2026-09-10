#ifndef PLATFORM_H
#define PLATFORM_H

#include <windows.h>

// Window creation
HWND Platform_CreateWindow(HINSTANCE instance, int showMode);

// Message loop
void Platform_RunMessageLoop(void);

// Window procedure
LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

#endif
