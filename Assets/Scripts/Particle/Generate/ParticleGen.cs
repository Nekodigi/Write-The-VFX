using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class ParticleGen : MonoBehaviour
{
    public int maxCount = 10000;

    [GradientUsage(true)] public Gradient gradient;
    public Vector3 posMin;
    public Vector3 posMax;
    public Vector3 velMin;//over lifetime
    public Vector3 velMax;
    public Vector3 sizeMin;
    public Vector3 sizeMax;
    //

    public ComputeShader computeShader;
    public Mesh mesh;
    public Material material;

    public ComputeBuffer particleBuffer;
    Particle[] particles;

    int kernelIndexInitialize;
    int kernelIndexUpdate;

    const int THREAD_NUM = 16;

    public RenderTexture field;

    // Start is called before the first frame update
    void Start()
    {
        maxCount = (maxCount / THREAD_NUM) * THREAD_NUM;

        field = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf);
        field.enableRandomWrite = true;
        field.Create();

        kernelIndexInitialize = computeShader.FindKernel("Initialize");
        kernelIndexUpdate = computeShader.FindKernel("Update");

        particleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(Particle)));
        particles = new Particle[maxCount];
        particleBuffer.SetData(particles);
        computeShader.SetBuffer(kernelIndexInitialize, "_ParticleBuffer", particleBuffer);
        computeShader.SetBuffer(kernelIndexUpdate, "_ParticleBuffer", particleBuffer);

        computeShader.SetVector("_PosMin", posMin);
        computeShader.SetVector("_PosMax", posMax);
        computeShader.SetVector("_VelMin", velMin);
        computeShader.SetVector("_VelMax", velMax);
        computeShader.SetVector("_SizeMin", sizeMin);
        computeShader.SetVector("_SizeMax", sizeMax);
        computeShader.SetFloat("_Time", 0);
        computeShader.SetFloat("_DeltaTime", 0);
        computeShader.SetTexture(kernelIndexInitialize, "_ColorDist", MyGradient.Bake(gradient, 128));
        material.SetBuffer("_ParticleBuffer", particleBuffer);

        computeShader.Dispatch(kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);

    }

    // Update is called once per frame
    void Update()
    {
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetTexture(kernelIndexUpdate, "_SourceVec", field);

        computeShader.Dispatch(kernelIndexUpdate, maxCount / THREAD_NUM, 1, 1);

        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 100), maxCount);
    }


    void OnDestroy()
    {
        particleBuffer.Release();
    }
}
