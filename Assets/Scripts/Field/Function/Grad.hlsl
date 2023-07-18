#include "Assets/Scripts/Field/Field.hlsl"

float2 Grad(uint2 id){
    float width, height;
    _Source.GetDimensions(width, height);

    float dxd = 2;
    float dyd = 2;
    if (id.x == 0 || id.x == width-1){
        dxd = 1;
    }
    if (id.y == 0 || id.y == width-1){
        dyd = 1;
    }
    float x1 = _Source[float2(clamp(id.x-1, 0, width-1), id.y)];
    float x2 = _Source[float2(clamp(id.x+1, 0, width-1), id.y)];
    float y1 = _Source[float2(id.x, clamp(id.y-1, 0, height-1))];
    float y2 = _Source[float2(id.x, clamp(id.y+1, 0, height-1))];
    float dx = (x2-x1)/dxd;
    float dy = (y2-y1)/dyd;
    return float2(dx, dy);
}