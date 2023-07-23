using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class VectorVisParticleController : MonoBehaviour
{
    int width, height;
    public int maxCount;
    Vector3 posMin;
    Vector3 posMax;
    Vector4 vecMin;
    Vector4 vecMax;
    Vector2 interval;

    ComputeShader computeShader;

    public ComputeBuffer particleBuffer;
    Particle[] particles;

    protected int kernelIndexInitialize;
    protected int kernelIndexUpdate;

    protected const int THREAD_NUM = 8;

    RenderTexture target;

    //Set target RenderTexture as target.
    public VectorVisParticleController(Vector2 interval, Vector3 posMin, Vector3 posMax, ComputeShader computeShader, Vector2 res)
    {
        width = Mathf.CeilToInt(res.x / interval.x);
        height = Mathf.CeilToInt(res.y / interval.y);
        this.maxCount = Mathf.CeilToInt(width / THREAD_NUM) * THREAD_NUM * Mathf.CeilToInt(height / THREAD_NUM) * THREAD_NUM ;
        this.interval = interval;
        this.posMin = posMin;
        this.posMax = posMax;
        
        this.computeShader = computeShader;

        kernelIndexInitialize = computeShader.FindKernel("Init");
        kernelIndexUpdate = computeShader.FindKernel("Update");

        particleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(Particle)));
        particles = new Particle[maxCount];
        particleBuffer.SetData(particles);

    }

    public void Update_(RenderTexture target, Vector4 vecMin, Vector4 vecMax)
    {
        this.target = target;
        this.vecMin = vecMin;
        this.vecMax = vecMax;

        SetValueToShader();
        SetTextureToShader(kernelIndexUpdate);
        SetBufferToShader(kernelIndexUpdate);

        computeShader.Dispatch(kernelIndexUpdate, width / THREAD_NUM, height / THREAD_NUM, 1);
    }

    protected void OnDestroy()
    {
        particleBuffer.Release();
    }

    protected void SetDataToShader(int kernelId)
    {
        SetValueToShader();
        SetTextureToShader(kernelId);
    }

    protected void SetValueToShader()
    {
        computeShader.SetVector("_Interval", interval);
        computeShader.SetVector("_PosMin", posMin);
        computeShader.SetVector("_PosMax", posMax);
        computeShader.SetVector("_VecMin", vecMin);
        computeShader.SetVector("_VecMax", vecMax);
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
    }

    protected void SetTextureToShader(int kernelId)
    {
        computeShader.SetTexture(kernelId, "_SourceVec", target);
    }

    void SetBufferToShader(int kernelId)
    {
        computeShader.SetBuffer(kernelId, "_ParticleBuffer", particleBuffer);
    }

}

