#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build;
using UnityEngine.Rendering;

public class BuildSamplesError
{
        private const string HDRPPipelineAssetPath = "Assets/SampleViewer/Resources/SampleGraphicSettings/SampleHDRPipeline.asset";
        private const string URPPipelineAssetPath = "Assets/SampleViewer/Resources/SampleGraphicSettings/SampleURPipeline.asset";

    [DidReloadScripts]
    public static void Initialize()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayer);
    }
    private static void BuildPlayer(BuildPlayerOptions options)
    {
        BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
#if USE_URP_PACKAGE && USE_HDRP_PACKAGE
        EditorUtility.DisplayDialog("Pipeline Error:", "\nBuilding with both render pipelines installed is not available. Please remove the HDRP package if building for a mobile device, or remove the URP package if building for Windows or MacOS.", "OK");
        
        throw new BuildFailedException("Cannot build with both render pipeline packages installed. Please remove one.");
#elif USE_OPENXR_PACKAGE && UNITY_STANDALONE_OSX
        EditorUtility.DisplayDialog("OpenXR Error:", "\nCannot build for MacOS standalone with OpenXR Plugin installed. Please remove the OpenXR Plugin package with the Package Manager", "OK");
        
        throw new BuildFailedException("Cannot build with OpenXR Plugin package installed. Please remove before building for MacOS standalone.");
#else
        SetBuildRenderPipeline();
        BuildPipeline.BuildPlayer(options);
#endif
    }

    private static void SetBuildRenderPipeline()
    {
#if USE_HDRP_PACKAGE
        var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(HDRPPipelineAssetPath);
#elif USE_URP_PACKAGE
        var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(URPPipelineAssetPath);
#else
        RenderPipelineAsset pipeline = null;
#endif

        if (pipeline == null)
        {
            throw new BuildFailedException("The selected render pipeline asset could not be loaded.");
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        AssetDatabase.SaveAssets();
    }
}
#endif