#ifndef CAMERA
#define CAMERA

float3 _ToCameraDir; // for orthographic camera
float3 _CameraPos;   // for perspective camera

inline bool useToCameraDir()
{
	return all(_ToCameraDir == 0);
}

inline float3 calcToCameraDir(float3 pos)
{
	return useToCameraDir() ? normalize(_CameraPos - pos) : _ToCameraDir;
}

float3 rightFromCamera(float3 dir, float width, float3 toCameraDir){
    return normalize(cross(dir, toCameraDir)) * width * 0.5f;
}

#endif