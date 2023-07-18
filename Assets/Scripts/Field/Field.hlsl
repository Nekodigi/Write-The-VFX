#ifndef FIELD
#define FIELD

Texture2D<float> _Source;
Texture2D<float2> _SourceVec;
RWTexture2D<float> _Dest;
RWTexture2D<float2> _DestVec;
SamplerState linearClampSampler;


#endif