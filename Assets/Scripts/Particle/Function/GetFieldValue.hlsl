#include "Assets/Scripts/Field/Field.hlsl"
#include "Assets/Scripts/Particle/Particle.hlsl"


float2 GetFieldVec(int id){
    Particle p = _ParticleBuffer[id.x];
    return _SourceVec.SampleLevel(linearClampSampler, ((p.pos-_PosMin)/(_BoundMax-_BoundMin)).xz, 0);
    //p.pos += float3(p2d.x,0,p2d.y);
    //_ParticleBuffer[id.x] = p;
}