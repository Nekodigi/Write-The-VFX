#include "Assets/Scripts/Field/Field.hlsl"

float2 Grad(uint2 id){
    float width, height;
    _Source.GetDimensions(width, height);

    float dxd = 2;
    float dyd = 2;
    if (id.x == 0 || id.x == width-1){
        dxd = 1;
    }
    if (id.y == 0 || id.y == height-1){
        dyd = 1;
    }
    float idx = id.x;
    float idy = id.y;
    float x1 = _Source[float2(clamp(idx-1, 0, width-1), id.y)];//why clamp return wrong value with minus
    float x2 = _Source[float2(clamp(id.x+1, 0, width-1), id.y)];
    float y1 = _Source[float2(id.x, clamp(idy, 0, height-1))];
    float y2 = _Source[float2(id.x, clamp(id.y+1, 0, height-1))];
    float dx = (x2-x1)/dxd;
    float dy = (y2-y1)/dyd;
    return float2(dx, dy);
}