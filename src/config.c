#include "config.h"
#include <stdlib.h>

static const char* ReadCommand(const char* name)
{
    const char* value = getenv(name);
    return value && value[0] ? value : NULL;
}

void Config_Load(MonolithConfig* config)
{
    if (!config)
        return;

    config->azureCommand = ReadCommand("MONOLITH_AZURE_COMMAND");
    config->mayaCommand = ReadCommand("MONOLITH_MAYA_COMMAND");
    config->unityCommand = ReadCommand("MONOLITH_UNITY_COMMAND");
    config->mediaCommand = ReadCommand("MONOLITH_MEDIA_COMMAND");
    config->publishCommand = ReadCommand("MONOLITH_PUBLISH_COMMAND");
}

const char* Config_CommandForStage(const MonolithConfig* config, int stage)
{
    if (!config)
        return NULL;

    switch (stage)
    {
    case 2: return config->azureCommand;
    case 3: return config->mayaCommand;
    case 4: return config->unityCommand;
    case 6: return config->mediaCommand;
    case 7: return config->publishCommand;
    default: return NULL;
    }
}

int Config_IsConfigured(const MonolithConfig* config, int stage)
{
    return Config_CommandForStage(config, stage) != NULL;
}

int Config_RunStage(const MonolithConfig* config, int stage, FILE* log)
{
    const char* command = Config_CommandForStage(config, stage);
    int result;

    if (!command)
        return 0;

    if (log)
        fprintf(log, "Executing stage %d: %s\n", stage, command);

    result = system(command);
    if (log)
        fprintf(log, "Stage %d exit code: %d\n", stage, result);

    return result == 0;
}
