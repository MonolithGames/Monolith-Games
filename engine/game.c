#include <stdio.h>
#include "game.h"
#include "engine.h"

void game_update(float dt) {
    printf("Game updating... dt = %.4f\n", dt);

    // Example: stop engine after 3 seconds
    static float timer = 0;
    timer += dt;

    if (timer >= 3.0f) {
        printf("Stopping engine.\n");
        engine_stop();
    }
}
