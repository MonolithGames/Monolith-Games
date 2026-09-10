#include <stdio.h>
#include <string.h>
#include "commands.h"

// ANSI colors
#define C_RESET   "\x1b[0m"
#define C_RED     "\x1b[31m"
#define C_GREEN   "\x1b[32m"
#define C_YELLOW  "\x1b[33m"
#define C_BLUE    "\x1b[34m"
#define C_MAGENTA "\x1b[35m"
#define C_CYAN    "\x1b[36m"

// Theme presets
ConsoleTheme THEME_DEFAULT = {
    C_CYAN, C_YELLOW, C_GREEN, C_RED, C_RESET
};

ConsoleTheme THEME_NEON = {
    C_MAGENTA, C_CYAN, C_YELLOW, C_RED, C_RESET
};

ConsoleTheme THEME_SOLARIZED = {
    "\x1b[36m", "\x1b[33m", "\x1b[32m", "\x1b[31m", C_RESET
};

// Active theme
ConsoleTheme ACTIVE_THEME = {0};

// Theme switcher
void set_theme(const char *name) {
    if (strcmp(name, "default") == 0) {
        ACTIVE_THEME = THEME_DEFAULT;
        printf("%sTheme set to DEFAULT%s\n", ACTIVE_THEME.accent, ACTIVE_THEME.reset);
        return;
    }

    if (strcmp(name, "neon") == 0) {
        ACTIVE_THEME = THEME_NEON;
        printf("%sTheme set to NEON%s\n", ACTIVE_THEME.accent, ACTIVE_THEME.reset);
        return;
    }

    if (strcmp(name, "solarized") == 0) {
        ACTIVE_THEME = THEME_SOLARIZED;
        printf("%sTheme set to SOLARIZED%s\n", ACTIVE_THEME.accent, ACTIVE_THEME.reset);
        return;
    }

    printf("%sUnknown theme: %s%s\n", ACTIVE_THEME.error, name, ACTIVE_THEME.reset);
}

// Command handler
void handle_command(const char *cmd) {

    // help
    if (strcmp(cmd, "help") == 0) {
        printf("%sAvailable commands:%s\n", ACTIVE_THEME.primary, ACTIVE_THEME.reset);
        printf("%s  help%s       - Show this list\n", ACTIVE_THEME.secondary, ACTIVE_THEME.reset);
        printf("%s  ping%s       - Test command\n", ACTIVE_THEME.secondary, ACTIVE_THEME.reset);
        printf("%s  theme <name>%s - Change console theme\n", ACTIVE_THEME.secondary, ACTIVE_THEME.reset);
        printf("%s  exit%s       - Quit the program\n", ACTIVE_THEME.secondary, ACTIVE_THEME.reset);
        return;
    }

    // ping
    if (strcmp(cmd, "ping") == 0) {
        printf("%sPONG!%s\n", ACTIVE_THEME.accent, ACTIVE_THEME.reset);
        return;
    }

    // theme command
    if (strncmp(cmd, "theme ", 6) == 0) {
        set_theme(cmd + 6);
        return;
    }

    // unknown
    printf("%sUnknown command: %s%s\n", ACTIVE_THEME.error, cmd, ACTIVE_THEME.reset);
}
