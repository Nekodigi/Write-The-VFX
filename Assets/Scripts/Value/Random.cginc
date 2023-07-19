#ifndef RANDOM_INCLUDED
#define RANDOM_INCLUDED

// Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
uint Hash(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float random(uint seed)
{
    return float(Hash(seed)) / 4294967295.0; // 2^32-1
}

float conflinctingRandom(float2 seeds)
{
    return frac(sin(dot(seeds, float2(12.9898, 78.233))) * 43758.5453);
}

float2 random2(float2 seeds)
{
    seeds = float2(dot(seeds, float2(12.9898, 78.233))+random(seeds.x),
                   dot(seeds, float2(269.5, 183.3)));

    return frac(sin(seeds) * 43758.5453123+float2(random(seeds.x), random(seeds.y)));
}

float3 random3(float3 seeds)
{
    seeds = float3(dot(seeds, float3(12.9898, 78.233, 542.3)),
                   dot(seeds, float3(269.5, 183.3, 461.7)),
                   dot(seeds, float3(732.1, 845.3, 231.7)));

    return frac(sin(seeds) * 43758.5453123+float3(random(seeds.x), random(seeds.y), random(seeds.z)));
}

#endif // RANDOM_INCLUDED