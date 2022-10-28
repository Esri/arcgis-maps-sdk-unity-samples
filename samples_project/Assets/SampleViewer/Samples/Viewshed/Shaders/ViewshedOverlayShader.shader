
Shader "URPViewshedOverlay"
{
    // Creates a viewshed effect based on an observer camera's depth buffer and optical properties
    // Apply this shader to a material on a plane/quad in front of the target camera
    // Depth and Opaque textures must be enabled on the target camera or render pipeline
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

            float _ViewshedDepthThreshold;
            float _ViewshedOpacity;
            float3 _ViewshedObserverPosition;
            float4x4 _ViewshedObserverViewProjMatrix;
            uniform sampler2D _CameraOpaqueTexture;
            uniform sampler2D _ViewshedObserverDepthTexture;

            static const float4 RED_COLOR = float4(1, 0, 0, 1);
            static const float4 GREEN_COLOR = float4(0, 1, 0, 1);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 UV = IN.positionHCS.xy / _ScaledScreenParams.xy;

                #if UNITY_REVERSED_Z
                    float depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                // Reconstruct the world space position
                float3 worldPos = ComputeWorldSpacePosition(UV, depth, UNITY_MATRIX_I_VP);

                // Reproject to the observer's view space
                float4 viewCoord = mul(_ViewshedObserverViewProjMatrix, float4(worldPos.xyz,1));
                float2 viewUV = (viewCoord.xy / viewCoord.w) * 0.5 + 0.5;
                viewUV.y = 1-viewUV.y;

                // Get the color of the current fragment
                float4 colorBase = tex2D(_CameraOpaqueTexture, UV);

                // Do not draw viewshed effect if fragment is outside of observer's view frustum (or beyond a set depth threshold)
                if(viewUV.x < 0 || viewUV.x > 1 || viewUV.y < 0 || viewUV.y > 1 || viewCoord.z < 0 || depth < _ViewshedDepthThreshold)
                    return colorBase;

                float observerDepth = tex2D(_ViewshedObserverDepthTexture, viewUV).r;

                //colorize fragments withing viewshed area
                const float eps = 0.001;
                if(viewCoord.z > observerDepth && abs(viewCoord.z - observerDepth) > eps)
                {
                    //red
                    colorBase = lerp(colorBase, RED_COLOR, _ViewshedOpacity);
                }
                else
                {
                    //green
                    colorBase = lerp(colorBase, GREEN_COLOR, _ViewshedOpacity);
                }

                return colorBase;
            }
            ENDHLSL
        }
    }
}