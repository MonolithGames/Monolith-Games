#include <stdio.h>
#include <string.h>
#include "commands.h"

void handle_command(const char *cmd) {
    if (strcmp(cmd, "help") == 0) {
        printf("Available commands:\n");
        printf("  help  - Show this list\n");
        printf("  ping  - Test command\n");
        printf("  exit  - Quit the program\n");
        return;
    }

    if (strcmp(cmd, "ping") == 0) {
        printf("PONG!\n");
        return;
    }

    printf("Unknown command: %s\n", cmd);
}
