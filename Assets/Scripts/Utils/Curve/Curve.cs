using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MyCurve : MonoBehaviour
{ 
    static public RenderTexture Bake(ref RenderTexture rt, AnimationCurve c)
    {
        return MyTexture.Bake(ref rt, (fac) => c.Evaluate(fac));
    }

    static public RenderTexture Bake(ref RenderTexture rt,AnimationCurve c1, AnimationCurve c2, AnimationCurve c3, AnimationCurve c4)
    {
        return MyTexture.Bake(ref rt, (fac) => c1.Evaluate(fac), (fac) => c2.Evaluate(fac), (fac) => c3.Evaluate(fac), (fac) => c4.Evaluate(fac));

    }

    static public RenderTexture Bake(ref RenderTexture rt,ParticleSystem.MinMaxCurve c)
    {
        return MyTexture.Bake(ref rt, (fac) => c.Evaluate(fac));
    }

    static public RenderTexture Bake(ref RenderTexture rt,ParticleSystem.MinMaxCurve c1, ParticleSystem.MinMaxCurve c2, ParticleSystem.MinMaxCurve c3, ParticleSystem.MinMaxCurve c4)
    {
        return MyTexture.Bake(ref rt, (fac) => c1.Evaluate(fac), (fac) => c2.Evaluate(fac), (fac) => c3.Evaluate(fac), (fac) => c4.Evaluate(fac));
    }

    static public RenderTexture Bake(ref RenderTexture rt,ParticleSystem.MinMaxCurve c1, ParticleSystem.MinMaxCurve c2, ParticleSystem.MinMaxCurve c3)
    {
        return MyTexture.Bake(ref rt, (fac) => c1.Evaluate(fac), (fac) => c2.Evaluate(fac), (fac) => c3.Evaluate(fac));
    }
}
