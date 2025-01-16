// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
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
        importPaths.Add("Packages/com.esri.arcgis-maps-sdk/SDK/Resources/Shaders/Materials/HDRP");
        importPaths.Add("Assets/Samples");

        foreach (string path in importPaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);
        }
    }
}
#endif