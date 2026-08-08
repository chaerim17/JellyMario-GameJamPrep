Shader "JellyMario/2D/JellyGround"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        [HideInInspector] _Color("Tint", Color) = (1, 1, 1, 1)
        [HideInInspector] _RendererColor("Renderer Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0

        [HideInInspector] _ImpactData0("Impact Data 0", Vector) = (0, 0, -1000, 0)
        [HideInInspector] _ImpactData1("Impact Data 1", Vector) = (0, 0, -1000, 0)
        [HideInInspector] _ImpactData2("Impact Data 2", Vector) = (0, 0, -1000, 0)
        [HideInInspector] _ImpactData3("Impact Data 3", Vector) = (0, 0, -1000, 0)

        [HideInInspector] _ImpactNormal0("Impact Normal 0", Vector) = (0, 1, 0, 0)
        [HideInInspector] _ImpactNormal1("Impact Normal 1", Vector) = (0, 1, 0, 0)
        [HideInInspector] _ImpactNormal2("Impact Normal 2", Vector) = (0, 1, 0, 0)
        [HideInInspector] _ImpactNormal3("Impact Normal 3", Vector) = (0, 1, 0, 0)

        _ImpactFrequency("Impact Frequency", Float) = 2.5
        _ImpactSpeed("Impact Speed", Float) = 2.2
        _ImpactFalloff("Impact Falloff", Float) = 0.55
        _ImpactDecay("Impact Decay", Float) = 0.7
        _WaveHeightMultiplier("Wave Height Multiplier", Float) = 2.5
        _MaxCombinedWaveOffset("Max Combined Wave Offset", Float) = 1.25
        [HideInInspector] _WaveDuration("Wave Duration", Float) = 3.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex JellyVertex
            #pragma fragment JellyFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ SKINNED_SPRITE

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _ImpactData0;
                float4 _ImpactData1;
                float4 _ImpactData2;
                float4 _ImpactData3;
                float4 _ImpactNormal0;
                float4 _ImpactNormal1;
                float4 _ImpactNormal2;
                float4 _ImpactNormal3;
                float _ImpactFrequency;
                float _ImpactSpeed;
                float _ImpactFalloff;
                float _ImpactDecay;
                float _WaveHeightMultiplier;
                float _MaxCombinedWaveOffset;
                float _WaveDuration;
            CBUFFER_END

            float2 CalculateWaveOffset(
                float2 vertexPosition,
                float4 impactData,
                float4 impactNormalData
            )
            {
                float strength = impactData.w;

                if (strength <= 0.00001)
                    return float2(0.0, 0.0);

                float2 impactNormal = impactNormalData.xy;
                impactNormal *= rsqrt(
                    max(dot(impactNormal, impactNormal), 0.000001));

                float elapsed = max(_Time.y - impactData.z, 0.0);
                float duration = max(_WaveDuration, 0.01);

                if (elapsed >= duration)
                    return float2(0.0, 0.0);

                float attackDuration = max(min(duration * 0.1, 0.1), 0.0001);
                float attackProgress = saturate(elapsed / attackDuration);
                float attackFade =
                    attackProgress * attackProgress
                    * (3.0 - 2.0 * attackProgress);

                float fadeStart = duration * 0.75;
                float fadeProgress = saturate(
                    (elapsed - fadeStart)
                    / max(duration - fadeStart, 0.0001));
                float smoothProgress =
                    fadeProgress * fadeProgress
                    * (3.0 - 2.0 * fadeProgress);
                float waveEnvelope = attackFade * (1.0 - smoothProgress);

                float2 delta = vertexPosition - impactData.xy;
                float2 tangent = float2(-impactNormal.y, impactNormal.x);

                float surfaceDistance = abs(dot(delta, tangent));
                float depthDistance = abs(dot(delta, impactNormal));

                float falloff = max(_ImpactFalloff, 0.0001);
                float spatialFade = exp(
                    -surfaceDistance * surfaceDistance
                    * falloff * falloff * 0.35);

                float depthFade = exp(
                    -depthDistance * (falloff + 1.0));

                float timeFade = exp(
                    -elapsed * max(_ImpactDecay, 0.0001));

                float phase =
                    surfaceDistance * _ImpactFrequency
                    - elapsed * _ImpactSpeed;

                float ripple = -cos(phase) * 0.8;

                float dent = -0.3 * exp(
                    -surfaceDistance * surfaceDistance
                    * max(_ImpactFalloff + 1.0, 0.0001)
                    -elapsed * max(_ImpactDecay + 2.0, 0.0001));

                float offset =
                    (ripple * spatialFade * depthFade * timeFade + dent)
                    * strength
                    * _WaveHeightMultiplier
                    * waveEnvelope;

                return impactNormal * offset;
            }

            Varyings JellyVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(
                    input.positionOS,
                    unity_SpriteProps.xy);

                float2 combinedOffset = float2(0.0, 0.0);

                combinedOffset += CalculateWaveOffset(
                    input.positionOS.xy,
                    _ImpactData0,
                    _ImpactNormal0);

                combinedOffset += CalculateWaveOffset(
                    input.positionOS.xy,
                    _ImpactData1,
                    _ImpactNormal1);

                combinedOffset += CalculateWaveOffset(
                    input.positionOS.xy,
                    _ImpactData2,
                    _ImpactNormal2);

                combinedOffset += CalculateWaveOffset(
                    input.positionOS.xy,
                    _ImpactData3,
                    _ImpactNormal3);

                float combinedLength = length(combinedOffset);
                float maximumOffset = max(_MaxCombinedWaveOffset, 0.0001);

                if (combinedLength > maximumOffset)
                    combinedOffset *= maximumOffset / combinedLength;

                input.positionOS.xy += combinedOffset;

                Varyings output = CommonLitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 JellyFragment(Varyings input) : SV_Target
            {
                return CommonLitFragment(input, input.color);
            }
            ENDHLSL
        }
    }
}
