#include "engine.h"
#include "logger.h"
#include "time.h"

static int running = 1;

void engine_init(void) {
    log_info("Engine initialized.");
}

void engine_shutdown(void) {
    log_info("Engine shutting down.");
}

void engine_run(void (*game_update)(float)) {
    engine_init();

    while (running) {
        float dt = time_delta();
        game_update(dt);
    }

    engine_shutdown();
}

void engine_stop(void) {
    running = 0;
}
