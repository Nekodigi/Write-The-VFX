using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class MyGradient
{

    static public RenderTexture Bake(RenderTexture rt, Gradient g)
    {
        return MyTexture.Bake(rt, (fac) => g.Evaluate(fac));
    }

    static public RenderTexture Bake(RenderTexture rt, ParticleSystem.MinMaxGradient g)
    {
        return MyTexture.Bake(rt, (fac) => g.Evaluate(fac));
    }
}