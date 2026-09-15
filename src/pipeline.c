#include "pipeline.h"
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
    WriteLog("External adapters are staged for configuration");
    return 1;
}

int Pipeline_Tick(void)
{
    if (!Pipeline_IsRunning())
        return 0;

    if (currentStage < PIPELINE_PUBLISH)
    {
        currentStage = (PipelineStage)(currentStage + 1);
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

int Pipeline_IsRunning(void)
{
    return currentStage >= PIPELINE_VALIDATING && currentStage <= PIPELINE_PUBLISH;
}

PipelineStage Pipeline_GetStage(void)
{
    return currentStage;
}

const char* Pipeline_GetStatus(void)
{
    return statusText;
}
