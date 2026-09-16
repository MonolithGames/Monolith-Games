#ifndef MONOLITH_CONFIG_H
#define MONOLITH_CONFIG_H

#include <stdio.h>

typedef struct
{
    const char* azureCommand;
    const char* mayaCommand;
    const char* unityCommand;
    const char* mediaCommand;
    const char* publishCommand;
} MonolithConfig;

void Config_Load(MonolithConfig* config);
const char* Config_CommandForStage(const MonolithConfig* config, int stage);
int Config_IsConfigured(const MonolithConfig* config, int stage);
int Config_RunStage(const MonolithConfig* config, int stage, FILE* log);

#endif
