using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class MyGradient
{

    static public RenderTexture Bake(Gradient g, int res)
    {
        RenderTexture rt = new RenderTexture(res, 1, 0, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();
        Texture2D texture = new Texture2D(res, 1);
        for (int h = 0; h < texture.height; h++)
        {
            for (int w = 0; w < texture.width; w++)
            {
                texture.SetPixel(w, h, g.Evaluate((float)w / texture.width));
            }
        }
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);
        return rt;
    }
}