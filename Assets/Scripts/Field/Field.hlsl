#ifndef FIELD
#define FIELD

#include "Assets/Scripts/Utils/Sampler/Sampler.hlsl"

Texture2D<float> _Source;
Texture2D<float2> _SourceVec;
RWTexture2D<float> _Dest;
RWTexture2D<float2> _DestVec;

float4 _VecMin;
float4 _VecMax;
float2 _Interval;
float2 _FRes;
float2 _FScale;
float _FTrans;
float _FMult;
float _FRange;
float _FGamma;

#endif