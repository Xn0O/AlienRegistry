Shader "Custom/URP_CRTEffect2D"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)

        // --- CRT 效果控制参数 ---
        _ScanlineCount ("Scanline Count", Float) = 800.0
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.15
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.06
        _NoiseSpeed ("Noise Speed", Float) = 10.0

        // --- UI 遮罩支持 (勿删，保证在 Canvas 里不穿帮) ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        // 支持 UI Mask 遮罩
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 引入 URP 核心库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            // URP 的纹理采样宏
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // 将变量放入 CBUFFER 以兼容 SRP Batcher，优化性能
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _ScanlineCount;
                float _ScanlineIntensity;
                float _NoiseIntensity;
                float _NoiseSpeed;
            CBUFFER_END

            // 伪随机数生成器
            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // 转换到裁剪空间
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _BaseColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // URP 的纹理采样方式
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                // 剔除完全透明的像素
                if (color.a <= 0.01) return color;

                // 1. 扫描线效果 (Scanlines)
                float scanline = sin(IN.uv.y * _ScanlineCount);
                scanline = scanline * 0.5 + 0.5; // 映射到 0~1
                float scanlineMultiplier = lerp(1.0, scanline, _ScanlineIntensity);
                color.rgb *= scanlineMultiplier;

                // 2. 动态噪点 (Noise)
                float2 noiseUV = IN.uv + float2(0.0, _Time.y * _NoiseSpeed);
                float noiseValue = rand(noiseUV);
                noiseValue = noiseValue * 2.0 - 1.0;
                color.rgb += noiseValue * _NoiseIntensity;

                // 3. 复古暗角 (Vignette)
                float2 centeredUV = IN.uv - 0.5;
                float vignette = 1.0 - dot(centeredUV, centeredUV);
                color.rgb *= smoothstep(0.6, 1.0, vignette);

                return color;
            }
            ENDHLSL
        }
    }
}