using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class IFieldController : MonoBehaviour
{
    public Vector2Int resolution = new Vector2Int(512, 512);
    public Vector3 BoundaryMin = new Vector3(-5, 0, -5);
    public Vector3 BoundaryMax = new Vector3(5, 0, 5);
    public Vector2 scale = new Vector2(1, 1);
    public float transition = 1;
    public float multiplier = 1;
    public float range = 1;
    public float gamma = 1;

    public ComputeShader computeShader;
    ComputeShader computeShader_;

    public ParticleGen particleGen;

    [HideInInspector]
    public RenderTexture source, dest, sourceVec, destVec;

    protected int kernelInit;
    protected int kernelUpdate;
    protected struct ThreadSize
    {
        public int x;
        public int y;
        public int z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = Mathf.Max(8, (int)x);
            this.y = Mathf.Max(8, (int)y);
            this.z = Mathf.Max(8, (int)z);
        }
    }
    protected ThreadSize threadSize;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        computeShader_ = Instantiate(computeShader);
        if (resolution.x == 0 || resolution.y == 0) Debug.LogError("Field resolution can't be 0");
        kernelInit = computeShader_.FindKernel("Init");
        kernelUpdate = computeShader_.FindKernel("Update");

        source = CreateRT();
        dest = CreateRT();
        sourceVec = CreateRT();
        destVec = CreateRT();

        uint threadSizeX, threadSizeY, threadSizeZ;
        computeShader_.GetKernelThreadGroupSizes
            (kernelInit,
             out threadSizeX, out threadSizeY, out threadSizeZ);
        threadSize
            = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);
        Dispatch(kernelInit);
    }

    private void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Dispatch(kernelUpdate);
        EditorApplication.QueuePlayerLoopUpdate();
    }

    protected void Dispatch(int kernelId)
    {
        SetValuesToShader();
        SetTexturesToShader(kernelId);
        SetBufferToShader(kernelId);
        computeShader_.Dispatch(kernelId,
                                    source.width / threadSize.x,
                                    source.height / threadSize.y,
                                    threadSize.z);
    }

    RenderTexture CreateRT()
    {
        RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGBFloat);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    protected void SetValuesToShader()
    {
        computeShader_.SetFloat("_Time", Time.time);
        computeShader_.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader_.SetVector("_FRes",(Vector2) resolution);
        computeShader_.SetVector("_FScale", scale);
        computeShader_.SetFloat("_FTrans", transition);
        computeShader_.SetFloat("_FMult", multiplier);
        computeShader_.SetFloat("_FRange", range);
        computeShader_.SetFloat("_FGamma", gamma);
        computeShader_.SetVector("_BoundMin", BoundaryMin);
        computeShader_.SetVector("_BoundMax", BoundaryMax);
    }

    protected void SetTexturesToShader(int kernelId)
    {
        computeShader_.SetTexture(kernelId, "_Source", source);
        computeShader_.SetTexture(kernelId, "_Dest", dest);
        computeShader_.SetTexture(kernelId, "_SourceVec", sourceVec);
        computeShader_.SetTexture(kernelId, "_DestVec", destVec);
    }

    protected void SetBufferToShader(int kernelId)
    {
        if(particleGen != null && particleGen.particleBuffer != null) computeShader_.SetBuffer(kernelId, "_ParticleBuffer", particleGen.particleBuffer);
    }
}
