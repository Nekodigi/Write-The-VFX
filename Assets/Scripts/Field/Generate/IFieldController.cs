using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class IFieldController : MonoBehaviour
{
    public Vector2Int resolution = new Vector2Int(512, 512);
    public Vector2 scale = new Vector2(1, 1);
    public float transition = 1;
    public float multiplier = 1;
    public float range = 1;
    public float gamma = 1;

    public ComputeShader computeShader;

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
        if (resolution.x == 0 || resolution.y == 0) Debug.LogError("Field resolution can't be 0");
        kernelInit = computeShader.FindKernel("Init");
        kernelUpdate = computeShader.FindKernel("Update");

        source = CreateRT();
        dest = CreateRT();
        sourceVec = CreateRT();
        destVec = CreateRT();

        uint threadSizeX, threadSizeY, threadSizeZ;
        computeShader.GetKernelThreadGroupSizes
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
        computeShader.Dispatch(kernelId,
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
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetVector("_FRes",(Vector2) resolution);
        computeShader.SetVector("_FScale", scale);
        computeShader.SetFloat("_FTrans", transition);
        computeShader.SetFloat("_FMult", multiplier);
        computeShader.SetFloat("_FRange", range);
        computeShader.SetFloat("_FGamma", gamma);
        if (particleGen.particleBuffer != null)
        {
            computeShader.SetVector("_PosMin", particleGen.posMin);
            computeShader.SetVector("_PosMax", particleGen.posMax);
        }
    }

    protected void SetTexturesToShader(int kernelId)
    {
        computeShader.SetTexture(kernelId, "_Source", source);
        computeShader.SetTexture(kernelId, "_Dest", dest);
        computeShader.SetTexture(kernelId, "_SourceVec", sourceVec);
        computeShader.SetTexture(kernelId, "_DestVec", destVec);
    }

    protected void SetBufferToShader(int kernelId)
    {
        if(particleGen.particleBuffer != null) computeShader.SetBuffer(kernelId, "_ParticleBuffer", particleGen.particleBuffer);
    }
}
