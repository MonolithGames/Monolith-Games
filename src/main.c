#include "platform.h"

int WINAPI WinMain(HINSTANCE instance, HINSTANCE prev, LPSTR cmd, int show)
{
    (void)prev;
    (void)cmd;

    Platform_CreateWindow(instance, show);
    Platform_RunMessageLoop();

    return 0;
}
