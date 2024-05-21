//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

// This is from ray tracing gems 2 chapter 7 https://github.com/Apress/Ray-Tracing-Gems-II

/////////// Begin ray cone functions ///////////
uint2 TexDims(Texture2D<float4> tex) { uint2 vSize; tex.GetDimensions(vSize.x, vSize.y); return vSize; }
uint2 TexDims(Texture2D<float3> tex) { uint2 vSize; tex.GetDimensions(vSize.x, vSize.y); return vSize; }
uint2 TexDims(Texture2D<float > tex) { uint2 vSize; tex.GetDimensions(vSize.x, vSize.y); return vSize; }

float2 UVAreaFromRayCone(float3 vRayDir,float3 vWorldNormal,float vRayConeWidth,float2 aUV[3],float3 aPos[3],float3x3 matWorld)
{
	float2 vUV10 = aUV[1]-aUV[0];
	float2 vUV20 = aUV[2]-aUV[0];
	float fTriUVArea = abs(vUV10.x*vUV20.y - vUV20.x*vUV10.y);

	// We need the area of the triangle, which is length(triangleNormal) in worldspace, and I
	// could not figure out a way with fewer than two 3x3 mtx multiplies for ray cones.
	float3 vEdge10 = mul(aPos[1]-aPos[0],matWorld);
	float3 vEdge20 = mul(aPos[2]-aPos[0],matWorld);

	float3 vFaceNrm = cross(vEdge10, vEdge20); // in world space, by design
	float fTriLODOffset = 0.5f * log2(fTriUVArea/length(vFaceNrm)); // Triangle-wide LOD offset value
	float fDistTerm = vRayConeWidth * vRayConeWidth;
	float fNormalTerm = dot(vRayDir, vWorldNormal);

	return float2(fTriLODOffset, fDistTerm/(fNormalTerm*fNormalTerm));
}

float UVAreaToTexLOD(uint2 vTexSize,float2 vUVAreaInfo)
{
	return vUVAreaInfo.x + 0.5f*log2(vTexSize.x * vTexSize.y * vUVAreaInfo.y);
}

float4 UVDerivsFromRayCone(float3 vRayDir,float3 vWorldNormal,float vRayConeWidth,float2 aUV[3],float3 aPos[3],float3x3 matWorld)
{
	float2 vUV10 = aUV[1]-aUV[0];
	float2 vUV20 = aUV[2]-aUV[0];
	float fQuadUVArea = abs(vUV10.x*vUV20.y - vUV20.x*vUV10.y);

	// Since the ray cone's width is in world-space, we need to compute the quad
	// area in world-space as well to enable proper ratio calculation
	float3 vEdge10 = mul(aPos[1]-aPos[0],matWorld);
	float3 vEdge20 = mul(aPos[2]-aPos[0],matWorld);
	float3 vFaceNrm = cross(vEdge10, vEdge20);
	float fQuadArea = length(vFaceNrm);

	float fDistTerm = abs(vRayConeWidth);
	float fNormalTerm = abs(dot(vRayDir,vWorldNormal));
	float fProjectedConeWidth = vRayConeWidth/fNormalTerm;
	float fVisibleAreaRatio = (fProjectedConeWidth*fProjectedConeWidth) / fQuadArea;

	float fVisibleUVArea = fQuadUVArea*fVisibleAreaRatio;
	float fULength = sqrt(fVisibleUVArea);
	return float4(fULength,0,0,fULength);
}
/////////// End ray cone functions ///////////