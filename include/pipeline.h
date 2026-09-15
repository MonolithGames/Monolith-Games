#ifndef PIPELINE_H
#define PIPELINE_H

#define MONOLITH_TEMPLATE_COUNT 3

typedef enum
{
    PIPELINE_IDLE,
    PIPELINE_VALIDATING,
    PIPELINE_AZURE,
    PIPELINE_MAYA,
    PIPELINE_UNITY,
    PIPELINE_QA,
    PIPELINE_MEDIA,
    PIPELINE_PUBLISH,
    PIPELINE_COMPLETE,
    PIPELINE_FAILED
} PipelineStage;

const char* Pipeline_TemplateName(int index);
int Pipeline_Start(int templateIndex);
int Pipeline_Tick(void);
int Pipeline_IsRunning(void);
PipelineStage Pipeline_GetStage(void);
const char* Pipeline_GetStatus(void);

#endif
