#ifndef HEIGHTMAP_INCLUDED
#define HEIGHTMAP_INCLUDED

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "Assets/Scripts/Utils/Lighting/BRDF.hlsl"

struct v2f
{
    float4 vertex   : SV_POSITION;
    float3 vertexW  : TEXCOORD0;
    float3 normal   : TEXCOORD1;
    float3 tangent  : TEXCOORD2;
    float3 binormal : TEXCOORD3;
    float2 uv       : TEXCOORD4;
};

sampler2D _MainTex;
sampler2D _OcclusionTex;
sampler2D _NormalMap;
sampler2D _HeightMap;

float4 _MainColor;
float4 _SpecularColor;
float  _Metalness;
float  _Roughness;
float  _Fresnel;
float  _Occlusion;
float  _Height;

v2f vert(appdata_tan v)
{
    v2f o;

    o.vertex   = UnityObjectToClipPos(v.vertex);
    o.vertexW  = mul(unity_ObjectToWorld, v.vertex);
    o.normal   = UnityObjectToWorldNormal(v.normal);
    o.tangent  = UnityObjectToWorldDir(v.tangent);
    o.binormal = cross(v.normal, v.tangent) * v.tangent.w;
    o.binormal = UnityObjectToWorldDir(o.binormal);
    o.uv       = v.texcoord;

    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    UNITY_LIGHT_ATTENUATION(attenuation, i, i.vertexW)

    float3 normal   = normalize(i.normal);
    float3 tangent  = normalize(i.tangent);
    float3 binormal = normalize(i.binormal);
    float3 light    = normalize(_WorldSpaceLightPos0.w == 0 ?
                                _WorldSpaceLightPos0.xyz :
                                _WorldSpaceLightPos0.xyz - i.vertexW);
    float3 view     = normalize(_WorldSpaceCameraPos - i.vertexW);
    float3 hlf      = normalize(light + view);

    float3x3 worldToTangent = float3x3(tangent, binormal, normal);
    float3x3 tangentToWorld = transpose(worldToTangent);

    float2 uv = i.uv + ParallaxOffset(tex2D(_HeightMap, i.uv), _Height, mul(worldToTangent, view));
    normal = normalize(mul(tangentToWorld, UnpackNormal(tex2D(_NormalMap, uv))));

    float dotNL = dot(normal, light);
    float dotNV = dot(normal, view);
    float dotNH = dot(normal, hlf);
    float dotVH = dot(view,   hlf);

    float dTerm = dTermGGX(dotNH, _Roughness);
    float gTerm = gTermSmithJoint(dotNL, dotNV, _Roughness);
    float fTerm = fresnelSchlick(dotNV, _Fresnel);

    float  diffuse  = saturate(dotNL);
    float  specular = saturate(dTerm * gTerm * fTerm / (dotNL * dotNV * 4) * dotNL);
    float3 ambient  = ShadeSH9(half4(normal, 1));

    fixed4 baseColor     = _MainColor * tex2D(_MainTex, uv);
    fixed4 diffuseColor  = diffuse * baseColor * _LightColor0 * attenuation;
    fixed4 specularColor = specular * _SpecularColor * _LightColor0 * attenuation;
    fixed4 ambientColor  = fixed4(0 ,0, 0, 1);

    #ifdef UNITY_PASS_FORWARDBASE

    float3 rflt  = normalize(reflect(-view, normal));

    fixed4 diffuseColorI = baseColor;
           diffuseColorI.rgb *= ambient;

    fixed specCubeLevel = UNITY_SPECCUBE_LOD_STEPS * _Roughness;

    fixed4 specularColorI = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, rflt, specCubeLevel);
           specularColorI.rgb = DecodeHDR(specularColorI, unity_SpecCube0_HDR);
           specularColorI.rgb *= _SpecularColor;

    ambientColor = lerp(diffuseColorI, specularColorI, _Metalness);
    ambientColor.rgb += ambient * _SpecularColor * fTerm * (1 - _Roughness);
    ambientColor.rgb *= pow(tex2D(_OcclusionTex, i.uv), _Occlusion); 

    #endif

    fixed4 color = lerp(diffuseColor, specularColor, _Metalness) + ambientColor;

    return color;
}

#endif // HEIGHTMAP_INCLUDED