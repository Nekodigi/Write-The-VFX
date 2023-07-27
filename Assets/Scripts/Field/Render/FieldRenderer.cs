using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(IFieldController))]
public class FieldRenderer : MonoBehaviour
{
    public Renderer renderer_;//SET VECTOR SHADER for best result
    public FieldType target;
    public Vector4 VecMin;
    public Vector4 VecMax = new Vector4(1,1,1,1);
    public Vector2Int interval = new Vector2Int(1,1);
    public bool asVector;

    public ComputeShader computeShader;
    public Mesh mesh;
    public Material material;

    RenderTexture targetRT;

    VectorVisParticleController vectorVisControler;

    IFieldController fieldController;

    // Start is called before the first frame update
    void Start()
    {
        fieldController = GetComponent<IFieldController>();
        transform.localScale = -(fieldController.BoundaryMax - fieldController.BoundaryMin) / 10;
        transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
        vectorVisControler = new VectorVisParticleController(interval, renderer_.bounds.max, renderer_.bounds.min, computeShader, fieldController.resolution);
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (target)
        {
            case FieldType.source:
                targetRT = fieldController.source;
                break;
            case FieldType.dest:
                targetRT = fieldController.dest;
                break;
            case FieldType.sourceVec:
                targetRT = fieldController.sourceVec;
                break;
            case FieldType.destVec:
                targetRT = fieldController.destVec;
                break;
        }
        Material mat = GetComponent<Renderer>().sharedMaterial;
        mat.SetVector("_VecMin", VecMin);
        mat.SetVector("_VecMax", VecMax);
        mat.mainTexture = targetRT;

        if (asVector)
        {
            renderer_.enabled = false;
            vectorVisControler.Update_(targetRT, VecMin, VecMax);
            material.SetBuffer("_ParticleBuffer", vectorVisControler.particleBuffer);

            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 10000), vectorVisControler.maxCount);
            //Debug.Log(vectorVisControler.maxCount);
        }
        else
        {
            renderer_.enabled = true;
        }
        
    }
}
