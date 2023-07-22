using UnityEngine;
using System;


public class MyTexture 
{
    static int res = 128;
    static Texture2D texture;


    static public RenderTexture Bake(RenderTexture rt, Func<float, float> sampler)
    {
        return Bake(rt, (fac) => new Vector4(sampler(fac), sampler(fac), sampler(fac), sampler(fac)));
    }
    static public RenderTexture Bake(RenderTexture rt, Func<float, float> sampler1, Func<float, float> sampler2, Func<float, float> sampler3)
    {
        return Bake(rt, (fac) => new Vector4(sampler1(fac), sampler2(fac), sampler3(fac), sampler3(fac)));
    }

    static public RenderTexture Bake(RenderTexture rt, Func<float, float> sampler1, Func<float, float> sampler2, Func<float, float> sampler3, Func<float, float> sampler4)
    {
        return Bake(rt, (fac) => new Vector4(sampler1(fac), sampler2(fac), sampler3(fac), sampler4(fac)));
    }

    static public RenderTexture Bake(RenderTexture rt, Func<float, Vector4> sampler)
    {
        if(!texture) texture = new Texture2D(res, 1, TextureFormat.RGBAFloat, false);
        for (int h = 0; h < texture.height; h++)
        {
            for (int w = 0; w < texture.width; w++)
            {
                float fac = (float)w / texture.width;
                texture.SetPixel(w, h, sampler(fac));
            }
        }
        texture.Apply();
        return Tex2DtoRT(rt, texture);
    }

    static public RenderTexture Tex2DtoRT(RenderTexture rt, Texture2D texture)
    {
        if(!rt) rt = new RenderTexture(texture.width, 1, 0, RenderTextureFormat.ARGBFloat);
        rt.enableRandomWrite = true;
        rt.Create();
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);
        return rt;
    }
}