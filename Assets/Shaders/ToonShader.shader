Shader "Custom/LightShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _RampTex ("Ramp Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            sampler2D _RampTex;
            float4 _Color;
            float4 _RampThresholds;
            float4 _LightColor0;

            v2f vert(appdata v)
            {
                v2f o;

                 // Calculate the world position of the object's origin
                float4 originClipPos = TransformObjectToHClip(float4(0, 0, 0, 1));
                float2 originScreenPos = originClipPos.xy / originClipPos.w;
                originScreenPos = originScreenPos * 0.5f + 0.5f; // Transform from [-1,1] to [0,1]

                // Snap the origin position
                float2 snappedScreenPos;
                snappedScreenPos.x = round(originScreenPos.x * _ScreenParams.x / 1) * 1 / _ScreenParams.x;
                snappedScreenPos.y = round(originScreenPos.y * _ScreenParams.y / 1) * 1 / _ScreenParams.y;

                // Calculate the offset needed to snap the object's origin
                float2 offset = snappedScreenPos - originScreenPos;


                float4 clipPos = TransformObjectToHClip(v.vertex);
                clipPos.xy += offset * clipPos.w * 2.0;

                o.pos = clipPos;
                o.uv = v.uv;
                //o.worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {

                half4 tex = tex2D(_MainTex, i.uv) * _Color;

                float3 normal = normalize(i.worldNormal);

                float3 lightDir = normalize(GetMainLight().direction);
                float NdotL = max(0, dot(normal, lightDir));
                half3 ramp = tex2D(_RampTex, float2(NdotL * 0.8 + 0.1, 0)).rgb;

                float4 color;
                color.rgb = tex.rgb * GetMainLight().color.rgb * ramp;
                color.a = tex.a;

                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
