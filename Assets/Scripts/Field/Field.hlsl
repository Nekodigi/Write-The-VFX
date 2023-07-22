#ifndef FIELD
#define FIELD

#include "Assets/Scripts/Utils/Sampler/Sampler.hlsl"

Texture2D<float> _Source;
Texture2D<float2> _SourceVec;
RWTexture2D<float> _Dest;
RWTexture2D<float2> _DestVec;


#endif