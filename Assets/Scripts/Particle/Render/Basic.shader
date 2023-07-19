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
            #define PARTICLE_R_ONLY
            #include "Assets/Scripts/Particle/Particle.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color  : TEXCOORD1;
            };

            float4x4 eulerAnglesToRotationMatrix(float3 angles)
            {
                float ch = cos(angles.y); float sh = sin(angles.y); // heading
                float ca = cos(angles.z); float sa = sin(angles.z); // attitude
                float cb = cos(angles.x); float sb = sin(angles.x); // bank
                // RyRxRz (Heading Bank Attitude)
                return float4x4(
                ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
                cb * sa, cb * ca, -sb, 0,
                -sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
                0, 0, 0, 1
                );
            }

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                Particle p = _ParticleBuffer[instanceID];

                v2f o;

                float4x4 object2world = (float4x4)0;
                object2world._11_22_33_44 = float4(p.size, 1.0);
                float4x4 rotMatrix =
                    eulerAnglesToRotationMatrix(p.rot);
                object2world = mul(rotMatrix, object2world);
                object2world._14_24_34 += p.pos;
                v.vertex = mul(object2world, v.vertex);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color  = p.col;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }

            ENDCG
        }
    }
}