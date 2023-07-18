#include "Assets/Scripts/Particle/Particle.hlsl"

void LoopBoundary (int id)
{
    particle p = _ParticleBuffer[id];
    if (p.pos.x > _PosMax.x)p.pos.x = _PosMin.x;
    if (p.pos.x < _PosMin.x)p.pos.x = _PosMax.x;
    if (p.pos.y > _PosMax.y)p.pos.y = _PosMin.y;
    if (p.pos.y < _PosMin.y)p.pos.y = _PosMax.y;
    if (p.pos.z > _PosMax.z)p.pos.z = _PosMin.z;
    if (p.pos.z < _PosMin.z)p.pos.z = _PosMax.z;
    _ParticleBuffer[id] = p;
}
