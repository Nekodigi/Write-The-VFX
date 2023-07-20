using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCurve : MonoBehaviour
{
    static public RenderTexture Bake(AnimationCurve c, int res)
    {
        RenderTexture rt = new RenderTexture(res, 1, 0, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();
        Texture2D texture = new Texture2D(res, 1);
        for (int h = 0; h < texture.height; h++)
        {
            for (int w = 0; w < texture.width; w++)
            {
                texture.SetPixel(w, h, new Color(c.Evaluate((float)w / texture.width), 0, 0, 0));
            }
        }
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);
        return rt;
    }

    static public RenderTexture Bake(AnimationCurve c1, AnimationCurve c2, AnimationCurve c3, AnimationCurve c4, int res)
    {
        RenderTexture rt = new RenderTexture(res, 1, 0, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();
        Texture2D texture = new Texture2D(res, 1);
        for (int h = 0; h < texture.height; h++)
        {
            for (int w = 0; w < texture.width; w++)
            {
                float fac = (float)w / texture.width;

                texture.SetPixel(w, h, new Color(c1.Evaluate(fac), c2.Evaluate(fac), c3.Evaluate(fac), c4.Evaluate(fac)));
            }
        }
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);
        return rt;
    }
}
