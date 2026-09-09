Shader "Hidden/VHSStatic"
{
    Properties
    {
        _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.12
        _ScanlineIntensity("Scanline Intensity", Range(0, 1)) = 0.2
        _TrackingIntensity("Tracking Distortion", Range(0, 1)) = 0.35
        _VignetteIntensity("Vignette", Range(0, 1)) = 0.35
        _ChromaticAberration("Chromatic Aberration", Range(0, 0.02)) = 0.003
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "VHSStatic"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _NoiseIntensity;
                float _ScanlineIntensity;
                float _TrackingIntensity;
                float _VignetteIntensity;
                float _ChromaticAberration;
            CBUFFER_END

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float time = _Time.y;

                // Distorsión de "tracking": de vez en cuando una franja horizontal
                // se desplaza lateralmente, como el rastreo desalineado de un VHS
                float bandNoise = Hash(float2(floor(uv.y * 60.0), floor(time * 6.0)));
                float glitchActive = step(0.94, Hash(float2(floor(time * 8.0), 1.0)));
                uv.x += (bandNoise - 0.5) * _TrackingIntensity * 0.06 * glitchActive;

                // Aberración cromática leve, típica de la óptica barata de una videocámara VHS
                float2 caOffset = float2(_ChromaticAberration, 0.0);
                float r = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + caOffset, 0).r;
                float g = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).g;
                float b = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - caOffset, 0).b;
                float3 color = float3(r, g, b);

                // Ruido estático animado (grano de cinta)
                float2 noiseUV = uv * float2(1280.0, 720.0) + time * 120.0;
                float staticNoise = Hash(noiseUV);
                color += (staticNoise - 0.5) * _NoiseIntensity;

                // Líneas de escaneo horizontales
                float scanline = sin(uv.y * 800.0) * 0.5 + 0.5;
                color *= 1.0 - scanline * _ScanlineIntensity;

                // Viñeta hacia los bordes, como el visor de una cámara analógica
                float2 centered = uv - 0.5;
                float vignette = 1.0 - dot(centered, centered) * _VignetteIntensity * 2.0;
                color *= saturate(vignette);

                return float4(saturate(color), 1.0);
            }
            ENDHLSL
        }
    }
}
