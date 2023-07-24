#ifndef COORDINATE
#define COORDINATE

float2 vecToRot(float3 vec){
    float rotY = atan2(vec.x, vec.z);
    float rotX =-asin(vec.y / length(vec.xyz)-1e-8);
    return float2(rotX, rotY);
}

float4x4 eulerAnglesToRotationMatrix(float3 angles)
{
    float ch = cos(angles.y); float sh = sin(angles.y); // heading
    float ca = cos(angles.z); float sa = sin(angles.z); // attitude
    float cb = cos(angles.x); float sb = sin(angles.x); // bank
    // RyRxRz (Heading Bank Attitude)
    return float4x4(
    ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
    cb * sa, cb * ca, -sb, 0,
    -sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
    0, 0, 0, 1
    );
}

float4 sizeRotPos(float4 target, float3 size, float3 rot, float3 pos){
    float4x4 object2world = (float4x4)0;
    object2world._11_22_33_44 = float4(size, 1.0);
    float4x4 rotMatrix =
        eulerAnglesToRotationMatrix(rot);
    object2world = mul(rotMatrix, object2world);
    object2world._14_24_34 += pos;
    return mul(object2world, target);
}
float4 rotCoord(float4 target, float3 rot){
    return sizeRotPos(target, float3(1,1,1), rot, float3(0,0,0));
}
#endif