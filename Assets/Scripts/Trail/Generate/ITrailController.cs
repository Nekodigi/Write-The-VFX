﻿using System.Collections;
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
    ComputeShader computeShader_;
    public ParticleGen particleGen;

    public TrailData trailData;

    float totalFrame;
    public bool syncStart = false;

    public IFieldController fieldController;



    protected virtual void Awake()
    {
    }

    private void Start()
    {
        if (!syncStart)
        {
            Start();
        }
        syncStart = false;
    }

    public void Start_()
    {
        computeShader_ = Instantiate(computeShader);
        this.trailData = new TrailData(particleGen.maxCount, life, inputPerSec);
        vertexPerTrail = trailData.NodeNumPerTrail * 2;
        vertexNum = trailData.TrailNum * vertexPerTrail;
        InitBufferIfNeed();

        var kernelInitNode = computeShader_.FindKernel("InitNode");

        SetValueToShader();
        computeShader_.SetBuffer(kernelInitNode, "_NodeBuffer", trailData.NodeBuffer);
        computeShader_.SetBuffer(kernelInitNode, "_TrailBuffer", trailData.TrailBuffer);//cannot contail external ParticleBuffer
        computeShader_.Dispatch(kernelInitNode, vertexNum / 512 / 2, 1, 1);

        particleGen.syncUpdate = true;
    }

    public GraphicsBuffer vertexBuffer;


    protected virtual void Update()
    {
        if(trailData == null)return;

        var kernelAppendNode = computeShader_.FindKernel("AppendNode");
        var kernelVertex = computeShader_.FindKernel("CreateVertex");

        SetValueToShader();

        Bake();

        if (trailData.inputPerSec > totalFrame/Time.time)
        {
            particleGen.Update_();

            SetBufferToShader(kernelAppendNode);

            computeShader_.Dispatch(kernelAppendNode, vertexNum / 512 / 2, 1, 1);

            SetBufferToShader(kernelVertex);
            SetTextureToShader(kernelVertex);

            computeShader_.Dispatch(kernelVertex, vertexNum / 512 / 2, 1, 1);
            totalFrame++;
        }

        EditorApplication.QueuePlayerLoopUpdate();
    }

    private void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
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
        MyCurve.Bake(ref bakedWidthOverLifetime, widthOverLifetime);
        MyGradient.Bake(ref bakedColorOverLifetime, colorOverLifetime);
        MyCurve.Bake(ref bakedCustomDataOverLifetime, customDataXOverLifetime, customDataYOverLifetime, customDataZOverLifetime, customDataWOverLifetime);
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

        computeShader_.SetFloat("_Time", Time.time);
        computeShader_.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader_.SetFloat("_TrailWidth", width);
        computeShader_.SetFloat("_TLife", life);
        computeShader_.SetInt("_NodePerTrail", trailData.NodeNumPerTrail);

        computeShader_.SetVector("_BoundMax", particleGen.BoundaryMax);
        computeShader_.SetVector("_BoundMin", particleGen.BoundaryMin);
        computeShader_.SetVector("_ToCameraDir", toCameraDir);
        computeShader_.SetVector("_CameraPos", Camera.main.transform.position);
        computeShader_.SetInt("_NodePerTrail", trailData.NodeNumPerTrail);
    }

    void SetBufferToShader(int kernelId)
    {
        computeShader_.SetBuffer(kernelId, "_ParticleBuffer", particleGen.particleBuffer);
        computeShader_.SetBuffer(kernelId, "_NodeBuffer", trailData.NodeBuffer);
        computeShader_.SetBuffer(kernelId, "_TrailBuffer", trailData.TrailBuffer);
        computeShader_.SetBuffer(kernelId, "_VertexBuffer", vertexBuffer);

        if (fieldController != null)
        {
            computeShader_.SetTexture(kernelId, "_Source", fieldController.source);
            computeShader_.SetTexture(kernelId, "_Dest", fieldController.dest);
            computeShader_.SetTexture(kernelId, "_SourceVec", fieldController.sourceVec);
            computeShader_.SetTexture(kernelId, "_DestVec", fieldController.destVec);
            computeShader_.SetTexture(kernelId, "_SourceVec4", fieldController.sourceVec4);
            computeShader_.SetTexture(kernelId, "_DestVec4", fieldController.destVec4);
        }
    }

    void SetTextureToShader(int kernelId)
    {
        computeShader_.SetTexture(kernelId, "_TWidthOverLifetime", bakedWidthOverLifetime);
        computeShader_.SetTexture(kernelId, "_TColorOverLifetime", bakedColorOverLifetime);
        computeShader_.SetTexture(kernelId, "_TCustomDataOverLifetime", bakedCustomDataOverLifetime);
    }
}
