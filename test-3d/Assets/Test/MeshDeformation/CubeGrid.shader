// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Shader "Custom/Rounded Cube Grid HLSL"
// {
//     Properties
//     {
//         _Color ("Color", Color) = (1,1,1,1)
//         _MainTex ("Albedo (RGB)", 2D) = "white" {}
//         _Glossiness ("Smoothness", Range(0,1)) = 0.5
//         _Metallic ("Metallic", Range(0,1)) = 0.0
//         [KeywordEnum(X, Y, Z)] _Faces("Face", Float) = 0
//     }

//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }
//         LOD 200

//         Pass
//         {
//             Tags { "LightMode" = "ForwardBase" }

//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma multi_compile_fwdbase
//             #pragma shader_feature _FACES_X _FACES_Y _FACES_Z
//             #pragma target 3.0

//             #include "UnityCG.cginc"
//             #include "Lighting.cginc"

//             sampler2D _MainTex;
//             float4 _MainTex_ST;
//             fixed4 _Color;
//             half _Glossiness;
//             half _Metallic;

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 float3 normal : NORMAL;
//                 float4 color : COLOR;
//             };

//             struct v2f
//             {
//                 float4 pos : SV_POSITION;
//                 float3 worldPos : TEXCOORD0;
//                 float3 worldNormal : TEXCOORD1;
//                 float2 uv : TEXCOORD2;
//             };

//             v2f vert (appdata v)
//             {
//                 v2f o;
//                 o.pos = UnityObjectToClipPos(v.vertex);
//                 o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
//                 o.worldNormal = UnityObjectToWorldNormal(v.normal);

//                 #if defined(_FACES_X)
//                     o.uv = v.color.yz * 255;
//                 #elif defined(_FACES_Y)
//                     o.uv = v.color.xz * 255;
//                 #elif defined(_FACES_Z)
//                     o.uv = v.color.xy * 255;
//                 #else
//                     o.uv = v.color.xy * 255;
//                 #endif

//                 return o;
//             }

//             fixed4 frag (v2f i) : SV_Target
//             {
//                 // 采样颜色
//                 fixed4 texCol = tex2D(_MainTex, i.uv) * _Color;

//                 // 基础光照（近似 Standard）
//                 fixed3 albedo = texCol.rgb;
//                 fixed3 normal = normalize(i.worldNormal);
//                 fixed3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

//                 // 主光源
//                 UnityLight light;
//                 UNITY_INITIALIZE_OUTPUT(UnityLight, light);
//                 light.color = _LightColor0.rgb;
//                 light.dir = normalize(_WorldSpaceLightPos0.xyz);

//                 fixed3 diffuse = albedo * light.color * max(0, dot(normal, light.dir));

//                 // 简化版 Specular（根据 Glossiness）
//                 fixed3 halfDir = normalize(light.dir + viewDir);
//                 float spec = pow(max(dot(normal, halfDir), 0), 16 * _Glossiness + 1);
//                 fixed3 specular = light.color * spec * _Metallic;

//                 fixed3 color = diffuse + specular;

//                 return fixed4(color, texCol.a);
//             }
//             ENDCG
//         }

//         // 额外 Pass 支持阴影
//         UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
//     }
// }
