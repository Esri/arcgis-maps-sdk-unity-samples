//
//Written by Sam the intern
//Reimports materials after render pipeline package is added/removed
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class CallReImport
{
    static CallReImport()
    {
        void Handle(PackageRegistrationEventArgs args)
        {
#if USE_HDRP_PACKAGE
            RenderPipelineAsset HDRPasset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/SampleViewer/Resources/SampleGraphicSettings/SampleHDRPipeline.asset");
            GraphicsSettings.defaultRenderPipeline = HDRPasset;
#elif USE_URP_PACKAGE
            RenderPipelineAsset URPasset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/SampleViewer/Resources/SampleGraphicSettings/SampleURPipeline.asset");
            GraphicsSettings.defaultRenderPipeline = URPasset;
#endif
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path.Contains("VR"))
                {
#if USE_OPENXR_PACKAGE
                    scene.enabled = true;
#else
                    scene.enabled = false;
#endif
                }
            }

            reImport();
        }

        Events.registeredPackages += Handle;
    }
    static void reImport()
    {
        List<string> importPaths = new List<string>();
        importPaths.Add("Assets/SampleViewer/Resources/Shaders");
        importPaths.Add("Assets/SampleViewer/Samples/FeatureLayer/Shaders");
        importPaths.Add("Assets/SampleViewer/Samples");
        importPaths.Add("Assets/Samples");

        var pipelineTypeName = GraphicsSettings.defaultRenderPipeline?.GetType().FullName;
        var pipelineMaterialsPath = pipelineTypeName == "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset"
            ? "Packages/com.esri.arcgis-maps-sdk/SDK/Resources/Shaders/Materials/HDRP"
            : "Packages/com.esri.arcgis-maps-sdk/SDK/Resources/Shaders/Materials/URP";
        importPaths.Add(pipelineMaterialsPath);

        foreach (string path in importPaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        }
    }

    [MenuItem("Tools/ArcGIS Maps SDK/Re-Import Samples Materials")]
    public static void ManualCall()
    {
        reImport();
    }
}
#endif