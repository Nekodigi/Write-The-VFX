using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Runtime.InteropServices;

[ExecuteInEditMode]
public class FieldToVFXGraph : MonoBehaviour
{
    [SerializeField] private VisualEffect _effect;

    readonly int sourceProp = Shader.PropertyToID("sourceTex");
    readonly int sourceVecProp = Shader.PropertyToID("sourceVecTex");
    readonly int boundMinProp = Shader.PropertyToID("boundMin");
    readonly int boundMaxProp = Shader.PropertyToID("boundMax");

    IFieldController fieldController;

    private void Start()
    {
        fieldController = GetComponent<IFieldController>();

        if (_effect != null)
        {
            _effect.SetTexture(sourceProp, fieldController.source);
            _effect.SetTexture(sourceVecProp, fieldController.sourceVec);
            _effect.SetVector3(boundMinProp, fieldController.BoundaryMin);
            _effect.SetVector3(boundMaxProp, fieldController.BoundaryMax);
        }
    }

    private void LateUpdate()
    {
        Graphics.Blit(fieldController.dest, fieldController.source);
        Graphics.Blit(fieldController.destVec, fieldController.sourceVec);
    }
}
