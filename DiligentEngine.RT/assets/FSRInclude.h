#define A_GPU 1
#define A_HLSL 1
#include "ffx_a.h"

#define FSR_EASU_F 1
#define FSR_RCAS_F 1
#include "ffx_fsr1.h"

cbuffer FSRConstants
{
    uint2 inputSize;
    uint2 outSize;
};