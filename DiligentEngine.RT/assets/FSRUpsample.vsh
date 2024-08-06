#include "FSRInclude.h"

struct PSInput 
{ 
    float4 Pos : SV_POSITION; 
    float2 UV : TEX_COORD0;
    AU4 con0 : TEX_COORD1;
    AU4 con1 : TEX_COORD2;
    AU4 con2 : TEX_COORD3;
    AU4 con3 : TEX_COORD4;
};

void main(in uint vid : SV_VertexID,
          out PSInput PSIn) 
{
    PSIn.UV  = float2(vid & 1, vid >> 1);
    PSIn.Pos = float4(PSIn.UV * 2.0 - 1.0, 0.0, 1.0);

    FsrEasuCon(PSIn.con0, PSIn.con1, PSIn.con2, PSIn.con3,
        inputSize.x, inputSize.y,  // Viewport size (top left aligned) in the input image which is to be scaled.
        inputSize.x, inputSize.y,  // The size of the input image.
        outSize.x, outSize.y); // The output resolution.
}
