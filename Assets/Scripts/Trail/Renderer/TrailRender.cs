//based on https://github.com/fuqunaga/GpuTrail
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Pool;
using UnityEngine.XR;
using UnityEditor;

[RequireComponent(typeof(ITrailController))]
[ExecuteInEditMode]
public class TrailRender : MonoBehaviour
{
    public Material material;

    Bounds bounds = new(Vector3.zero, Vector3.one * 100000f);

    public MaterialPropertyBlock PropertyBlock;
    protected GraphicsBuffer indexBuffer;
    protected GraphicsBuffer argsBuffer;

    ITrailController trailController;
    int IndexNumPerTrail;
    TrailData trailData;

    public IFieldController fieldController;

    private void Awake()
    {
        trailController = GetComponent<ITrailController>();
        trailController.syncStart = true;
    }

    void Start()
    {
        trailController.Start_();
        IndexNumPerTrail = IndexNumPerTrail = (trailController.vertexPerTrail - 1) * 6;
        trailData = trailController.trailData;
        InitBufferIfNeed();
    }

    protected void InitBufferIfNeed()
    {
        if ((PropertyBlock != null))
        {
            return;
        }
        PropertyBlock = new();
        indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index | GraphicsBuffer.Target.Structured, IndexNumPerTrail, Marshal.SizeOf<uint>()); // 1 node to 2 triangles(6vertexs)
#if UNITY_2022_2_OR_NEWER
        using var indexArray = new NativeArray<int>(indexBuffer.count, Allocator.Temp);
        var indices = indexArray.AsSpan();
#else
            var indices = new NativeArray<int>(IndexNumPerTrail, Allocator.Temp);
#endif
        // 各Nodeの最後と次のNodeの最初はポリゴンを繋がないので-1
        var idx = 0;
        for (var iNode = 0; iNode < trailData.NodeNumPerTrail - 1; ++iNode)
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
        PropertyBlock.SetInt("_VertexPerTrail", trailController.vertexPerTrail);
        PropertyBlock.SetBuffer("_VertexBuffer", trailController.vertexBuffer);
        var renderParams = new RenderParams(material)
        {
            matProps = PropertyBlock,
            worldBounds = bounds
        };
        Graphics.RenderPrimitivesIndexedIndirect(renderParams, MeshTopology.Triangles, indexBuffer, argsBuffer);

        EditorApplication.QueuePlayerLoopUpdate();
    }
}
