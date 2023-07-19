using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Pool;
using UnityEngine.XR;

public class TrailGPU : MonoBehaviour
{
    public int trailNum;
    public int vertexPerTrail;
    int vertexNum;
    int IndexNumPerTrail;

    public Material material;
    //public Material matVert;
    //public Mesh mesh;

    public ComputeShader createVertexCS;

    protected GraphicsBuffer nodeBuffer;
    protected GraphicsBuffer trailBuffer;

    protected GraphicsBuffer vertexBuffer;
    protected GraphicsBuffer indexBuffer;
    protected GraphicsBuffer argsBuffer;

    public MaterialPropertyBlock PropertyBlock;
    public struct Node
    {
        public Vector3 pos;
        public float spawnTime;
        public Color color;
    }

    public struct Trail
    {
        public float spawnTime;
        public int totalInputNum;
    }

    public struct Vertex //NODE AND VERTEX ARE DIFFERENT!
    {
        public Vector3 pos;
        public Vector2 uv;
        public Color color;
    }

    public Bounds bounds = new(Vector3.zero, Vector3.one * 100000f);


    // Start is called before the first frame update
    void Start()
    {
        vertexNum = trailNum * vertexPerTrail;
        IndexNumPerTrail = (vertexPerTrail - 1) * 6;
        InitBufferIfNeed();

        
    }

    protected void InitBufferIfNeed()
    {
        if ((vertexBuffer != null) && (vertexBuffer.count == vertexNum))
        {
            return;
        }
        PropertyBlock = new();
        vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexNum, Marshal.SizeOf<Vertex>()); // 1 node to 2 vtx(left,right)
        vertexBuffer.Fill(default(Vertex));
        trailBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, trailNum, Marshal.SizeOf<Trail>());
        trailBuffer.Fill(default(Trail));
        nodeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexNum/2, Marshal.SizeOf<Node>());
        nodeBuffer.Fill(default(Node));

        indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index | GraphicsBuffer.Target.Structured, IndexNumPerTrail, Marshal.SizeOf<uint>()); // 1 node to 2 triangles(6vertexs)
#if UNITY_2022_2_OR_NEWER
        using var indexArray = new NativeArray<int>(indexBuffer.count, Allocator.Temp);
        var indices = indexArray.AsSpan();
#else
            var indices = new NativeArray<int>(IndexNumPerTrail, Allocator.Temp);
#endif
        // 各Nodeの最後と次のNodeの最初はポリゴンを繋がないので-1
        var idx = 0;
        for (var iNode = 0; iNode < vertexPerTrail/2 - 1; ++iNode)
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

    protected virtual void ReleaseBuffer()
    {
        trailBuffer?.Release();
        nodeBuffer?.Release();
    }
    public void Dispose()
    {
        ReleaseBuffer();
    }

    public void ResetArgsBuffer()
    {
        InitBufferIfNeed();

        using var _ = ListPool<int>.Get(out var argsList);

        argsList.Add(IndexNumPerTrail);
        argsList.Add(trailNum * (IsSinglePassInstancedRendering ? 2 : 1));
        argsList.Add(0);
        argsList.Add(0);
        argsList.Add(0);

        argsBuffer.SetData(argsList);
    }

    // Update is called once per frame
    protected virtual void LateUpdate()
    {
        var toCameraDir = default(Vector3);
        if (Camera.main.orthographic)
        {
            toCameraDir = -Camera.main.transform.forward;
        }


        createVertexCS.SetFloat("_Time", Time.time);

        createVertexCS.SetVector("_ToCameraDir", toCameraDir);
        createVertexCS.SetVector("_CameraPos", Camera.main.transform.position);

        var kernel = createVertexCS.FindKernel("CreateNodeTrail");
        var kernelVertex = createVertexCS.FindKernel("CreateVertex");
        createVertexCS.SetInt("_VertexPerTrail", vertexPerTrail);
        createVertexCS.SetBuffer(kernel, "_NodeBuffer", nodeBuffer);
        createVertexCS.SetBuffer(kernel, "_TrailBuffer", trailBuffer);
        createVertexCS.SetBuffer(kernel, "_VertexBuffer", vertexBuffer);

        createVertexCS.Dispatch(kernel, vertexNum / 16 / 2, 1, 1);

        createVertexCS.SetBuffer(kernelVertex, "_NodeBuffer", nodeBuffer);
        createVertexCS.SetBuffer(kernelVertex, "_TrailBuffer", trailBuffer);
        createVertexCS.SetBuffer(kernelVertex, "_VertexBuffer", vertexBuffer);

        createVertexCS.Dispatch(kernelVertex, vertexNum / 16 / 2, 1, 1);


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
