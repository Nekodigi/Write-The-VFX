#ifndef RANDOM_INCLUDED
#define RANDOM_INCLUDED

#include "Assets/Scripts/Utils/Consts/Consts.hlsl"

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
    seeds = float2(dot(seeds, float2(12.9898, 78.233)),
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

float3 randomBetween(float3 seeds, float3 min, float3 max){
    return min + random3(seeds)*(max-min);
}

//ADD VOLUMETRIC
float3 uniformSphere(float2 cord) {
  float ang1 = (cord.x + 1.0) * PI; // [-1..1) -> [0..2*PI)
  float u = cord.y; // [-1..1), cos and acos(2v-1) cancel each other out, so we arrive at [-1..1)
  float u2 = u * u;
  float sqrt1MinusU2 = sqrt(1.0 - u2);
  float x = sqrt1MinusU2 * cos(ang1);
  float y = sqrt1MinusU2 * sin(ang1);
  float z = u;
  return float3(x, y, z);
}
float3 randomSphere(float2 seeds, float3 center, float r){
    return center+uniformSphere(random2(seeds)*2-1)*r;
}
float3 randomSphere(float2 seeds){
    return randomSphere(seeds, float3(0,0,0), 1);
}

#endif // RANDOM_INCLUDED