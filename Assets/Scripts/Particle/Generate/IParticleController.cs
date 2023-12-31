using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;

//VALUE NOT APPLIED IN INSPECTOR, PLEASE USE PROPERTY
[ExecuteInEditMode]
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
    public Vector3 BoundaryMin = new Vector3(-5, 0, -5);
    public Vector3 BoundaryMax = new Vector3(5, 0, 5);
    public float posRange = 1;
    public Vector3 posMin;
    public Vector3 posMax;
    public float velRange = 1;
    public Vector3 velMin;
    public Vector3 velMax;
    public Vector3 sizeMin = Vector3.one;
    public Vector3 sizeMax = Vector3.one;

    public ParticleSystem.MinMaxCurve life;
    public ParticleSystem.MinMaxCurve weight = 1;// addUV
    public ParticleSystem.MinMaxCurve velOverLifetimeX;
    public ParticleSystem.MinMaxCurve velOverLifetimeY;
    public ParticleSystem.MinMaxCurve velOverLifetimeZ;
    public ParticleSystem.MinMaxCurve rotVelOverLifetimeX;
    public ParticleSystem.MinMaxCurve rotVelOverLifetimeY;
    public ParticleSystem.MinMaxCurve rotVelOverLifetimeZ;
    public ParticleSystem.MinMaxCurve sizeOverLifetimeX = 1;
    public ParticleSystem.MinMaxCurve sizeOverLifetimeY = 1;
    public ParticleSystem.MinMaxCurve sizeOverLifetimeZ = 1;
    public ParticleSystem.MinMaxGradient colorOverLifetime;
    public ParticleSystem.MinMaxCurve weightOverLifetime = 1;
    public ParticleSystem.MinMaxCurve customDataOverLifetimeX;// addUV
    public ParticleSystem.MinMaxCurve customDataOverLifetimeY;
    public ParticleSystem.MinMaxCurve customDataOverLifetimeZ;
    public ParticleSystem.MinMaxCurve customDataOverLifetimeW;
    public ParticleSystem.MinMaxCurve fieldOverLifetime;
    public ParticleSystem.MinMaxCurve dampOverLifetime;

    [HideInInspector]
    protected RenderTexture bakedCol, bakedLife, bakedWeight, bakedSizeOverLifetime, bakedVelOverLifetime,
        bakedRotVelOverLifetime, bakedColorOverLifetime, bakedWeightOverLifetime, bakedCustomDataOverLifetime,
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
    float deltaTime;

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

        if (fieldController != null)
        {
            BoundaryMin = fieldController.BoundaryMin;
            BoundaryMax = fieldController.BoundaryMax;
        }

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
        deltaTime += Time.deltaTime;
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
        int total = (int)Mathf.Min(emitCount + additional, particleCount[0]);
        if (total > 0)
        {
            SetBufferToShader(kernelIndexEmit);
            SetTextureToShader(kernelIndexEmit);
            
            computeShader.Dispatch(kernelIndexEmit, total, 1, 1);
        }


        SetBufferToShader(kernelIndexUpdate);
        SetTextureToShader(kernelIndexUpdate);
        computeShader.Dispatch(kernelIndexUpdate, maxCount / THREAD_NUM, 1, 1);
        
        //Debug.Log(particleCount[0]);
        totalEmit += emitCount;
        deltaTime = 0;
    }

    protected void OnDestroy()
    {
        if(particleBuffer != null) particleBuffer.Release();
        if (pooledParticleBuffer != null) pooledParticleBuffer.Release();

    }

    protected void Bake()
    {
        MyGradient.Bake(ref bakedCol, gradient);

        MyCurve.Bake(ref bakedLife, life);
        MyCurve.Bake(ref bakedWeight, weight);
        MyCurve.Bake(ref bakedVelOverLifetime, velOverLifetimeX, velOverLifetimeY, velOverLifetimeZ);
        MyCurve.Bake(ref bakedRotVelOverLifetime, rotVelOverLifetimeX, rotVelOverLifetimeY, rotVelOverLifetimeZ);
        MyCurve.Bake(ref bakedSizeOverLifetime, sizeOverLifetimeX, sizeOverLifetimeY, sizeOverLifetimeZ);
        MyGradient.Bake(ref bakedColorOverLifetime, colorOverLifetime);
        MyCurve.Bake(ref bakedWeightOverLifetime, weightOverLifetime);
        MyCurve.Bake(ref bakedCustomDataOverLifetime, customDataOverLifetimeX, customDataOverLifetimeY, customDataOverLifetimeZ, customDataOverLifetimeW);
        MyCurve.Bake(ref bakedFieldOverLifetime, fieldOverLifetime);
        MyCurve.Bake(ref bakedDampDataOverLifetime, dampOverLifetime);
    }

    protected void SetDataToShader(int kernelId)
    {
        SetValueToShader();
        SetTextureToShader(kernelId);
    }

    protected void SetValueToShader()
    {
        computeShader.SetVector("_BoundMin", BoundaryMin);
        computeShader.SetVector("_BoundMax", BoundaryMax);
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
        computeShader.SetFloat("_DeltaTime", deltaTime);
        computeShader.SetBool("_UseField", fieldController != null);
    }

    protected void SetTextureToShader(int kernelId)
    {
        SetTextureToShader(kernelId, computeShader);
    }


    public void SetTextureToShader(int kernelId, ComputeShader cs)//make external call version
    {
        Bake();

        cs.SetTexture(kernelId, "_PCol", bakedCol);
        cs.SetTexture(kernelId, "_PLife", bakedLife);
        cs.SetTexture(kernelId, "_PWeight", bakedWeight);

        cs.SetTexture(kernelId, "_PVelOverLife", bakedVelOverLifetime);
        cs.SetTexture(kernelId, "_PRotVelOverLife", bakedRotVelOverLifetime);
        cs.SetTexture(kernelId, "_PSizeOverLife", bakedSizeOverLifetime);
        cs.SetTexture(kernelId, "_PColOverLife", bakedColorOverLifetime);
        cs.SetTexture(kernelId, "_PWeightOverLife", bakedWeightOverLifetime);
        cs.SetTexture(kernelId, "_PCustomDataOverLife", bakedCustomDataOverLifetime);
        cs.SetTexture(kernelId, "_PFieldOverLife", bakedFieldOverLifetime);
        cs.SetTexture(kernelId, "_PDampOverLife", bakedDampDataOverLifetime);

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

