#include <time.h>
#include "time.h"

static clock_t last = 0;

float time_delta(void) {
    clock_t now = clock();
    float dt = (float)(now - last) / CLOCKS_PER_SEC;
    last = now;
    return dt;
}
