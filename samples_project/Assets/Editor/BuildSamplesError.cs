#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Build;

public class BuildSamplesError
{
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
#else
        BuildPipeline.BuildPlayer(options);
#endif
    }
}
#endif