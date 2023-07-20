#ifndef VERTEX
#define VERTEX

struct Vertex
{
    float3 pos;
    float4 customData;
    float2 uv;
	half4 col;
};

#ifndef VERTEX_R_ONLY
RWStructuredBuffer<Vertex> _VertexBuffer;
#else
StructuredBuffer<Vertex> _VertexBuffer;
#endif

#endif