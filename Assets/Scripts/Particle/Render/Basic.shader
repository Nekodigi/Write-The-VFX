//based on https://github.com/IndieVisualLab/UnityGraphicsProgramming
Shader "Particle/Basic"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        Pass
        {
            CGPROGRAM

            #pragma vertex   vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #define SHADER
            #include "Assets/Scripts/Particle/Particle.hlsl"
            #include "Assets/Scripts/Utils/Coordinate/Coordinate.hlsl"
            #include "Assets/Scripts/Utils/Lighting/Standard.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };


            v2f vert (appdata_tan v, uint instanceID : SV_InstanceID)
            {
                Particle p = _ParticleBuffer[instanceID];
                v2f o = initVert(v, p.size, p.rot, p.pos);

                o.color  = p.colDest;


                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //fixed4 col = shadedColor(i);//DONT WORK WITH HDRP
                return i.color;
            }

            ENDCG
        }
    }
}