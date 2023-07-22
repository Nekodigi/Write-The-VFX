using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class ParticleGen : MonoBehaviour
{
    public int maxCount = 10000;

    [GradientUsage(true)] public Gradient hdrGradientPicker;

    public ParticleSystem.MinMaxGradient gradient;
    public float posRange;
    public Vector3 posMin;
    public Vector3 posMax;
    public float velRange;
    public Vector3 velMin;//over lifetime
    public Vector3 velMax;
    public Vector3 sizeMin;
    public Vector3 sizeMax;
    //

    public ParticleSystem.MinMaxCurve life;
    public ParticleSystem.MinMaxCurve velOverLifetimeX;
    public ParticleSystem.MinMaxCurve velOverLifetimeY;
    public ParticleSystem.MinMaxCurve velOverLifetimeZ;
    public ParticleSystem.MinMaxCurve rotVelOverLifetimeX;
    public ParticleSystem.MinMaxCurve rotVelOverLifetimeY;
    public ParticleSystem.MinMaxCurve rotVelOverLifetimeZ;
    public ParticleSystem.MinMaxCurve sizeOverLifetimeX;
    public ParticleSystem.MinMaxCurve sizeOverLifetimeY;
    public ParticleSystem.MinMaxCurve sizeOverLifetimeZ;
    public ParticleSystem.MinMaxGradient colorOverLifetime;
    public ParticleSystem.MinMaxCurve customDataOverLifetimeX;// addUV
    public ParticleSystem.MinMaxCurve customDataOverLifetimeY;
    public ParticleSystem.MinMaxCurve customDataOverLifetimeZ;
    public ParticleSystem.MinMaxCurve customDataOverLifetimeW;
    public ParticleSystem.MinMaxCurve fieldOverLifetime;
    public ParticleSystem.MinMaxCurve dampOverLifetime;

    RenderTexture bakedCol;
    RenderTexture bakedLife;
    RenderTexture bakedSizeOverLifetime;
    RenderTexture bakedVelOverLifetime;
    RenderTexture bakedRotVelOverLifetime;
    RenderTexture bakedColorOverLifetime;
    RenderTexture bakedCustomDataOverLifetime;
    RenderTexture bakedFieldOverLifetime;
    RenderTexture bakedDampDataOverLifetime;


    public ComputeShader computeShader;
    public Mesh mesh;
    public Material material;

    public ComputeBuffer particleBuffer;
    Particle[] particles;

    int kernelIndexInitialize;
    int kernelIndexUpdate;

    const int THREAD_NUM = 16;
    const int BAKE_RES = 128;

    public RenderTexture field;

    public bool syncUpdate = false;

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

        SetValueToShader();

        SetTextureToShader(kernelIndexInitialize);
        computeShader.SetBuffer(kernelIndexInitialize, "_ParticleBuffer", particleBuffer);
        computeShader.SetBuffer(kernelIndexUpdate, "_ParticleBuffer", particleBuffer);

        computeShader.Dispatch(kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);

        material.SetBuffer("_ParticleBuffer", particleBuffer);
    }

    // Update is called once per frame
    void Update()
    {
        if (!syncUpdate) Update_();
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 100), maxCount);
    }

    public void Update_()
    {
        SetTextureToShader(kernelIndexUpdate);

        computeShader.Dispatch(kernelIndexUpdate, maxCount / THREAD_NUM, 1, 1);
    }


    void OnDestroy()
    {
        particleBuffer.Release();
    }

    void Bake()
    {
        bakedCol = MyGradient.Bake(bakedCol, gradient);

        bakedLife = MyCurve.Bake(bakedLife, life);
        bakedVelOverLifetime = MyCurve.Bake(bakedVelOverLifetime, velOverLifetimeX, velOverLifetimeY, velOverLifetimeZ);
        bakedRotVelOverLifetime = MyCurve.Bake(bakedRotVelOverLifetime, rotVelOverLifetimeX, rotVelOverLifetimeY, rotVelOverLifetimeZ);
        bakedSizeOverLifetime = MyCurve.Bake(bakedSizeOverLifetime, sizeOverLifetimeX, sizeOverLifetimeY, sizeOverLifetimeZ);
        bakedColorOverLifetime = MyGradient.Bake(bakedColorOverLifetime, colorOverLifetime);
        bakedCustomDataOverLifetime = MyCurve.Bake(bakedCustomDataOverLifetime, customDataOverLifetimeX, customDataOverLifetimeY, customDataOverLifetimeZ, customDataOverLifetimeW);
        bakedFieldOverLifetime = MyCurve.Bake(bakedFieldOverLifetime, fieldOverLifetime);
        bakedDampDataOverLifetime = MyCurve.Bake(bakedDampDataOverLifetime, dampOverLifetime);
    }

    void SetValueToShader()
    {
        computeShader.SetFloat("_PosRange", posRange);
        computeShader.SetVector("_PosMin", posMin);
        computeShader.SetVector("_PosMax", posMax);
        computeShader.SetFloat("_VelRange", velRange);
        computeShader.SetVector("_VelMin", velMin);
        computeShader.SetVector("_VelMax", velMax);
        computeShader.SetVector("_SizeMin", sizeMin);
        computeShader.SetVector("_SizeMax", sizeMax);
        computeShader.SetFloat("_Time", 0);
        computeShader.SetFloat("_DeltaTime", 0);
    }

    void SetTextureToShader(int kernelId)
    {
        Bake();

        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);

        computeShader.SetTexture(kernelId, "_PCol", bakedCol);
        computeShader.SetTexture(kernelId, "_PLife", bakedLife);

        computeShader.SetTexture(kernelId, "_PVelOverLife", bakedVelOverLifetime);
        computeShader.SetTexture(kernelId, "_PRotVelOverLife", bakedRotVelOverLifetime);
        computeShader.SetTexture(kernelId, "_PSizeOverLife", bakedSizeOverLifetime);
        computeShader.SetTexture(kernelId, "_PColOverLife", bakedColorOverLifetime);
        computeShader.SetTexture(kernelId, "_PCustomDataOverLife", bakedCustomDataOverLifetime);
        computeShader.SetTexture(kernelId, "_PFieldOverLife", bakedFieldOverLifetime);
        computeShader.SetTexture(kernelId, "_PDampOverLife", bakedDampDataOverLifetime);

        computeShader.SetTexture(kernelIndexUpdate, "_SourceVec", field);


    }
}
