#include <stdio.h>

int main(void) {
    char name[50];

    printf("What's your name? ");
    scanf("%49s", name);

    printf("Hello, %s! Welcome to C programming.\n", name);
    return 0;
}
