using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;

//VALUE NOT APPLIED IN INSPECTOR, PLEASE USE PROPERTY
public class IParticleController : MonoBehaviour
{
    public int burst;
    public float burstIntervalSec;
    public int inputPerSec;
    public int maxCount { get {int t = Mathf.CeilToInt((1f*(burstIntervalSec == 0 ? 0 : burst / burstIntervalSec) + inputPerSec) * life.constantMax + (burstIntervalSec == 0 ? burst : 0));
            return Mathf.CeilToInt(1f*t / THREAD_NUM) * THREAD_NUM;
        } }

    [GradientUsage(true)] public Gradient hdrGradientPicker;

    public ParticleSystem.MinMaxGradient gradient;
    public float posRange;
    public Vector3 posMin;
    public Vector3 posMax;
    public float velRange;
    public Vector3 velMin;
    public Vector3 velMax;
    public Vector3 sizeMin;
    public Vector3 sizeMax;

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

    [HideInInspector]
    protected RenderTexture bakedCol, bakedLife, bakedSizeOverLifetime, bakedVelOverLifetime,
        bakedRotVelOverLifetime, bakedColorOverLifetime, bakedCustomDataOverLifetime,
        bakedFieldOverLifetime, bakedDampDataOverLifetime;

    public ComputeShader computeShader_;
    ComputeShader computeShader;

    public ComputeBuffer particleBuffer;
    ComputeBuffer pooledParticleBuffer;
    ComputeBuffer particleCountBuffer;
    Particle[] particles;
    uint[] particleCount;

    protected int kernelIndexInitialize;
    protected int kernelIndexEmit;
    protected int kernelIndexUpdate;

    protected const int THREAD_NUM = 16;

    public IFieldController fieldController;

    [HideInInspector]
    public bool syncUpdate = false;
    int totalEmit = 0;
    float lastBurst;


    // Start is called before the first frame update

    private void Awake()
    {
        syncUpdate = false;

    }

    void Start()
    {
        computeShader = Instantiate(computeShader_);
        kernelIndexInitialize = computeShader.FindKernel("Init");
        kernelIndexEmit = computeShader.FindKernel("Emit");
        kernelIndexUpdate = computeShader.FindKernel("Update");

        particleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(Particle)));
        particles = new Particle[maxCount];
        particleBuffer.SetData(particles);

        pooledParticleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(uint)), ComputeBufferType.Append);
        pooledParticleBuffer.SetCounterValue(0);

        particleCountBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(uint)), ComputeBufferType.IndirectArguments);
        particleCount = new uint[] { 0 };
        particleCountBuffer.SetData(particleCount);

        SetValueToShader();
        SetTextureToShader(kernelIndexInitialize);
        SetBufferToShader(kernelIndexInitialize);
        SetBufferToShader(kernelIndexUpdate);

        SetTextureToShader(kernelIndexEmit);
        SetBufferToShader(kernelIndexEmit);

        computeShader.Dispatch(kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);
        if(burst > 0)computeShader.Dispatch(kernelIndexEmit, burst , 1, 1);//seems certain amount not rendererd
        lastBurst = Time.time;
    }

    private void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    void Update()
    {
        if (!syncUpdate) Update_();

        EditorApplication.QueuePlayerLoopUpdate();
    }

    public void Update_()
    {

        SetValueToShader();

        ComputeBuffer.CopyCount(pooledParticleBuffer, particleCountBuffer, 0);
        particleCountBuffer.GetData(particleCount);
        //Debug.Log(particleCount[0]);

        int emitCount = (int)((Time.time * inputPerSec-totalEmit));
        int additional = 0;
        if (burstIntervalSec != 0 && burstIntervalSec < Time.time - lastBurst)
        {
            additional += burst;
            lastBurst = Time.time;
        }
        if (emitCount+additional > 0)
        {
            SetBufferToShader(kernelIndexEmit);
            SetTextureToShader(kernelIndexEmit);
            
            computeShader.Dispatch(kernelIndexEmit, (int)Mathf.Min(emitCount+additional, particleCount[0]), 1, 1);
        }


        SetBufferToShader(kernelIndexUpdate);
        SetTextureToShader(kernelIndexUpdate);
        computeShader.Dispatch(kernelIndexUpdate, maxCount / THREAD_NUM, 1, 1);
        
        //Debug.Log(particleCount[0]);
        totalEmit += emitCount;
    }

    protected void OnDestroy()
    {
        if(particleBuffer != null) particleBuffer.Release();
        if (pooledParticleBuffer != null) pooledParticleBuffer.Release();

    }

    protected void Bake()
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

    protected void SetDataToShader(int kernelId)
    {
        SetValueToShader();
        SetTextureToShader(kernelId);
    }

    protected void SetValueToShader()
    {
        computeShader.SetFloat("_PosRange", posRange);
        computeShader.SetVector("_PosMin", posMin);
        computeShader.SetVector("_PosMax", posMax);
        computeShader.SetVector("_PosOrigin", transform.position) ;
        computeShader.SetVector("_RotOrigin", transform.rotation.eulerAngles);
        computeShader.SetVector("_SizeOrigin", transform.localScale);
        computeShader.SetFloat("_VelRange", velRange);
        computeShader.SetVector("_VelMin", velMin);
        computeShader.SetVector("_VelMax", velMax);
        computeShader.SetVector("_SizeMin", sizeMin);
        computeShader.SetVector("_SizeMax", sizeMax);
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetBool("_UseField", fieldController != null);
    }

    protected void SetTextureToShader(int kernelId)
    {
        Bake();

        computeShader.SetTexture(kernelId, "_PCol", bakedCol);
        computeShader.SetTexture(kernelId, "_PLife", bakedLife);

        computeShader.SetTexture(kernelId, "_PVelOverLife", bakedVelOverLifetime);
        computeShader.SetTexture(kernelId, "_PRotVelOverLife", bakedRotVelOverLifetime);
        computeShader.SetTexture(kernelId, "_PSizeOverLife", bakedSizeOverLifetime);
        computeShader.SetTexture(kernelId, "_PColOverLife", bakedColorOverLifetime);
        computeShader.SetTexture(kernelId, "_PCustomDataOverLife", bakedCustomDataOverLifetime);
        computeShader.SetTexture(kernelId, "_PFieldOverLife", bakedFieldOverLifetime);
        computeShader.SetTexture(kernelId, "_PDampOverLife", bakedDampDataOverLifetime);

        if (fieldController != null)
        {
            
            computeShader.SetTexture(kernelId, "_Source", fieldController.source);
            computeShader.SetTexture(kernelId, "_Dest", fieldController.dest);
            computeShader.SetTexture(kernelId, "_SourceVec", fieldController.sourceVec);
            computeShader.SetTexture(kernelId, "_DestVec", fieldController.destVec);
        }
    }

    void SetBufferToShader(int kernelId)
    {
        computeShader.SetBuffer(kernelId, "_ParticleBuffer", particleBuffer);
        computeShader.SetBuffer(kernelId, "_DeadParticleBuffer", pooledParticleBuffer);
        computeShader.SetBuffer(kernelId, "_PooledParticleBuffer", pooledParticleBuffer);
    }

}

