#ifndef PLATFORM_H
#define PLATFORM_H

#include <windows.h>

HWND Platform_CreateWindow(HINSTANCE instance, int showMode);
void Platform_RunMessageLoop(void);
LRESULT CALLBACK Platform_WindowProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

#endif
