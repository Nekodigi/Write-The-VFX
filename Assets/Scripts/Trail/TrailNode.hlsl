#ifndef TRAIL_NODE
#define TRAIL_NODE

#include "Assets/Scripts/Particle/Particle.hlsl"
#include "Assets/Scripts/Utils/Sampler/Sampler.hlsl"
#include "Assets/Scripts/Utils/Camera/Camera.hlsl"
#include "Assets/Scripts/Utils/Vertex/Vertex.hlsl"


struct Trail
{
    float spawnTime;
    float life;
    uint totalInputNum;
};

RWStructuredBuffer<Particle> _NodeBuffer;
RWStructuredBuffer<Trail> _TrailBuffer;
uint _NodePerTrail;
float _TrailWidth;

Texture2D<float4> _TWidthOverLifetime;
Texture2D<float4> _TColorOverLifetime;
Texture2D<float4> _TCustomDataOverLifetime;

//*NOTE OFFSET ARE ALREADY APPLIED
int getTrailId(int nodeId){
    return nodeId / _NodePerTrail;
}
int clampNodeId(int nodeId){//value grater than -_NodePerTrail
    return (_NodePerTrail+nodeId)%_NodePerTrail;
}
int getGlobalNodeId(int trailId, int nodeId){
    Trail trail = _TrailBuffer[trailId];
    return trailId*_NodePerTrail+clampNodeId(nodeId+trail.totalInputNum);
}
int getGlobalNodeIdNoOffset(int trailId, int nodeId){
    return trailId*_NodePerTrail+nodeId;
}
int getLocalNodeId(int trailId, int idx){
    Trail trail = _TrailBuffer[trailId];
    return clampNodeId(idx-trail.totalInputNum);
}

int getVertexId(int trailId, int nodeId){
    return (trailId*_NodePerTrail+nodeId)*2;
}

Particle getNode(int trailId, int nodeId){
    return _NodeBuffer[getGlobalNodeId(trailId, nodeId)];
}
float3 getNodeDir(int trailId, int nodeId){//disable if particle warped
    Particle current = getNode(trailId, nodeId);
    Particle next = getNode(trailId, nodeId+1);
    float3 dir = float3(0, 0, 0);
    if(current.disable == 0 && next.disable == 0){//last particle=> disable. to adapt warp problem
        dir = next.pos - current.pos;
    }
    return dir;
}

float getRate(Trail trail, Particle node){
    float age = _Time - node.spawnTime;
    return age/trail.life;
}

float getWidth(float rate){
    float mult = _TWidthOverLifetime.SampleLevel(linearClampSampler, float2(rate, 0), 0).x;
    return mult * _TrailWidth;
}
float4 getColor(float rate){
    float4 col = _TColorOverLifetime.SampleLevel(linearClampSampler, float2(rate, 0), 0);
    return col;
}
float4 getCustomData(float rate){
    float4 customData = _TCustomDataOverLifetime.SampleLevel(linearClampSampler, float2(rate, 0), 0);
    return customData;
}

float3 getNodeRight(uint trailId, uint nodeId){
    Particle node = getNode(trailId, nodeId);
    float rate = getRate(_TrailBuffer[trailId], node);
    float3 toCameraDir = calcToCameraDir(node.pos);
	return rightFromCamera(getNodeDir(trailId, nodeId), getWidth(rate), toCameraDir);
}
void addPairVertex(Vertex v, uint vertexId, float3 right){
    v.pos = v.pos-right;
    _VertexBuffer[vertexId] = v;
    v.pos = v.pos+right*2;
    _VertexBuffer[vertexId+1] = v;
}

#endif