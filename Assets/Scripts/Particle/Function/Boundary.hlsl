#include "Assets/Scripts/Particle/Particle.hlsl"

void loopBoundary (Particle p)
{
    if (p.pos.x > _PosMax.x)p.pos.x = _PosMin.x;
    if (p.pos.x < _PosMin.x)p.pos.x = _PosMax.x;
    if (p.pos.y > _PosMax.y)p.pos.y = _PosMin.y;
    if (p.pos.y < _PosMin.y)p.pos.y = _PosMax.y;
    if (p.pos.z > _PosMax.z)p.pos.z = _PosMin.z;
    if (p.pos.z < _PosMin.z)p.pos.z = _PosMax.z;
}

void collideBoundary (Particle p)
{
    if (p.pos.x > _PosMax.x || p.pos.x < _PosMin.x){p.vel.x *= -1;p.pos.x = clamp(p.pos.x, _PosMin.x, _PosMax.x);p.pos.x += p.vel.x;p.disable = 2;}
    if (p.pos.y > _PosMax.y || p.pos.z < _PosMin.z){p.vel.y *= -1;p.pos.y = clamp(p.pos.y, _PosMin.y, _PosMax.y);p.pos.y += p.vel.y;p.disable = 2;}
    if (p.pos.z > _PosMax.z || p.pos.z < _PosMin.z){p.vel.z *= -1;p.pos.z = clamp(p.pos.z, _PosMin.z, _PosMax.z);p.pos.z += p.vel.z;p.disable = 2;}
}