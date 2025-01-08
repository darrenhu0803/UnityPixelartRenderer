Shader "Custom/NormalShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "Default"

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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD1;
            };
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
                o.normal = v.normal;
                return o;
            }

            float3 frag(v2f i) : SV_Target
            {
                return 0.5 * (normalize(i.normal) + 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
