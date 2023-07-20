//based on https://github.com/fuqunaga/GpuTrail
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Pool;
using UnityEngine.XR;

public class TrailRender : MonoBehaviour
{
    public float life = 1f;
    public float inputPerSec = 60f;
    public float width = 0.01f;
    public AnimationCurve widthOverLifetime;
    public Gradient colorOverLifetime;
    public AnimationCurve customDataXOverLifetime;
    public AnimationCurve customDataYOverLifetime;
    public AnimationCurve customDataZOverLifetime;
    public AnimationCurve customDataWOverLifetime;
    public float minimalDistance = 0.01f;

    int vertexPerTrail;
    int vertexNum;//vertex and nodes are different!
    int IndexNumPerTrail;//index used for creating surface

    public ComputeShader computeShader;
    public Material material;
    public ParticleGen particleGen;

    TrailData trailData;
    Bounds bounds = new(Vector3.zero, Vector3.one * 100000f);

    float deltaTimeUpdate;

    public struct Vertex
    {
        public Vector3 pos;
        public Vector2 uv;
        public Color col;
    }

    void Start()
    {
        this.trailData = new TrailData(particleGen.maxCount, life, inputPerSec);
        vertexPerTrail = trailData.NodeNumPerTrail * 2;
        vertexNum = trailData.TrailNum * vertexPerTrail;
        IndexNumPerTrail = (vertexPerTrail - 1) * 6;
        InitBufferIfNeed();
    }

    protected GraphicsBuffer vertexBuffer;
    protected GraphicsBuffer indexBuffer;
    protected GraphicsBuffer argsBuffer;

    public MaterialPropertyBlock PropertyBlock;

    protected void InitBufferIfNeed()
    {
        if ((vertexBuffer != null) && (vertexBuffer.count == vertexNum))
        {
            return;
        }
        PropertyBlock = new();
        vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexNum, Marshal.SizeOf<Vertex>()); // 1 node to 2 vtx(left,right)
        vertexBuffer.Fill(default(Vertex));

        indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index | GraphicsBuffer.Target.Structured, IndexNumPerTrail, Marshal.SizeOf<uint>()); // 1 node to 2 triangles(6vertexs)
#if UNITY_2022_2_OR_NEWER
        using var indexArray = new NativeArray<int>(indexBuffer.count, Allocator.Temp);
        var indices = indexArray.AsSpan();
#else
            var indices = new NativeArray<int>(IndexNumPerTrail, Allocator.Temp);
#endif
        // 各Nodeの最後と次のNodeの最初はポリゴンを繋がないので-1
        var idx = 0;
        for (var iNode = 0; iNode < vertexPerTrail / 2 - 1; ++iNode)
        {
            var offset = iNode * 2;
            indices[idx++] = 0 + offset;
            indices[idx++] = 1 + offset;
            indices[idx++] = 2 + offset;
            indices[idx++] = 2 + offset;
            indices[idx++] = 1 + offset;
            indices[idx++] = 3 + offset;
        }

#if UNITY_2022_2_OR_NEWER
        indexBuffer.SetData(indexArray);
#else
            indexBuffer.SetData(indices);
            indices.Dispose();
#endif

        argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, sizeof(uint));
        ResetArgsBuffer();
    }
    protected bool IsSinglePassInstancedRendering => XRSettings.enabled && XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced;

    public void ResetArgsBuffer()
    {
        InitBufferIfNeed();

        using var _ = ListPool<int>.Get(out var argsList);

        argsList.Add(IndexNumPerTrail);
        argsList.Add(trailData.TrailNum * (IsSinglePassInstancedRendering ? 2 : 1));
        argsList.Add(0);
        argsList.Add(0);
        argsList.Add(0);

        argsBuffer.SetData(argsList);
    }

    protected virtual void LateUpdate()
    {
        var toCameraDir = default(Vector3);
        if (Camera.main.orthographic)
        {
            toCameraDir = -Camera.main.transform.forward;
        }

        var kernel = computeShader.FindKernel("CreateNodeTrail");
        var kernelAppendNode = computeShader.FindKernel("AppendNode");
        var kernelVertex = computeShader.FindKernel("CreateVertex");
        computeShader.SetFloat("_Time", Time.time);
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);

        computeShader.SetVector("_ToCameraDir", toCameraDir);
        computeShader.SetVector("_CameraPos", Camera.main.transform.position);
        computeShader.SetInt("_VertexPerTrail", vertexPerTrail);


        deltaTimeUpdate += Time.deltaTime;
        if (deltaTimeUpdate > 1 / trailData.inputPerSec)
        {
            computeShader.SetBuffer(kernelAppendNode, "_ParticleBuffer", particleGen.particleBuffer);
            computeShader.SetBuffer(kernelAppendNode, "_NodeBuffer", trailData.NodeBuffer);
            computeShader.SetBuffer(kernelAppendNode, "_TrailBuffer", trailData.TrailBuffer);
            computeShader.SetBuffer(kernelAppendNode, "_VertexBuffer", vertexBuffer);

            computeShader.Dispatch(kernelAppendNode, vertexNum / 512 / 2, 1, 1);
            deltaTimeUpdate = 0;

            computeShader.SetBuffer(kernelVertex, "_NodeBuffer", trailData.NodeBuffer);
            computeShader.SetBuffer(kernelVertex, "_TrailBuffer", trailData.TrailBuffer);
            computeShader.SetBuffer(kernelVertex, "_VertexBuffer", vertexBuffer);

            computeShader.Dispatch(kernelVertex, vertexNum / 512 / 2, 1, 1);
        }

        PropertyBlock.SetInt("_VertexPerTrail", vertexPerTrail);
        PropertyBlock.SetBuffer("_VertexBuffer", vertexBuffer);
        var renderParams = new RenderParams(material)
        {
            matProps = PropertyBlock,
            worldBounds = bounds
        };

        Graphics.RenderPrimitivesIndexedIndirect(renderParams, MeshTopology.Triangles, indexBuffer, argsBuffer);
    }
}
