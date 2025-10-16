Shader "Custom/Rounded Cube Grid URP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [KeywordEnum(X, Y, Z)] _Faces("Faces", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // URP keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            // Face selection keyword
            #pragma multi_compile _ _FACES_X _FACES_Y _FACES_Z

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 cubeUV : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 color : TEXCOORD3;
                float fogCoord : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float _Smoothness;
                float _Metallic;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Transform position and normal to world space
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.color = IN.color;
                
                // Calculate cube UV based on face selection and vertex color
                #if defined(_FACES_X)
                    OUT.cubeUV = IN.color.yz * 255.0;
                #elif defined(_FACES_Y)
                    OUT.cubeUV = IN.color.xz * 255.0;
                #elif defined(_FACES_Z)
                    OUT.cubeUV = IN.color.xy * 255.0;
                #else
                    // Default to Z faces
                    OUT.cubeUV = IN.color.xy * 255.0;
                #endif
                
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample texture
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.cubeUV) * _Color;
                
                // Basic lighting data
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = IN.positionWS;
                lightingInput.normalWS = normalize(IN.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                lightingInput.fogCoord = IN.fogCoord;
                lightingInput.bakedGI = SampleSH(lightingInput.normalWS);

                // Surface data
                SurfaceData surfaceData;
                surfaceData.albedo = col.rgb;
                surfaceData.alpha = col.a;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.emission = 0;
                surfaceData.occlusion = 1;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.specular = half3(0, 0, 0);

                // Apply URP lighting
                half4 finalColor = UniversalFragmentPBR(lightingInput, surfaceData);
                
                // Apply fog
                finalColor.rgb = MixFog(finalColor.rgb, IN.fogCoord);
                
                return finalColor;
            }
            ENDHLSL
        }

        // Shadow caster pass for receiving shadows
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}

// Shader "Custom/Rounded Cube Grid" {
// 	Properties {
// 		_Color ("Color", Color) = (1,1,1,1)
// 		_MainTex ("Albedo (RGB)", 2D) = "white" {}
// 		_Glossiness ("Smoothness", Range(0,1)) = 0.5
// 		_Metallic ("Metallic", Range(0,1)) = 0.0
//         [KeywordEnum(X, Y, Z)] _Faces("Face", Float) = 0
// 	}
// 	SubShader {
// 		Tags { "RenderType"="Opaque" }
// 		LOD 200
		
// 		CGPROGRAM
//         #pragma shader_feature _FACES_X _FACES_Y _FACES_Z
// 		#pragma surface surf Standard fullforwardshadows vertex:vert
// 		#pragma target 3.0

// 		sampler2D _MainTex;

// 		struct Input {
// 			float2 cubeUV;
// 		};

// 		half _Glossiness;
// 		half _Metallic;
// 		fixed4 _Color;

// 		void vert (inout appdata_full v, out Input o) {
// 			UNITY_INITIALIZE_OUTPUT(Input, o);
// 			#if defined(_FACES_X)
// 				o.cubeUV = v.color.yz * 255;
// 			#elif defined(_FACES_Y)
// 				o.cubeUV = v.color.xz * 255;
// 			#elif defined(_FACES_Z)
// 				o.cubeUV = v.color.xy * 255;
// 			#endif
// 		}

// 		void surf (Input IN, inout SurfaceOutputStandard o) {
// 			fixed4 c = tex2D(_MainTex, IN.cubeUV) * _Color;
// 			o.Albedo = c.rgb;
// 			o.Metallic = _Metallic;
// 			o.Smoothness = _Glossiness;
// 			o.Alpha = c.a;
// 		}
// 		ENDCG
// 	} 
// 	FallBack "Diffuse"
// }