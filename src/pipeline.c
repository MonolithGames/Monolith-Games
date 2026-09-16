#include "pipeline.h"
#include "config.h"
#include <stdio.h>
#include <string.h>
#include <time.h>

static const char* templateNames[MONOLITH_TEMPLATE_COUNT] = {
    "Skyline Runner | 3D action template",
    "Neon Kart | Multiplayer racing template",
    "Pocket Planet | Casual mobile template"
};

static const char* stageNames[] = {
    "Ready",
    "Validating template",
    "Azure worker adapter",
    "Maya asset adapter",
    "Unity build adapter",
    "Quality checks",
    "Media package",
    "Publishing approval",
    "Complete",
    "Failed"
};

static PipelineStage currentStage = PIPELINE_IDLE;
static int currentTemplate = -1;
static FILE* jobLog = NULL;
static char statusText[160] = "Ready - select a template to begin";
static MonolithConfig config;
static int configLoaded = 0;

static void WriteLog(const char* message)
{
    if (jobLog)
    {
        fprintf(jobLog, "%s\n", message);
        fflush(jobLog);
    }
}

static void WriteManifest(void)
{
    FILE* manifest = fopen("build/monolith_job.json", "w");

    if (!manifest)
        return;

    fprintf(manifest,
            "{\n"
            "  \"product\": \"Monolith\",\n"
            "  \"template\": \"%s\",\n"
            "  \"status\": \"pipeline-started\",\n"
            "  \"azure\": \"adapter-pending\",\n"
            "  \"maya\": \"adapter-pending\",\n"
            "  \"unity\": \"adapter-pending\",\n"
            "  \"publishing\": \"approval-required\"\n"
            "}\n",
            Pipeline_TemplateName(currentTemplate));
    fclose(manifest);
}

static void FailPipeline(const char* message)
{
    currentStage = PIPELINE_FAILED;
    snprintf(statusText, sizeof(statusText), "Pipeline failed: %s", message);
    WriteLog(statusText);
    if (jobLog)
    {
        fclose(jobLog);
        jobLog = NULL;
    }
}

const char* Pipeline_TemplateName(int index)
{
    if (index < 0 || index >= MONOLITH_TEMPLATE_COUNT)
        return NULL;

    return templateNames[index];
}

int Pipeline_Start(int templateIndex)
{
    char manifest[256];
    time_t now;

    if (!Pipeline_TemplateName(templateIndex) || Pipeline_IsRunning())
        return 0;

    currentTemplate = templateIndex;
    if (!configLoaded)
    {
        Config_Load(&config);
        configLoaded = 1;
    }
    currentStage = PIPELINE_VALIDATING;
    strcpy(statusText, "Validating selected template");

    jobLog = fopen("build/monolith_job.log", "a");
    if (!jobLog)
    {
        currentStage = PIPELINE_FAILED;
        strcpy(statusText, "Could not open build/monolith_job.log");
        return 0;
    }

    now = time(NULL);
    fprintf(jobLog, "\nMonolith job started: %s", ctime(&now));
    fprintf(jobLog, "Template: %s\n", Pipeline_TemplateName(currentTemplate));
    WriteManifest();
    strcpy(manifest, "Job manifest created for template-driven production");
    WriteLog(manifest);
    WriteLog("Adapter configuration loaded from MONOLITH_*_COMMAND variables");
    return 1;
}

int Pipeline_Tick(void)
{
    if (!Pipeline_IsRunning())
        return 0;

    if (currentStage < PIPELINE_PUBLISH)
    {
        currentStage = (PipelineStage)(currentStage + 1);
        if ((currentStage == PIPELINE_AZURE || currentStage == PIPELINE_MAYA ||
            currentStage == PIPELINE_UNITY || currentStage == PIPELINE_MEDIA ||
             currentStage == PIPELINE_PUBLISH) &&
            !Config_IsConfigured(&config, currentStage))
        {
            snprintf(statusText, sizeof(statusText),
                     "%s not configured: set MONOLITH_*_COMMAND",
                     stageNames[currentStage]);
            WriteLog(statusText);
            return 1;
        }
        snprintf(statusText, sizeof(statusText), "%s: %s",
                 stageNames[currentStage], Pipeline_TemplateName(currentTemplate));
        WriteLog(statusText);
        return 1;
    }

    currentStage = PIPELINE_COMPLETE;
    strcpy(statusText, "Pipeline complete: review artifacts before publishing");
    WriteLog(statusText);
    if (jobLog)
    {
        fclose(jobLog);
        jobLog = NULL;
    }
    return 1;
}
            if (currentStage == PIPELINE_AZURE || currentStage == PIPELINE_MAYA ||
                currentStage == PIPELINE_UNITY || currentStage == PIPELINE_MEDIA ||
                currentStage == PIPELINE_PUBLISH)
}
                if (!Config_IsConfigured(&config, currentStage))
                {
                    FailPipeline("adapter command not configured");
                    return 1;
                }

                if (!Config_RunStage(&config, currentStage, jobLog))
                {
                    FailPipeline(stageNames[currentStage]);
                    return 1;
                }
}

const char* Pipeline_GetStatus(void)
{
    return statusText;
}
