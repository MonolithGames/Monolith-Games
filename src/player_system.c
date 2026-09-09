#include <stdio.h>

struct Player {
    char name[32];
    int health;
    int attack;
};

int main(void) {
    struct Player party[3] = {
        {"Astra", 100, 15},
        {"Kael", 120, 12},
        {"Mira", 90, 18}
    };

    for (int i = 0; i < 3; i++) {
        printf("Player: %s\n", party[i].name);
        printf("  Health: %d\n", party[i].health);
        printf("  Attack: %d\n\n", party[i].attack);
    }

    return 0;
}
