#ifndef RANDOM_INCLUDED
#define RANDOM_INCLUDED

float random(float2 seeds)
{
    return frac(sin(dot(seeds, float2(12.9898, 78.233))) * 43758.5453);
}

float2 random2(float2 seeds)
{
    seeds = float2(dot(seeds, float2(12.9898, 78.233)),
                   dot(seeds, float2(269.5, 183.3)));

    return frac(sin(seeds) * 43758.5453123);
}

float3 random3(float3 seeds)
{
    seeds = float3(dot(seeds, float3(12.9898, 78.233, 542.3)),
                   dot(seeds, float3(269.5, 183.3, 461.7)),
                   dot(seeds, float3(732.1, 845.3, 231.7)));

    return frac(sin(seeds) * 43758.5453123);
}

#endif // RANDOM_INCLUDED