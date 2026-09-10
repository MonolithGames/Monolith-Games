#include <stdio.h>
#include <string.h>
#include "commands.h"

int main(void) {
    char input[64];

    printf("Monolith Console App\n");
    printf("Type 'help' for commands.\n\n");

    while (1) {
        printf("> ");
        fgets(input, sizeof(input), stdin);

        // Remove newline
        input[strcspn(input, "\n")] = 0;

        if (strcmp(input, "exit") == 0) {
            printf("Exiting...\n");
            break;
        }

        handle_command(input);
    }

    return 0;
}
