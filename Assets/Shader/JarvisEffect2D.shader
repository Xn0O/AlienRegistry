Shader "Custom/URP_JarvisEffect2D"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1, 1, 1, 1)

        // --- 贾维斯效果控制参数 ---

        // 1. 失真与色差
        _ChromaticAberration ("Chromatic Aberration Intensity", Range(0, 1)) = 0.35
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.3
        _GlitchTimeScale ("Glitch Speed", Float) = 15.0

        // 2. 基础 CRT 质感
        _ScanlineCount ("Scanline Count", Float) = 400.0
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.15
        _NoiseSpeed ("Noise Speed", Float) = 10.0

        // 3. 动态网格基底
        _GridOpacity ("Grid Opacity", Range(0, 1)) = 0.1
        _GridScale ("Grid Scale", Float) = 30.0

        // --- UI 遮罩支持 ---
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _ChromaticAberration;
                float _GlitchIntensity;
                float _GlitchTimeScale;
                float _ScanlineCount;
                float _ScanlineIntensity;
                float _NoiseIntensity;
                float _NoiseSpeed;
                float _GridOpacity;
                float _GridScale;
            CBUFFER_END

            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _BaseColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // URP 2D Jarvis Shader 核心实现：

                // --- 1. 动态故障失真 (Horizontal Glitch) ---
                // 使用基于时间的随机函数，计算随机的水平偏移。
                float glitchTime = _Time.y * _GlitchTimeScale;
                // 产生一个介于 [-1, 1] 之间的随机偏移量，仅对 UV.x 产生影响。
                float glitchOffset = rand(float2(IN.uv.y + glitchTime, glitchTime)) * 2.0 - 1.0;
                // 仅在某些随机垂直切片上产生故障。
                float glitchMask = rand(float2(floor(IN.uv.y * 50.0) + glitchTime, glitchTime));

                // 将故障偏移量应用到 UV.x 上，并乘以一个非常明显的强度。
                float2 uvShift = float2(glitchOffset * _GlitchIntensity * 0.1 * step(0.9, glitchMask), 0.0);
                float2 glitchedUV = IN.uv + uvShift;

                // --- 2. 动态色差采样 (Chromatic Aberration) ---
                // 分别采样 R, G, B 三个通道。R 和 B 通道分别向左和右偏移。
                half redSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,
                                    glitchedUV + float2(-_ChromaticAberration * 0.015, 0.0)).r;
                half greenSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, glitchedUV).g;
                half blueSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,
                glitchedUV + float2(_ChromaticAberration * 0.015, 0.0)).b;
                half alphaSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, glitchedUV).a;

                // 将分离出的彩色样本合成颜色。
                half4 color = half4(redSample, greenSample, blueSample, alphaSample) * IN.color;

                // 剔除完全透明像素。
                if (color.a <= 0.01) return color;

                // --- 3. 动态网格基底 (Dynamic Grid) ---
                // 创建一个动态、旋转的全息网格底纹。
                float2 gridUV = IN.uv * _GridScale + _Time.y * float2(1.0, 1.0) * 0.1;
                float2 gridPattern = abs(sin(gridUV * 3.14159));
                float gridIntensity = gridPattern.x * gridPattern.y;
                // 将网格应用到颜色上。
                color.rgb += (gridIntensity * _GridOpacity * half3(0.1, 1.0, 0.1));

                // --- 4. 强烈 CRT 质感 (Agessive CRT finish) ---
                // 使用之前方案的扫描线和噪点，但默认值更激进。
                float scanline = sin(IN.uv.y * _ScanlineCount);
                scanline = scanline * 0.5 + 0.5;
                float scanlineMultiplier = lerp(1.0, scanline, _ScanlineIntensity);
                color.rgb *= scanlineMultiplier;

                // 噪点。
                float2 noiseUV = glitchedUV + float2(0.0, _Time.y * _NoiseSpeed);
                float noiseValue = rand(noiseUV);
                noiseValue = noiseValue * 2.0 - 1.0;
                color.rgb += noiseValue * _NoiseIntensity * half3(1.0, 1.0, 1.0); // 默认彩色噪点

                // 复古暗角。
                float2 centeredUV = IN.uv - 0.5;
                float vignette = 1.0 - dot(centeredUV, centeredUV);
                color.rgb *= smoothstep(0.6, 1.0, vignette);

                return color;
            }
            ENDHLSL
        }
    }
}