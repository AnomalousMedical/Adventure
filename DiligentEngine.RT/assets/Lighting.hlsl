SurfaceReflectanceInfo GetSurfaceReflectance(   //int Workflow, //Not including workflow option, make separate function if needed
    float3     BaseColor,
    float4     PhysicalDesc
)
{
    float Metallic;

    //This is from the GLTF_PBR shader
    SurfaceReflectanceInfo SrfInfo;

    float3 specularColor;

    float3 f0 = float3(0.04, 0.04, 0.04);

    // Metallic and Roughness material properties are packed together
    // Roughness is stored in the 'g' channel, metallic is stored in the 'b' channel.
    // This layout intentionally reserves the 'r' channel for (optional) occlusion map data
    SrfInfo.PerceptualRoughness = PhysicalDesc.g;
    Metallic = PhysicalDesc.b;

    SrfInfo.DiffuseColor = BaseColor.rgb * (float3(1.0, 1.0, 1.0) - f0) * (1.0 - Metallic);
    specularColor = lerp(f0, BaseColor.rgb, Metallic);

    SrfInfo.PerceptualRoughness = clamp(SrfInfo.PerceptualRoughness, 0.0, 1.0);

    // Compute reflectance.
    float reflectance = max(max(specularColor.r, specularColor.g), specularColor.b);

    SrfInfo.Reflectance0 = specularColor.rgb;
    // Anything less than 2% is physically impossible and is instead considered to be shadowing. Compare to "Real-Time-Rendering" 4th editon on page 325.
    SrfInfo.Reflectance90 = clamp(reflectance * 50.0, 0.0, 1.0) * float3(1.0, 1.0, 1.0);

    return SrfInfo;
}

// The following equation models the Fresnel reflectance term of the spec equation (aka F())
// Implementation of fresnel from "An Inexpensive BRDF Model for Physically based Rendering" by Christophe Schlick
// (https://www.cs.virginia.edu/~jdl/bib/appearance/analytic%20models/schlick94b.pdf), Equation 15
float3 SchlickReflection(float VdotH, float3 Reflectance0, float3 Reflectance90)
{
    return Reflectance0 + (Reflectance90 - Reflectance0) * pow(clamp(1.0 - VdotH, 0.0, 1.0), 5.0);
}

// Visibility = G(v,l,a) / (4 * (n,v) * (n,l))
// see https://google.github.io/filament/Filament.md.html#materialsystem/specularbrdf/geometricshadowing(specularg)
float SmithGGXVisibilityCorrelated(float NdotL, float NdotV, float AlphaRoughness)
{
    float a2 = AlphaRoughness * AlphaRoughness;

    float GGXV = NdotL * sqrt(max(NdotV * NdotV * (1.0 - a2) + a2, 1e-7));
    float GGXL = NdotV * sqrt(max(NdotL * NdotL * (1.0 - a2) + a2, 1e-7));

    return 0.5 / (GGXV + GGXL);
}

AngularInfo GetAngularInfo(float3 PointToLight, float3 Normal, float3 View)
{
    float3 n = normalize(Normal);       // Outward direction of surface point
    float3 v = normalize(View);         // Direction from surface point to camera
    float3 l = normalize(PointToLight); // Direction from surface point to light
    float3 h = normalize(l + v);        // Direction of the vector between l and v

    AngularInfo info;
    info.NdotL = clamp(dot(n, l), 0.0, 1.0);
    info.NdotV = clamp(dot(n, v), 0.0, 1.0);
    info.NdotH = clamp(dot(n, h), 0.0, 1.0);
    info.LdotH = clamp(dot(l, h), 0.0, 1.0);
    info.VdotH = clamp(dot(v, h), 0.0, 1.0);

    return info;
}

