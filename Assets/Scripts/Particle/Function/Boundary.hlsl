#include "Assets/Scripts/Particle/Particle.hlsl"

void loopBoundary (Particle p)
{
    if (p.pos.x > _BoundMax.x)p.pos.x = _BoundMin.x;
    if (p.pos.x < _BoundMin.x)p.pos.x = _BoundMax.x;
    if (p.pos.y > _BoundMax.y)p.pos.y = _BoundMin.y;
    if (p.pos.y < _BoundMin.y)p.pos.y = _BoundMax.y;
    if (p.pos.z > _BoundMax.z)p.pos.z = _BoundMin.z;
    if (p.pos.z < _BoundMin.z)p.pos.z = _BoundMax.z;
}

void collideBoundary (Particle p)
{
    if (p.pos.x > _BoundMax.x || p.pos.x < _BoundMin.x){p.vel.x *= -1;p.pos.x = clamp(p.pos.x, _BoundMin.x, _BoundMax.x);p.pos.x += p.vel.x;p.disable = 2;}
    if (p.pos.y > _BoundMax.y || p.pos.z < _BoundMin.z){p.vel.y *= -1;p.pos.y = clamp(p.pos.y, _BoundMin.y, _BoundMax.y);p.pos.y += p.vel.y;p.disable = 2;}
    if (p.pos.z > _BoundMax.z || p.pos.z < _BoundMin.z){p.vel.z *= -1;p.pos.z = clamp(p.pos.z, _BoundMin.z, _BoundMax.z);p.pos.z += p.vel.z;p.disable = 2;}
}