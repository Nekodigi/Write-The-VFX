using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Runtime.InteropServices;

[ExecuteInEditMode]
public class ParticleToVFXGraph : MonoBehaviour
{
    [SerializeField] private VisualEffect _effect;

    public ComputeShader computeShader;
    ComputeShader computeShader_;

    GraphicsBuffer posBuffer;
    GraphicsBuffer velBuffer;
    GraphicsBuffer colBuffer;
    GraphicsBuffer rotBuffer;
    GraphicsBuffer sizeBuffer;
    GraphicsBuffer weightsBuffer;
    GraphicsBuffer lifesBuffer;

    int maxCount = 0;

    private Vector3[] vec3Array;
    private Vector4[] vec4Array;
    private Vector2[] vec2Array;


    readonly int posProp = Shader.PropertyToID("posBuffer");
    readonly int velProp = Shader.PropertyToID("velBuffer");
    readonly int colProp = Shader.PropertyToID("colBuffer");//vec4
    readonly int rotProp = Shader.PropertyToID("rotBuffer");
    readonly int sizeProp = Shader.PropertyToID("sizeBuffer");
    readonly int weightsProp = Shader.PropertyToID("weightsBuffer");//weightDest, field, damp
    readonly int lifesProp = Shader.PropertyToID("lifesBuffer");//vec2  life, age, disable

    readonly int maxCountProp = Shader.PropertyToID("maxCount");

    IParticleController particleController;

    int kernelUpdate;

    protected const int THREAD_NUM = 16;

    private void Start()
    {
        particleController = GetComponent<IParticleController>();
        computeShader_ = computeShader;
        kernelUpdate = computeShader.FindKernel("Update");


        maxCount = particleController.maxCount;
        vec3Array = new Vector3[maxCount];
        vec2Array = new Vector2[maxCount];
        vec4Array = new Vector4[maxCount];

        posBuffer = Vec3Buffer();
        velBuffer = Vec3Buffer();
        rotBuffer = Vec3Buffer();
        sizeBuffer = Vec3Buffer();
        weightsBuffer = Vec3Buffer();
        lifesBuffer = Vec3Buffer();

        colBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxCount, Marshal.SizeOf(new Vector4()));
        colBuffer.SetData(vec4Array);

        if (_effect != null)
        {
            _effect.SetGraphicsBuffer(posProp, posBuffer);
            _effect.SetGraphicsBuffer(velProp, velBuffer);
            _effect.SetGraphicsBuffer(colProp, colBuffer);
            _effect.SetGraphicsBuffer(rotProp, rotBuffer);
            _effect.SetGraphicsBuffer(sizeProp, sizeBuffer);
            _effect.SetGraphicsBuffer(weightsProp, weightsBuffer);
            _effect.SetGraphicsBuffer(lifesProp, lifesBuffer);

            _effect.SetInt(maxCountProp, maxCount);
        }
    }

    private void LateUpdate()
    {
        SetTexturesToShader(kernelUpdate);
        SetBufferToShader(kernelUpdate);
        computeShader_.Dispatch(kernelUpdate, maxCount / THREAD_NUM, 1, 1);

    }

    void SetTexturesToShader(int kernelId)
    {
        particleController.SetTextureToShader(kernelId, computeShader);
    }

    void SetBufferToShader(int kernelId)
    {
        computeShader_.SetBuffer(kernelId, "_ParticleBuffer", particleController.particleBuffer);
        computeShader_.SetBuffer(kernelId, "_PosBuffer", posBuffer);
        computeShader_.SetBuffer(kernelId, "_VelBuffer", velBuffer);
        computeShader_.SetBuffer(kernelId, "_ColBuffer", colBuffer);
        computeShader_.SetBuffer(kernelId, "_RotBuffer", rotBuffer);
        computeShader_.SetBuffer(kernelId, "_SizeBuffer", sizeBuffer);
        computeShader_.SetBuffer(kernelId, "_WeightsBuffer", weightsBuffer);
        computeShader_.SetBuffer(kernelId, "_LifesBuffer", lifesBuffer);

    }

    GraphicsBuffer Vec3Buffer()
    {
        GraphicsBuffer buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxCount, Marshal.SizeOf(new Vector3()));
        buffer.SetData(vec3Array);
        return buffer;
    }
}
