Shader "GpuTrail/Trail" {
Properties {
	_StartColor("StartColor", Color) = (1,1,1,1)
	_EndColor("EndColor", Color) = (0,0,0,1)
}
   
SubShader {
	Tags { "Queue" = "Transparent" }

	Pass{
		Cull Off Fog { Mode Off }
		ZWrite Off
		Blend SrcAlpha One


		CGPROGRAM
		#pragma target 5.0

		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		struct Vertex
		{
			float3 pos;
			float2 uv;
			half4 color;
		};

		uint _VertexPerTrail;
		StructuredBuffer<Vertex> _VertexBuffer;


		Vertex GetVertex(uint vertexIdx, uint trailIdx)
		{
			uint idx = vertexIdx +  (trailIdx * _VertexPerTrail);
			return _VertexBuffer[idx];
		}

		struct vs_out {
			float4 pos : SV_POSITION;
			float4 col : COLOR;
			float2 uv  : TEXCOORD;
		};

		vs_out vert (uint vId : SV_VertexID, uint iId : SV_InstanceID)
		{
			vs_out Out;
			Vertex vtx = GetVertex(vId, iId);

			Out.pos = UnityObjectToClipPos(float4(vtx.pos, 1.0));
			Out.uv = vtx.uv;
			//Out.pos = float4(vId+iId, iId, 0);
			//Out.uv = float2(0, 0);
			Out.col = float4(1,1,1,1);
			//Out.col = vtx.color;

			return Out;
		}

		fixed4 frag (vs_out In) : COLOR0
		{
			return In.col;
		}

		ENDCG
   
	   }
	}
}

