#include "FSRInclude.h"

struct PSInput 
{ 
    float4 Pos : SV_POSITION; 
    float2 UV : TEX_COORD0;
    AU4 con0 : TEX_COORD1;
};

void main(in uint vid : SV_VertexID,
          out PSInput PSIn) 
{
    PSIn.UV  = float2(vid & 1, vid >> 1);
    PSIn.Pos = float4(PSIn.UV * 2.0 - 1.0, 0.0, 1.0);

    float fsrSharpening = 0.0;
    FsrRcasCon(PSIn.con0, fsrSharpening);
}