// The following equation(s) model the distribution of microfacet normals across the area being drawn (aka D())
// Implementation from "Average Irregularity Representation of a Roughened Surface for Ray Reflection" by T. S. Trowbridge, and K. P. Reitz
// Follows the distribution function recommended in the SIGGRAPH 2013 course notes from EPIC Games [1], Equation 3.
float NormalDistribution_GGX(float NdotH, float AlphaRoughness)
{
    float a2 = AlphaRoughness * AlphaRoughness;
    float f = NdotH * NdotH * (a2 - 1.0) + 1.0;
    return a2 / (PI * f * f);
}

void BRDF(in float3                 PointToLight,
    in float3                 Normal,
    in float3                 View,
    in SurfaceReflectanceInfo SrfInfo,
    out float3                SpecContrib,
    out float                 NdotL)
{
    AngularInfo angularInfo = GetAngularInfo(PointToLight, Normal, View);

    SpecContrib = float3(0.0, 0.0, 0.0);
    NdotL = angularInfo.NdotL;
    // If one of the dot products is larger than zero, no division by zero can happen. Avoids black borders.
    if (angularInfo.NdotL > 0.0 || angularInfo.NdotV > 0.0)
    {
        //           D(h,a) * G(v,l,a) * F(v,h,f0)
        // f(v,l) = -------------------------------- = D(h,a) * Vis(v,l,a) * F(v,h,f0)
        //               4 * (n,v) * (n,l)
        // where
        //
        // Vis(v,l,a) = G(v,l,a) / (4 * (n,v) * (n,l))

        // It is not a mistake that AlphaRoughness = PerceptualRoughness ^ 2 and that
        // SmithGGXVisibilityCorrelated and NormalDistribution_GGX then use a2 = AlphaRoughness ^ 2.
        // See eq. 3 in https://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf
        float AlphaRoughness = SrfInfo.PerceptualRoughness * SrfInfo.PerceptualRoughness;
        float  D = NormalDistribution_GGX(angularInfo.NdotH, AlphaRoughness);
        float  Vis = SmithGGXVisibilityCorrelated(angularInfo.NdotL, angularInfo.NdotV, AlphaRoughness);
        float3 F = SchlickReflection(angularInfo.VdotH, SrfInfo.Reflectance0, SrfInfo.Reflectance90);

        SpecContrib = F * Vis * D;
    }
}

void LightingPass(inout float3 Color, float3 Pos, float3 Norm, float3 pertbNorm, uint Recursion)
{
    RayDesc ray;
    float3  col = float3(0.0, 0.0, 0.0);

    // Add a small offset to avoid self-intersections.
    ray.Origin = Pos + Norm * SMALL_OFFSET;
    ray.TMin = 0.0;

    //float3 eyeDir = normalize(g_ConstantsCB.CameraPos.xyz - Pos);

    ray.TMax = 100; //Make this configurable
    ray.Direction = normalize(Norm);
    float3 emissive = GetNearbyEmissiveLighting(ray, Recursion);

    for (int i = 0; i < g_ConstantsCB.NumActiveLights; ++i)
    {
        // Limit max ray length by distance to light source.
        ray.TMax = distance(g_ConstantsCB.LightPos[i].xyz, Pos) * 1.01;

        //Only shoot ray if we are close enough to hit the light
        if (ray.TMax < g_ConstantsCB.LightColor[i].a)
        {
            float3 rayDir = normalize(g_ConstantsCB.LightPos[i].xyz - Pos);
            float  NdotL = max(0.0, dot(pertbNorm, rayDir));
            float attenuation = 1.0f - (ray.TMax / g_ConstantsCB.LightColor[i].a);

            // Optimization - don't trace rays if NdotL is zero or negative
            if (NdotL > 0.0)
            {
                // Cast multiple rays that are distributed within a cone.
                ray.Direction = rayDir;
                float shading = saturate(CastShadow(ray, Recursion).Shading);

                col += Color * (g_ConstantsCB.LightColor[i].rgb * attenuation) * NdotL * shading;
                //These commented lines and the eyeDir above give crappy specular highlights
                //float3 halfVec = normalize(eyeDir + rayDir);
                //float specularLight = pow(saturate(dot(pertbNorm, halfVec)), 250);
                //col += specularLight;
            }
        }
        col += Color * g_ConstantsCB.Darkness;
    }
    Color = col * (1.0 / float(g_ConstantsCB.NumActiveLights)) + g_ConstantsCB.AmbientColor.rgb + emissive;
}

