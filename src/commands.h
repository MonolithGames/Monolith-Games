#ifndef COMMANDS_H
#define COMMANDS_H

// Console theme structure
typedef struct {
    const char *primary;
    const char *secondary;
    const char *accent;
    const char *error;
    const char *reset;
} ConsoleTheme;

// Global active theme
extern ConsoleTheme ACTIVE_THEME;

// Theme switching
void set_theme(const char *name);

// Command handler
void handle_command(const char *cmd);

#endif
