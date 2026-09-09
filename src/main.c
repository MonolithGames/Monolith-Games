#include "engine/engine.h"
#include "game/game.h"

int main(void) {
    engine_run(game_update);
    return 0;
}
