using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.Pool;
using UnityEngine.XR;

[ExecuteInEditMode]
public class ITrailController : MonoBehaviour
{
    public float life = 1f;
    public float inputPerSec = 60f;
    public float width = 0.01f;
    public ParticleSystem.MinMaxCurve widthOverLifetime;
    public ParticleSystem.MinMaxGradient colorOverLifetime;
    public ParticleSystem.MinMaxCurve customDataXOverLifetime;
    public ParticleSystem.MinMaxCurve customDataYOverLifetime;
    public ParticleSystem.MinMaxCurve customDataZOverLifetime;
    public ParticleSystem.MinMaxCurve customDataWOverLifetime;
    public float minimalDistance = 0.01f;

    RenderTexture bakedWidthOverLifetime;
    RenderTexture bakedColorOverLifetime;
    RenderTexture bakedCustomDataOverLifetime;

    [HideInInspector]
    public int vertexPerTrail;
    int vertexNum;//vertex and nodes are different!

    public ComputeShader computeShader;
    public ParticleGen particleGen;

    public TrailData trailData;

    float totalFrame;


    protected virtual void Start()
    {
        this.trailData = new TrailData(particleGen.maxCount, life, inputPerSec);
        vertexPerTrail = trailData.NodeNumPerTrail * 2;
        Debug.Log(trailData.NodeNumPerTrail);
        vertexNum = trailData.TrailNum * vertexPerTrail;
        InitBufferIfNeed();

        var kernelInitNode = computeShader.FindKernel("InitNode");

        SetValueToShader();
        computeShader.SetBuffer(kernelInitNode, "_NodeBuffer", trailData.NodeBuffer);
        computeShader.SetBuffer(kernelInitNode, "_TrailBuffer", trailData.TrailBuffer);//cannot contail external ParticleBuffer
        computeShader.Dispatch(kernelInitNode, vertexNum / 512 / 2, 1, 1);

        particleGen.syncUpdate = true;
    }

    public GraphicsBuffer vertexBuffer;


    protected virtual void LateUpdate()
    {
        var kernelAppendNode = computeShader.FindKernel("AppendNode");
        var kernelVertex = computeShader.FindKernel("CreateVertex");

        SetValueToShader();

        Bake();

        if (trailData.inputPerSec > totalFrame/Time.time)
        {
            particleGen.Update_();

            SetBufferToShader(kernelAppendNode);

            computeShader.Dispatch(kernelAppendNode, vertexNum / 512 / 2, 1, 1);

            SetBufferToShader(kernelVertex);
            SetTextureToShader(kernelVertex);

            computeShader.Dispatch(kernelVertex, vertexNum / 512 / 2, 1, 1);
            totalFrame++;
        }

        EditorApplication.QueuePlayerLoopUpdate();
    }

    private void OnEnable()
    {
        EditorApplication.update += LateUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= LateUpdate;
    }

    protected void InitBufferIfNeed()
    {
        if ((vertexBuffer != null) && (vertexBuffer.count == vertexNum))
        {
            return;
        }
        vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexNum, Marshal.SizeOf<Vertex>()); // 1 node to 2 vtx(left,right)
        vertexBuffer.Fill(default(Vertex));
    }

    void Bake()
    {
        bakedWidthOverLifetime = MyCurve.Bake(bakedWidthOverLifetime, widthOverLifetime);
        bakedColorOverLifetime = MyGradient.Bake(bakedColorOverLifetime, colorOverLifetime);
        bakedCustomDataOverLifetime = MyCurve.Bake(bakedCustomDataOverLifetime, customDataXOverLifetime, customDataYOverLifetime, customDataZOverLifetime, customDataWOverLifetime);
    }

    public void Dispose()
    {
        ReleaseBuffer();
    }

    protected virtual void ReleaseBuffer()
    {
        vertexBuffer?.Release();
    }

    void SetValueToShader()
    {
        var toCameraDir = default(Vector3);
        if (Camera.main.orthographic)
        {
            toCameraDir = -Camera.main.transform.forward;
        }

        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.SetFloat("_TrailWidth", width);
        computeShader.SetFloat("_TLife", life);
        computeShader.SetInt("_NodePerTrail", trailData.NodeNumPerTrail);

        computeShader.SetVector("_ToCameraDir", toCameraDir);
        computeShader.SetVector("_CameraPos", Camera.main.transform.position);
        computeShader.SetInt("_NodePerTrail", trailData.NodeNumPerTrail);
    }

    void SetBufferToShader(int kernelId)
    {
        computeShader.SetBuffer(kernelId, "_ParticleBuffer", particleGen.particleBuffer);
        computeShader.SetBuffer(kernelId, "_NodeBuffer", trailData.NodeBuffer);
        computeShader.SetBuffer(kernelId, "_TrailBuffer", trailData.TrailBuffer);
        computeShader.SetBuffer(kernelId, "_VertexBuffer", vertexBuffer);
    }

    void SetTextureToShader(int kernelId)
    {
        computeShader.SetTexture(kernelId, "_TWidthOverLifetime", bakedWidthOverLifetime);
        computeShader.SetTexture(kernelId, "_TColorOverLifetime", bakedColorOverLifetime);
        computeShader.SetTexture(kernelId, "_TCustomDataOverLifetime", bakedCustomDataOverLifetime);
    }
}
