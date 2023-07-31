Shader "ScalingShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag

           #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _ScalingRatio;
            float _ScalingRatioX;
            float _ScalingRatioY;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 scale;
                if(_ScalingRatio != 0){
                    scale = float2(_ScalingRatio, _ScalingRatio);
                }else{
                    scale = float2(_ScalingRatioX, _ScalingRatioY);
                }
                i.uv = (i.uv - float2(0.5, 0.5)) * scale + float2(0.5, 0.5);
                fixed4 col = tex2D(_MainTex, i.uv);
                col.w = 1;
                return col;
            }
            ENDCG
        }
    }
}