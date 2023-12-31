#ifndef PARTICLE
#define PARTICLE

#include "Assets/Scripts/Utils/Sampler/Sampler.hlsl"
#include "Assets/Scripts/Utils/Time/Time.hlsl"
#include "Assets/Scripts/Value/Random.cginc"
#include "Assets/Scripts/Utils/Consts/Consts.hlsl"
#include "Assets/Scripts/Utils/Coordinate/Coordinate.hlsl"


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
    float weight;
    float weightDest;
    float lifeTime;
    float spawnTime;
    int disable;//0=enable, 1 disable 2=warped
};


Texture2D<float4> _PLife;
Texture2D<float4> _PCol;
Texture2D<float4> _PWeight;

Texture2D<float4> _PVelOverLife;
Texture2D<float4> _PRotVelOverLife;
Texture2D<float4> _PSizeOverLife;
Texture2D<float4> _PColOverLife;
Texture2D<float4> _PWeightOverLife;
Texture2D<float4> _PCustomDataOverLife;
Texture2D<float4> _PFieldOverLife;//compress float1
Texture2D<float4> _PDampOverLife;

#ifndef SHADER
RWStructuredBuffer<Particle>  _ParticleBuffer;
RWStructuredBuffer<float3>  _PosBuffer;
RWStructuredBuffer<float3>  _VelBuffer;
RWStructuredBuffer<float4>  _ColBuffer;
RWStructuredBuffer<float3>  _RotBuffer;
RWStructuredBuffer<float3>  _SizeBuffer;
RWStructuredBuffer<float3>  _WeightsBuffer;//weight, field, damp
RWStructuredBuffer<float3>  _LifesBuffer;//life, age

#else
StructuredBuffer<Particle>  _ParticleBuffer;
#endif

ConsumeStructuredBuffer<uint> _PooledParticleBuffer;
AppendStructuredBuffer<uint> _DeadParticleBuffer;

float _PosRange;
float3 _PosMax;
float3 _PosMin;
float _VelRange;
float3 _VelMin;
float3 _VelMax;
float3 _SizeMin;
float3 _SizeMax;
bool _UseField;

float3 _PosOrigin;
float3 _RotOrigin;
float3 _SizeOrigin;

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
float getFieldAt(float rate){
    return _PFieldOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).x;
}
float getDampAt(float rate){
    return _PDampOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).x;
}
float getWeightAt(float rate){
    return _PWeightOverLife.SampleLevel(linearClampSampler, float2(rate, 0), 0).x;  
}

float3 normPos(Particle p){
    return (p.pos-_BoundMin)/(_BoundMax-_BoundMin);
}


Particle initParticle(int id_){
    uint3 id = uint3(id_,0,0);
    float3 offset = 0;
    Particle p = _ParticleBuffer[id.x];
    if(_PosRange == 0){
        p.pos = randomBetween(id.xxx, _PosMin, _PosMax);
    }else{
        p.pos = randomSphere(id.xx, float3(0, 0, 0), _PosRange);
    }
    if(_VelRange == 0){
        p.vel = randomBetween(id.xxx+offset, _VelMin, _VelMax);offset+=1.23;
    }else{
        p.vel = randomSphere(id.xx, float3(0, 0, 0), _VelRange);
    }
    p.rot = randomBetween(id.xxx+offset, float(0).xxx, float(2*PI).xxx);offset+=1.23;
    p.rotVel = randomBetween(id.xxx+offset, float(0).xxx, float(0.5).xxx);offset+=1.23;
    p.size = randomBetween(id.xxx+offset, _SizeMin, _SizeMax);offset+=1.23;
    p.spawnTime = _Time;
    p.lifeTime = _PLife.SampleLevel(linearClampSampler, random2(id.xx+offset), 0).x;offset+=1.23;
    p.weight = _PWeight.SampleLevel(linearClampSampler, random2(id.xx+offset), 0).x;    offset+=1.23;
    p.col = _PCol.SampleLevel(linearClampSampler, random2(id.xx+offset), 0);offset+=1.23;
    p.disable = 2;

    p.pos += _PosOrigin;
    p.vel = rotCoord(p.vel.xyzz, degToRad(_RotOrigin)).xyz;
    return p;
}

Particle deleteParticle(int id){
    _DeadParticleBuffer.Append(id);
    Particle p = (Particle)0;
    p.disable = 1;
    return p;
}

Particle updateParticle(Particle p, uint3 id){
    if(p.disable == 2)p.disable = 0;
    float rate = getRate(p);
    p.vel *= (1-getDampAt(rate));

    float3 vel = p.vel;
    vel += getVelAt(rate);
    
    p.pos += vel * _DeltaTime;
    p.rot += p.rotVel * _DeltaTime;

    p.colDest = p.col * getColAt(rate);
    p.weightDest = p.weight * getWeightAt(rate);  
    p.sizeDest = p.size * getSizeAt(rate);
    p.customData = getCustomDataAt(rate);

    if(p.disable == 0 && p.lifeTime < getAge(p))p = deleteParticle(id.x);

    return p;
}


#endif