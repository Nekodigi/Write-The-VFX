#ifndef CURLNOISE
#define CURLNOISE
float3 curlNoise(float3 dx, float3 dy, float3 dz){

	return float3(dy.z-dz.y, dz.x-dx.z, dx.y-dy.x);
}

float2 curlNoise(float2 grad){
	return float2(grad.y, -grad.x);
}
#endif