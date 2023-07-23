using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : IFieldController
{
    public GameObject display;

    public ParticleGen particleGen;

    int kernelUpdate2;

    protected override void Awake()
    {
        base.Awake();
        kernelUpdate2 = computeShader.FindKernel("Update2");
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        Graphics.Blit(dest, source);
        Dispatch(kernelUpdate2);
        Graphics.Blit(destVec, sourceVec);
        //particleGen.field = dest;
    }

    
}
