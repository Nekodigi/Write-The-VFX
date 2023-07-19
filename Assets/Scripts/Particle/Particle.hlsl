#ifndef PARTICLE
#define PARTICLE

struct Particle
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
    int disable;//0=enable, 1 disable 2=warped
};

#ifndef PARTICLE_R_ONLY
RWStructuredBuffer<Particle>  _ParticleBuffer;
#else
StructuredBuffer<Particle>  _ParticleBuffer;
#endif

float3 _PosMax;
float3 _PosMin;
#endif