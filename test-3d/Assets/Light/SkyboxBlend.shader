Shader "Custom/SkyboxBlend"
{
    Properties
    {
        _DaySkybox ("Day Skybox", CUBE) = "white" {}
        _NightSkybox ("Night Skybox", CUBE) = "black" {}
        _Blend ("Blend", Range(0, 1)) = 0.5
        _Exposure ("Exposure", Float) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            samplerCUBE _DaySkybox;
            samplerCUBE _NightSkybox;
            half4 _DaySkybox_HDR;
            half4 _NightSkybox_HDR;
            float _Blend;
            float _Exposure;
            float _Rotation;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                o.pos = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample both skyboxes
                half4 dayColor = texCUBE(_DaySkybox, i.texcoord);
                half4 nightColor = texCUBE(_NightSkybox, i.texcoord);

                // Decode HDR
                dayColor.rgb = DecodeHDR(dayColor, _DaySkybox_HDR);
                nightColor.rgb = DecodeHDR(nightColor, _NightSkybox_HDR);

                // Blend between day and night
                half3 color = lerp(nightColor.rgb, dayColor.rgb, _Blend);

                // Apply exposure
                color *= _Exposure;

                return half4(color, 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}