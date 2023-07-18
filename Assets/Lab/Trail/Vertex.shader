//based on https://github.com/IndieVisualLab/UnityGraphicsProgramming
Shader "GpuTrail/Vertex"
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

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color  : TEXCOORD1;
            };

            struct Vertex
            {
                float3 pos;
			    float2 uv;
			    half4 color;
            };


		      StructuredBuffer<Vertex> _VertexBuffer;

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                Vertex p = _VertexBuffer[instanceID];

                v2f o;

                //float4x4 object2world = (float4x4)0;
                //object2world._14_24_34 += p.pos;
                //v.vertex = mul(object2world, v.vertex);

                o.vertex = UnityObjectToClipPos(v.vertex+p.pos);
                o.color  = p.color;

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