void LightingPass(inout float3 Color, float3 Pos, float3 Norm, float3 pertbNorm, uint Recursion, float4 physicalInfo)
{
    RayDesc ray;
    float3  col = float3(0.0, 0.0, 0.0);

    // Add a small offset to avoid self-intersections.
    ray.Origin = Pos + Norm * SMALL_OFFSET;
    ray.TMin = 0.0;

    float3 view = g_ConstantsCB.CameraPos.xyz - Pos;
    SurfaceReflectanceInfo surfInfo = GetSurfaceReflectance(Color, physicalInfo);

    ray.TMax = 100; //Make this configurable
    ray.Direction = normalize(Norm);
    float3 emissive = GetNearbyEmissiveLighting(ray, Recursion);

    for (int i = 0; i < g_ConstantsCB.NumActiveLights; ++i)
    {
        // Limit max ray length by distance to light source.
        ray.TMax = distance(g_ConstantsCB.LightPos[i].xyz, Pos) * 1.01;

        //Only shoot ray if we are close enough to hit the light
        if (ray.TMax < g_ConstantsCB.LightColor[i].a)
        {
            float3 rayDir = normalize(g_ConstantsCB.LightPos[i].xyz - Pos);
            float  NdotL;// = max(0.0, dot(pertbNorm, rayDir));
            float attenuation = 1.0f - (ray.TMax / g_ConstantsCB.LightColor[i].a);
            float3 SpecContrib;
            BRDF(rayDir, pertbNorm, view, surfInfo,
                SpecContrib, NdotL);

            // Optimization - don't trace rays if NdotL is zero or negative
            if (NdotL > 0.0)
            {
                // Cast multiple rays that are distributed within a cone.
                ray.Direction = rayDir;
                float shading = saturate(CastShadow(ray, Recursion).Shading);

                col += (Color + SpecContrib) * (g_ConstantsCB.LightColor[i].rgb * attenuation) * NdotL * shading;
            }
        }
        col += Color * g_ConstantsCB.Darkness;
    }
    Color = col * (1.0 / float(g_ConstantsCB.NumActiveLights)) + g_ConstantsCB.AmbientColor.rgb + emissive;
}

float3 GetPerterbedNormal(
    float3 barycentrics, out float3 normal,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    float3 sampledNormal
)
{
    // Calculate vertex tangent.
    float3 tangent = posX.tangent.xyz * barycentrics.x +
                     posY.tangent.xyz * barycentrics.y +
                     posZ.tangent.xyz * barycentrics.z;

    // Calculate vertex binormal.
    float3 binormal = posX.binormal.xyz * barycentrics.x +
                      posY.binormal.xyz * barycentrics.y +
                      posZ.binormal.xyz * barycentrics.z;

    // Calculate vertex normal.
    normal = posX.normal.xyz * barycentrics.x +
             posY.normal.xyz * barycentrics.y +
             posZ.normal.xyz * barycentrics.z;

    //Get Mapped normal
    float3 pertNormal = sampledNormal * float3(2.0, 2.0, 2.0) - float3(1.0, 1.0, 1.0);
    float3x3 tbn = MatrixFromRows(tangent, binormal, normal);
    pertNormal = normalize(mul(pertNormal, tbn)); //Can probably skip this normalize

    //Convert to world space
    normal = normalize(mul((float3x3) ObjectToWorld3x4(), normal));
    pertNormal = normalize(mul((float3x3) ObjectToWorld3x4(), pertNormal));

    return pertNormal;
}

