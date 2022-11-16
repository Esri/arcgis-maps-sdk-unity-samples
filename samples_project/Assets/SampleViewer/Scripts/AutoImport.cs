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
            GraphicsSettings.renderPipelineAsset = HDRPasset;
#elif USE_URP_PACKAGE
            RenderPipelineAsset URPasset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/SampleViewer/Resources/SampleGraphicSettings/SampleURPipeline.asset");
            GraphicsSettings.renderPipelineAsset = URPasset;
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
        importPaths.Add("Assets/SampleViewer/Samples");
        importPaths.Add("Packages/com.esri.arcgis-maps-sdk/SDK/Resources/Shaders/Materials/URP");
        importPaths.Add("Assets/Samples/ArcGIS Maps SDK for Unity/1.0.0/All Samples/Resources/Materials/URP");
        importPaths.Add("Packages/com.esri.arcgis-maps-sdk/SDK/Resources/Shaders/Materials/HDRP");
        importPaths.Add("Assets/Samples/ArcGIS Maps SDK for Unity/1.0.0/All Samples/Resources/Materials/HDRP");

        foreach (string path in importPaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);
        }
    }

    [MenuItem("ArcGIS Maps SDK/Re-Import Samples Materials")]
    public static void ManualCall()
    {
       reImport();
    }
}
#endif