using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(IParticleController))]
public class ParticleRenderer : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    Material material_;

    IParticleController particleController;

    // Start is called before the first frame update
    void Start()
    {
        particleController = GetComponent<IParticleController>();
        material_ = new Material(material);
    }

    // Update is called once per frame
    void Update()
    {
        material_.SetBuffer("_ParticleBuffer", particleController.particleBuffer);

        Graphics.DrawMeshInstancedProcedural(mesh, 0, material_, new Bounds(transform.position, Vector3.one * 10000), particleController.maxCount);
    }
}
