//
//Written by Sam the intern
//Reimports materials after render pipeline package is added/removed
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public static class CallReImport
{
    static CallReImport()
    {
        void Handle(PackageRegistrationEventArgs args)
        {
            reImport();
        }
        Events.registeredPackages += Handle;
    }
    static void reImport()
    {
        List<string> importPaths = new List<string>();
        importPaths.Add("Assets/SampleViewer/Samples/FeatureLayer/StadiumMaterial.mat");
        importPaths.Add("Assets/SampleViewer/Samples/FeatureLayer/StadiumShader.shadergraph");
        importPaths.Add("Assets/SampleViewer/Samples/LineOfSight/Materials");
        importPaths.Add("Assets/SampleViewer/Samples/Routing/Breadcrumb.mat");
        importPaths.Add("Assets/SampleViewer/Samples/Routing/MarkerBody.mat");
        importPaths.Add("Assets/SampleViewer/Samples/Routing/MarkerHead.mat");
        importPaths.Add("Packages/com.esri.arcgis-maps-sdk/SDK/Resources/Shaders/Materials/URP");
        importPaths.Add("Assets/Samples/ArcGIS Maps SDK for Unity/1.0.0/All Samples/Resources/Materials/URP");
        importPaths.Add("Packages/com.esri.arcgis-maps-sdk/SDK/Resources/Shaders/Materials/HDRP");
        importPaths.Add("Assets/Samples/ArcGIS Maps SDK for Unity/1.0.0/All Samples/Resources/Materials/HDRP");

        foreach (string path in importPaths)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);
            Debug.Log("Re-Imported material at: " + path);
        }
    }
    [MenuItem("ArcGIS Maps SDK/Re-Import Samples Materials")]
    public static void ManualCall()
    {
       reImport();
    }
}
#endif