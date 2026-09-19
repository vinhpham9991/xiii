Shader "Custom/SelectionCircle"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.6, 0, 1) // Vàng cam
        _Radius ("Radius", Range(0, 0.5)) = 0.45
        _Thickness ("Thickness", Range(0, 0.5)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Radius;
                float _Thickness;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Dời tâm về giữa Quad
                float2 uv = i.uv - 0.5;
                float dist = length(uv);
                
                // Vẽ viền tròn mượt bằng smoothstep
                float ring = smoothstep(_Radius + _Thickness, _Radius, dist) - smoothstep(_Radius, _Radius - _Thickness, dist);
                
                // Hiệu ứng chớp nháy nhẹ
                float pulse = (sin(_Time.y * 5.0) + 1.0) * 0.5;
                pulse = 0.6 + 0.4 * pulse;
                
                return half4(_Color.rgb * pulse, ring * _Color.a);
            }
            ENDHLSL
        }
    }
}
