#ifndef VERTEX
#define VERTEX

struct Vertex
{
    float3 pos;
    float2 uv;
	half4 color;
};

RWStructuredBuffer<Vertex> _VertexBuffer;

#endif