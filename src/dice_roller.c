#include <stdio.h>
#include <stdlib.h>
#include <time.h>

int roll_die(void) {
    return (rand() % 6) + 1;
}

int main(void) {
    srand(time(NULL));

    printf("Rolling the die...\n");
    printf("You got: %d\n", roll_die());

    return 0;
}
