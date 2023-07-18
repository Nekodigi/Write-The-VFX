#ifndef PARTICLE
#define PARTICLE

struct particle
{
    float3 pos;
    float3 vel;
    float3 rot;
    float3 rotVel;
    float3 size;
    float4 col;
    float4 customData;
    float lifeTime;
    float spawnTime;
    int enable;
};

#ifndef PARTICLE_R_ONLY
RWStructuredBuffer<particle>  _ParticleBuffer;
#else
StructuredBuffer<particle>  _ParticleBuffer;
#endif

float3 _PosMax;
float3 _PosMin;
#endif