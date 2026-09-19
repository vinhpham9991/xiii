Shader "Custom/URP_SpriteGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Base Map", 2D) = "white" {} // Compatibility for URP inspector
        _GlowColor ("Glow Color", Color) = (0, 0.5, 1, 1)
        _GlowThickness ("Glow Thickness", Range(0, 50)) = 5
        [Toggle] _IsGlowing ("Is Glowing", Float) = 0
        
        [Toggle] _IsFlashing ("Is Flashing", Float) = 0
        _FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _SpriteUVRect ("Sprite UV Rect", Vector) = (0, 0, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _GlowColor;
                float _GlowThickness;
                float _IsGlowing;
                float _IsFlashing;
                float4 _FlashColor;
                float4 _SpriteUVRect; // (minU, minV, maxU, maxV)
            CBUFFER_END
            
            // Hàm lấy mẫu an toàn: nếu vượt quá vùng UV của sprite (bị lem sang frame khác), trả về trong suốt
            half4 SampleSpriteAlpha(float2 uv)
            {
                if (uv.x < _SpriteUVRect.x || uv.x > _SpriteUVRect.z || uv.y < _SpriteUVRect.y || uv.y > _SpriteUVRect.w)
                    return half4(0,0,0,0);
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 col = SampleSpriteAlpha(i.uv) * _Color;
                
                if (_IsFlashing > 0.5 && col.a > 0.1)
                {
                    return half4(_FlashColor.rgb, col.a);
                }
                
                if (_IsGlowing > 0.5)
                {
                    // Lấy mẫu nhiều lần xung quanh (Multi-tap Radial Blur) để tạo viền dày và mờ
                    float2 texel = _MainTex_TexelSize.xy * _GlowThickness;
                    half blurAlpha = 0;
                    
                    // Vòng ngoài
                    blurAlpha += SampleSpriteAlpha(i.uv + float2( texel.x,  texel.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2( texel.x, -texel.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(-texel.x,  texel.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(-texel.x, -texel.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2( texel.x, 0)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(-texel.x, 0)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(0,  texel.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(0, -texel.y)).a;
                    
                    // Vòng trong
                    float2 texel2 = texel * 0.5;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2( texel2.x,  texel2.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2( texel2.x, -texel2.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(-texel2.x,  texel2.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(-texel2.x, -texel2.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2( texel2.x, 0)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(-texel2.x, 0)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(0,  texel2.y)).a;
                    blurAlpha += SampleSpriteAlpha(i.uv + float2(0, -texel2.y)).a;
                    
                    blurAlpha /= 16.0;

                    // Nếu pixel này trong suốt, nhưng có ảnh mờ ở xung quanh -> Viền Glow Mờ
                    if (col.a < 0.5 && blurAlpha > 0.01)
                    {
                        // Tạo hiệu ứng chớp tắt nhịp nhàng
                        float pulse = (sin(_Time.y * 4.0) + 1.0) * 0.5; 
                        pulse = 0.7 + 0.3 * pulse; // Dao động nhẹ từ 0.7 đến 1.0
                        
                        half4 glow = _GlowColor * pulse;
                        // Fade out ở rìa để mờ dần (saturate giới hạn giá trị từ 0 đến 1)
                        glow.a = saturate(blurAlpha * 3.0); 
                        
                        // Mix màu glow vào (nếu pixel có tí hình ảnh nào thì đè lên luôn)
                        return glow;
                    }
                }
                
                return col;
            }
            ENDHLSL
        }
    }
}