void LightAndShadeBase
(
    inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    float3 baseColor
)
{
    payload.Depth = RayTCurrent();

    // Calculate vertex normal.
    float3 normal = posX.normal.xyz * barycentrics.x +
        posY.normal.xyz * barycentrics.y +
        posZ.normal.xyz * barycentrics.z;

    //Convert to world space
    normal = normalize(mul((float3x3) ObjectToWorld3x4(), normal));

    // Sample texturing.
    payload.Color = baseColor;

    // Apply lighting.
    float3 rayOrigin = WorldRayOrigin() + WorldRayDirection() * RayTCurrent();
    LightingPass(payload.Color, rayOrigin, normal, normal, payload.Recursion + 1);
}

void LightAndShadeBaseNormal
(
    inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    float3 baseColor, float3 sampleNormal
)
{
    payload.Depth = RayTCurrent();

    // Calculate vertex normal.
    float3 normal;
    float3 pertNormal = GetPerterbedNormal(barycentrics, normal,
        posX, posY, posZ,
        sampleNormal);

    // Sample texturing.
    payload.Color = baseColor;

    // Apply lighting.
    float3 rayOrigin = WorldRayOrigin() + WorldRayDirection() * RayTCurrent();
    LightingPass(payload.Color, rayOrigin, normal, pertNormal, payload.Recursion + 1);
}

void LightAndShadeBaseNormalPhysical
(
    inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    float3 baseColor, float3 sampleNormal, float4 physical
)
{
    payload.Depth = RayTCurrent();

    // Calculate vertex normal.
    float3 normal;
    float3 pertNormal = GetPerterbedNormal(barycentrics, normal,
        posX, posY, posZ,
        sampleNormal);

    // Sample texturing.
    payload.Color = baseColor;

    // Apply lighting.
    float3 rayOrigin = WorldRayOrigin() + WorldRayDirection() * RayTCurrent();
    LightingPass(payload.Color, rayOrigin, normal, pertNormal, payload.Recursion + 1, physical);
}

void LightAndShadeBaseNormalPhysicalReflective
(
    inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    float3 baseColor, float3 sampleNormal, float4 physical
)
{
    payload.Depth = RayTCurrent();

    // Calculate vertex normal.
    float3 normal;
    float3 pertNormal = GetPerterbedNormal(barycentrics, normal,
        posX, posY, posZ,
        sampleNormal);

    float roughness = physical.g;
    float reflective = physical.a;

    if (reflective > 0.5)
    {
        // Reflect from the normal
        RayDesc ray;
        ray.Origin = WorldRayOrigin() + WorldRayDirection() * RayTCurrent() + normal * SMALL_OFFSET;
        ray.TMin = 0.0;
        ray.TMax = 100.0;
        ray.Direction = reflect(WorldRayDirection(), pertNormal);
        float3 reflectedColor = CastPrimaryRay(ray, payload.Recursion + 1).Color;

        // Calculate final color
        payload.Color = baseColor * roughness + reflectedColor * (1.0f - roughness);
    }
    else 
    {
        payload.Color = baseColor;
    }

    // Apply lighting.
    float3 rayOrigin = WorldRayOrigin() + WorldRayDirection() * RayTCurrent();
    LightingPass(payload.Color, rayOrigin, normal, pertNormal, payload.Recursion + 1, physical);

    payload.Depth = RayTCurrent();
}

void LightDispatch
(
    inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    int mip, float2 uv, SamplerState sState
)
{
    [forcecase] switch (instanceData.lightingType)
    {
        case $$(LIGHTANDSHADEBASE):
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, sState)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMAL):
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, sState),
                GetSampledNormal(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL):
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, sState),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE):
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, sState),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            break;
    }

    #if HAS_EMISSIVE_MAP
    payload.Color += GetEmissive(mip, uv, sState);
    #endif
}

void LightMesh
(
    inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    int mip, float2 uv
)
{
    LightDispatch(payload, barycentrics, posX, posY, posZ, mip, uv, g_SamLinearWrap);
}

void LightSprite
(
    inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ,
    int mip, float2 uv
) 
{
    LightDispatch(payload, barycentrics, posX, posY, posZ, mip, uv, g_SamPointWrap);
}