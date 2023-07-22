#ifndef PARTICLE
#define PARTICLE

#include "Assets/Scripts/Utils/Sampler/Sampler.hlsl"
#include "Assets/Scripts/Utils/Time/Time.hlsl"
#include "Assets/Scripts/Value/Random.cginc"
#include "Assets/Scripts/Utils/Consts/Consts.hlsl"


struct Particle
{
    float3 pos;
    float3 vel;
    float3 rot;
    float3 rotVel;
    float3 size;
    float3 sizeDest;
    float4 col;
    float4 colDest;
    float4 customData;
    float lifeTime;
    float spawnTime;
    int disable;//0=enable, 1 disable 2=warped
};


Texture2D<float4> _PLife;
Texture2D<float4> _PCol;

Texture2D<float4> _PVelOverLife;
Texture2D<float4> _PRotVelOverLife;
Texture2D<float4> _PSizeOverLife;
Texture2D<float4> _PColOverLife;
Texture2D<float4> _PCustomDataOverLife;
Texture2D<float4> _PFieldOverLife;//compress float1
Texture2D<float4> _PDampOverLife;

#ifndef SHADER
RWStructuredBuffer<Particle>  _ParticleBuffer;
#else
StructuredBuffer<Particle>  _ParticleBuffer;
#endif

float _PosRange;
float3 _PosMax;
float3 _PosMin;
float _VelRange;
float3 _VelMin;
float3 _VelMax;
float3 _SizeMin;
float3 _SizeMax;

float getAge(Particle p){
    return _Time - p.spawnTime;
}
float getRate(Particle p){
    return getAge(p)/p.lifeTime;
}

float3 getVelAt(float rate){
    return _PVelOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).xyz;
}
float3 getRotVelAt(float rate){
    return _PRotVelOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).xyz;
}
float3 getSizeAt(float rate){
    return _PSizeOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).xyz;
}
float4 getColAt(float rate){
    return _PColOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0);
}
float4 getCustomDataAt(float rate){
    return _PCustomDataOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0);
}
float geFieldAt(float rate){
    return _PFieldOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).x;
}
float geDampAt(float rate){
    return _PDampOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).x;
}

Particle initParticle(uint3 id){
    Particle p = _ParticleBuffer[id.x];
    if(_PosRange == 0){
        p.pos = randomBetween(id.xxx, _PosMin, _PosMax);
    }else{
        p.pos = randomSphere(id.xx, float3(0, 0, 0), _PosRange);
    }
    if(_VelRange == 0){
        p.vel = randomBetween(id.xxx, _VelMin, _VelMax);
    }else{
        p.vel = randomSphere(id.xx, float3(0, 0, 0), _VelRange);
    }
    p.rot = randomBetween(id.xxx, float(0).xxx, float(2*PI).xxx);
    p.rotVel = randomBetween(id.xxx, float(0).xxx, float(0.5).xxx);
    p.size = randomBetween(id.xxx, _SizeMin, _SizeMax);
    p.spawnTime = _Time;
    p.lifeTime = _PLife.SampleLevel(linearClampSampler, random2(id.xx+0.01), 0).x;
    p.col = _PCol.SampleLevel(linearClampSampler, random2(id.xx), 0);
    p.disable = 2;
    return p;
}

Particle updateParticle(Particle p, uint3 id){
    float rate = getRate(p);
    p.vel *= (1-geDampAt(rate));

    float3 vel = p.vel;
    vel += getVelAt(rate);
    
    p.pos += vel * _DeltaTime;
    p.rot += p.rotVel * _DeltaTime;

    p.colDest = p.col * getColAt(rate);
    p.sizeDest = p.size * getSizeAt(rate);
    p.customData = getCustomDataAt(rate);

    if(p.disable == 2)p.disable = 0;
    if(p.lifeTime < getAge(p))p = initParticle(id.x);

    return p;
}


#endif