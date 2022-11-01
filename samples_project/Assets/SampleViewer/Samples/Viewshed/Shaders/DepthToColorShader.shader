
Shader "DepthToColor"
{
    // Creates a single channel float texture based on the scene "eye depth" (not normalized) value per fragment
    // Apply this shader to a material on a plane/quad in front of the target camera
    // Depth texture must be enabled on the target camera or render pipeline
    Properties
    { }

    SubShader
    {
        //Place in transparent queue to ensure opaques are rendered to depth buffer
        Tags { "Queue" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
            };

            float3 _ViewshedObserverPosition;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float frag(Varyings IN) : SV_Target
            {
                float2 UV = IN.positionHCS.xy / _ScaledScreenParams.xy;

                // Sample the depth from the Camera depth texture
                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                return LinearEyeDepth(depth, _ZBufferParams);
            }
            ENDHLSL
        }
    }
    SubShader
    {
        //Place in transparent queue to ensure opaques are rendered to depth buffer
        Tags { "Queue" = "Transparent" "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
            };

            float3 _ViewshedObserverPosition;
            float4 _ViewshedObserverScreenParams;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float frag(Varyings IN) : SV_Target
            {
                float2 UV = IN.positionHCS.xy / _ViewshedObserverScreenParams.xy;

                // Sample the depth from the Camera depth texture
                #if UNITY_REVERSED_Z
                    float depth = SampleCameraDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleCameraDepth(UV));
                #endif

                return LinearEyeDepth(depth, _ZBufferParams);
            }
            ENDHLSL
        }
    }
}