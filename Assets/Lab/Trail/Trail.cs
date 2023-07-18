using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Pool;
using UnityEngine.XR;

public class Trail : MonoBehaviour
{
    public int trailNum;
    public int vertexPerTrail;
    int vertexNum;
    int IndexNumPerTrail;

    public Material material;
    //public Material matVert;
    //public Mesh mesh;

    public ComputeShader createVertexCS;


    protected GraphicsBuffer vertexBuffer;
    protected GraphicsBuffer indexBuffer;
    protected GraphicsBuffer argsBuffer;

    public MaterialPropertyBlock PropertyBlock;


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
        var kernel = createVertexCS.FindKernel("CreateVertex");
        createVertexCS.SetInt("_VertexPerTrail", vertexPerTrail);
        createVertexCS.SetBuffer(kernel, "_VertexBuffer", vertexBuffer);

        createVertexCS.Dispatch(kernel, vertexNum / 16, 1, 1);
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
        //Debug.Log(PropertyBlock);
        //Debug.Log();
        //PropertyBlock = new MaterialPropertyBlock();
        //Debug.Log(vertexBuffer.count);
        /*matVert.SetInt("_VertexPerTrail", vertexPerTrail);
        matVert.SetBuffer("_VertexBuffer", vertexBuffer);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, matVert, new Bounds(Vector3.zero, Vector3.one * 100), vertexNum);
        */
        
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
