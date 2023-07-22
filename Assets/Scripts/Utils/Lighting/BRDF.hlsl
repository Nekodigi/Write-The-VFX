//https://github.com/XJINE/UnityShaderProgramming_03_Sample
#ifndef BRDF_INCLUDED
#define BRDF_INCLUDED

#include "Assets/Scripts/Utils/Consts/Consts.hlsl"

float dTermBeckmann(float dotNH, float roughness)
{
    dotNH = dotNH * dotNH;
    roughness = roughness * roughness;

    return exp((dotNH - 1) / (roughness * dotNH)) / (PI * roughness * dotNH * dotNH);
}

float dTermGGX(float dotNH, float roughness)
{
    dotNH = dotNH * dotNH;
    roughness = roughness * roughness;

    return roughness / (PI * pow(dotNH * (roughness - 1) + 1, 2));
}

float gTermTorrance(float dotNL, float dotNV, float dotNH, float dotVH)
{
    return min(1, min(2 * dotNH * dotNV / dotVH,
                      2 * dotNH * dotNL / dotVH));
}

float gTermSmithSchlick(float dotNL, float dotNV, float roughness)
{
    float k = roughness * sqrt(2 / PI);
    float l = dotNL / (dotNL * (1 - k) + k);
    float v = dotNV / (dotNV * (1 - k) + k);

    return saturate(l) * saturate(v);
}

float gTermSmithJoint(float dotNL, float dotNV, float roughness)
{
    roughness = roughness * roughness;
    dotNL = dotNL * dotNL;
    dotNV = dotNV * dotNV;

    float l = (-1 + sqrt(1 + roughness * (1 / dotNL - 1))) * 0.5;
    float v = (-1 + sqrt(1 + roughness * (1 / dotNV - 1))) * 0.5;

    return 1 / (1 + l + v);
}

float gTermSmithBeckmann(float dot, float roughness)
{
    float c = dot / (roughness * sqrt(1 - dot * dot));
    return c < 1.6 ? (3.535 * c + 2.181 * c * c) / (1 + 2.276 * c + 2.577 * c * c) : 1;
}

float gTermSmithBeckmann(float dotNL, float dotNV, float roughness)
{
    return gTermSmithBeckmann(dotNL, roughness) * gTermSmithBeckmann(dotNV, roughness);
}

float fresnelSchlick(float dotNV, float fresnel)
{
    return saturate(fresnel + (1 - fresnel) * pow(1 - dotNV, 5));
}

float fresnelFast(float dotNV, float fresnel)
{
    return saturate(fresnel + (1 - fresnel) * exp(-6 * dotNV));
}

#endif // BRDF_INCLUDED