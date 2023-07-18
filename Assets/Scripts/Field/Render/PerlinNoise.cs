using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : MonoBehaviour
{
    public GameObject display;
    public ComputeShader computeShader;
    //public ComputeShader computeShaderGrad;
    //public ComputeShader computeShaderCurl;
    public ParticleGen particleGen;

    RenderTexture source;
    RenderTexture dest;
    int kernelIndexPerlinNoise;
    int kernelIndexCurlNoise;
    //int kernelIndexCurl;

    struct ThreadSize
    {
        public int x;
        public int y;
        public int z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }
    }

    ThreadSize threadSize;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        kernelIndexPerlinNoise = computeShader.FindKernel("PerlinNoise");
        kernelIndexCurlNoise = computeShader.FindKernel("CurlNoise");
        //kernelIndexGrad = computeShaderGrad.FindKernel("CSMain");
        //kernelIndexCurl = computeShaderCurl.FindKernel("CSMain");

        source = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf);
        dest = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf);
        source.enableRandomWrite = true;
        source.Create();
        dest.enableRandomWrite = true;
        dest.Create();

        uint threadSizeX, threadSizeY, threadSizeZ;
        computeShader.GetKernelThreadGroupSizes
            (kernelIndexPerlinNoise,
             out threadSizeX, out threadSizeY, out threadSizeZ);
        threadSize
            = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

        
       
        //computeShaderGrad.SetTexture(kernelIndexGrad, "Source", texture);
        //computeShaderGrad.SetTexture(kernelIndexGrad, "Dest", dest);
        //computeShaderCurl.SetTexture(kernelIndexCurl, "Result", dest);
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        computeShader.SetTexture(kernelIndexPerlinNoise, "_Dest", dest);
        computeShader.SetFloat("_Time", time);
        computeShader.Dispatch(kernelIndexPerlinNoise,
                                    source.width / threadSize.x,
                                    source.height / threadSize.y,
                                    threadSize.z);

        Graphics.Blit(dest, source);
        computeShader.SetTexture(kernelIndexCurlNoise, "_Source", source);
        computeShader.SetTexture(kernelIndexCurlNoise, "_DestVec", dest);
        computeShader.Dispatch(kernelIndexCurlNoise,
                                    source.width / threadSize.x,
                                    source.height / threadSize.y,
                                    threadSize.z);
        /*computeShaderGrad.Dispatch(kernelIndexGrad,
                                    dest.width / threadSize.x,
                                    dest.height / threadSize.y,
                                    threadSize.z);
        computeShaderCurl.Dispatch(kernelIndexCurl,
                                    dest.width / threadSize.x,
                                    dest.height / threadSize.y,
                                    threadSize.z);*/
        particleGen.field = dest;
        display.GetComponent<Renderer>().material.mainTexture = dest;
    }
}
