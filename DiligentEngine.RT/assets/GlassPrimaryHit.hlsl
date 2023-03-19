// Simulate light absorption inside glass.
float3 LightAbsorption(float3 color1, float depth)
{
    float  factor1 = depth * 0.25;
    float  factor2 = pow(depth * g_ConstantsCB.GlassAbsorption, 2.2) * 0.25;
    float  factor  = clamp(factor1 + factor2 + 0.05, 0.0, 1.0); 
    float3 color2  = color1 * g_ConstantsCB.GlassMaterialColor.rgb;
    return lerp(color1, color2, factor);
}

float3 BlendWithReflection(float3 srcColor, float3 reflectionColor, float factor)
{
    return lerp(srcColor, reflectionColor * g_ConstantsCB.GlassReflectionColorMask.rgb, factor);
}

// Optimized fresnel calculation.
float Fresnel(float eta, float cosThetaI)
{
    cosThetaI = clamp(cosThetaI, -1.0, 1.0);
    if (cosThetaI < 0.0)
    {
        eta = 1.0 / eta;
        cosThetaI = -cosThetaI;
    }

    float sinThetaTSq = eta * eta * (1.0 - cosThetaI * cosThetaI);
    if (sinThetaTSq > 1.0)
        return 1.0;

    float cosThetaT = sqrt(1.0 - sinThetaTSq);

    float Rs = (eta * cosThetaI - cosThetaT) / (eta * cosThetaI + cosThetaT);
    float Rp = (eta * cosThetaT - cosThetaI) / (eta * cosThetaT + cosThetaI);

    return 0.5 * (Rs * Rs + Rp * Rp);
}

//void GlassHit(inout PrimaryRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
void Glass(inout PrimaryRayPayload payload, float3 barycentrics,
    CubeAttribVertex posX, CubeAttribVertex posY, CubeAttribVertex posZ)
{
    float3 normal = posX.normal.xyz * barycentrics.x +
        posY.normal.xyz * barycentrics.y +
        posZ.normal.xyz * barycentrics.z;

    normal        = normalize(mul((float3x3) ObjectToWorld3x4(), normal));
    
    // Air index of refraction
    const float  AirIOR      = 1.0;
    float3       resultColor = float3(0.0, 0.0, 0.0);

    RayDesc ray;
    ray.Direction = WorldRayDirection();
    ray.TMin      = SMALL_OFFSET;
    ray.TMax      = 100.0;
      
    float3 rayDir = WorldRayDirection();
    float  relIOR = 1.0;

    // Refraction at the interface between air and glass.
    if (HitKind() == HIT_KIND_TRIANGLE_FRONT_FACE)
    {
        relIOR = AirIOR / g_ConstantsCB.GlassIndexOfRefraction.x;
        rayDir = refract(rayDir, normal, relIOR);
    }
    // Refraction at the interface between glass and air.
    else if (HitKind() == HIT_KIND_TRIANGLE_BACK_FACE)
    {
        relIOR = g_ConstantsCB.GlassIndexOfRefraction.x / AirIOR;
        normal = -normal;
        rayDir = refract(rayDir, normal, relIOR);
    }
        
    float  fresnel = Fresnel(relIOR, dot(WorldRayDirection(), -normal));
    float3 reflColor;
        
    // Reflection
    {
        ray.Origin    = WorldRayOrigin() + WorldRayDirection() * RayTCurrent() + normal * SMALL_OFFSET;
        ray.Direction = reflect(WorldRayDirection(), normal);

        PrimaryRayPayload reflPayload = CastPrimaryRay(ray, payload.Recursion + 1);
        reflColor = reflPayload.Color;
            
        if (HitKind() == HIT_KIND_TRIANGLE_BACK_FACE)
        {
            reflColor = LightAbsorption(reflColor, reflPayload.Depth);
        }
    }
        
    // Refraction
    if (fresnel < 1.0)
    {
        ray.Origin    = WorldRayOrigin() + WorldRayDirection() * RayTCurrent();
        ray.Direction = rayDir;

        PrimaryRayPayload nextPayload = CastPrimaryRay(ray, payload.Recursion + 1);
        resultColor = nextPayload.Color;
            
        if (HitKind() == HIT_KIND_TRIANGLE_FRONT_FACE || payload.Recursion == 0)
        {
            resultColor = LightAbsorption(resultColor, nextPayload.Depth);
        }
    }
        
    resultColor = BlendWithReflection(resultColor, reflColor, fresnel);

    payload.Color = resultColor;
    payload.Depth = RayTCurrent();
}
