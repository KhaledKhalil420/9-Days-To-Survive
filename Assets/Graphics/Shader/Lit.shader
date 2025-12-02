Shader "Custom/URP/LitLeafSway"
{
    Properties
    {
        [MainTexture]_BaseMap("Texture", 2D) = "white" {}
        [MainColor]_BaseColor("Color", Color) = (1,1,1,1)

        _SwayStrength("Sway Strength", Float) = 0.1
        _SwaySpeed("Sway Speed", Float) = 1
        _Cutoff("Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" 
               "Queue"="AlphaTest" 
               "RenderType"="TransparentCutout" }

        Pass
        {
            Name "ForwardLit"
            Tags{ "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            // Add fog and shadow variants
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _SwayStrength;
                float _SwaySpeed;
                float _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float fogFactor    : TEXCOORD4;
            };

            float3 RotateY(float3 v, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3(
                    v.x * c - v.z * s,
                    v.y,
                    v.x * s + v.z * c
                );
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float sway = sin(_Time.y * _SwaySpeed + IN.positionOS.x * 3.0) * _SwayStrength;

                float3 pos = IN.positionOS.xyz;
                float3 nrm = IN.normalOS;

                pos = RotateY(pos, sway);
                nrm = RotateY(nrm, sway);

                float3 posWS = TransformObjectToWorld(pos);

                OUT.positionWS  = posWS;
                OUT.positionHCS = TransformWorldToHClip(posWS);
                OUT.normalWS    = TransformObjectToWorldNormal(nrm);
                OUT.uv          = IN.uv;
                
                // Proper shadow coordinate calculation
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    OUT.shadowCoord = ComputeScreenPos(OUT.positionHCS);
                #else
                    OUT.shadowCoord = TransformWorldToShadowCoord(posWS);
                #endif
                
                OUT.fogFactor = ComputeFogFactor(OUT.positionHCS.z);
                
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                clip(tex.a - _Cutoff);

                float3 N = normalize(IN.normalWS);

                // Get main light with shadow data
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    float4 shadowCoord = IN.shadowCoord;
                #else
                    float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                #endif
                
                Light mainLight = GetMainLight(shadowCoord);

                float3 L = normalize(mainLight.direction);
                float ndotl = saturate(dot(N, L));

                // Apply shadow attenuation to main light
                float3 directDiffuse = tex.rgb * _BaseColor.rgb * ndotl * mainLight.color * mainLight.shadowAttenuation;

                float3 ambient = SampleSH(N);
                float3 envDiffuse = tex.rgb * _BaseColor.rgb * ambient;

                float3 finalColor = directDiffuse + envDiffuse;
                
                // Add additional lights (optional, includes shadows)
                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                        float3 lightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
                        float3 additionalDiffuse = tex.rgb * _BaseColor.rgb * saturate(dot(N, light.direction)) * lightColor;
                        finalColor += additionalDiffuse;
                    }
                #endif
                
                // Apply fog
                finalColor = MixFog(finalColor, IN.fogFactor);

                return float4(finalColor, tex.a * _BaseColor.a);
            }

            ENDHLSL
        }
        
        // ShadowCaster pass - required to cast shadows
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _SwayStrength;
                float _SwaySpeed;
                float _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            float3 RotateY(float3 v, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float3(
                    v.x * c - v.z * s,
                    v.y,
                    v.x * s + v.z * c
                );
            }

            float3 _LightDirection;

            float4 GetShadowPositionHClip(Attributes input)
            {
                float sway = sin(_Time.y * _SwaySpeed + input.positionOS.x * 3.0) * _SwayStrength;
                float3 pos = RotateY(input.positionOS.xyz, sway);
                float3 positionWS = TransformObjectToWorld(pos);
                float3 normalWS = TransformObjectToWorldNormal(RotateY(input.normalOS, sway));

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.uv = input.uv;
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                float alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                clip(alpha - _Cutoff);
                return 0;
            }

            ENDHLSL
        }
    }
}