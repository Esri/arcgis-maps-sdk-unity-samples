using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.ObjectModel;

[InitializeOnLoad]
public static class PipelineCameraDataCleanup
{
    private const string HDRPPackageName = "com.unity.render-pipelines.high-definition";
    private const string URPPackageName = "com.unity.render-pipelines.universal";
    private const string HDRPCameraDataTypeName = "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData";
    private const string URPCameraDataTypeName = "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData";

    static PipelineCameraDataCleanup()
    {
        Events.registeredPackages += SynchronizeCameraData;
    }

    private static void SynchronizeCameraData(PackageRegistrationEventArgs args)
    {
        if (ContainsPackage(args.added, URPPackageName))
        {
            AddCameraData(URPCameraDataTypeName, "URP");
        }

        if (ContainsPackage(args.added, HDRPPackageName))
        {
            AddCameraData(HDRPCameraDataTypeName, "HDRP");
        }

        if (ContainsPackage(args.removed, URPPackageName) || ContainsPackage(args.removed, HDRPPackageName))
        {
            RemoveMissingCameraData();
        }
    }

    private static bool ContainsPackage(ReadOnlyCollection<UnityEditor.PackageManager.PackageInfo> packages, string packageName)
    {
        foreach (var package in packages)
        {
            if (package.name == packageName)
            {
                return true;
            }
        }

        return false;
    }

    private static void AddCameraData(string componentTypeName, string pipelineName)
    {
        var componentType = FindType(componentTypeName);
        if (componentType == null)
        {
            Debug.LogWarning($"Could not find {pipelineName} additional camera data after the package was added.");
            return;
        }

        UpdateCameraAssets(camera =>
        {
            if (camera.GetComponent(componentType) != null)
            {
                return 0;
            }

            camera.gameObject.AddComponent(componentType);
            return 1;
        });
    }

    private static void RemoveMissingCameraData()
    {
        UpdateCameraAssets(camera => GameObjectUtility.RemoveMonoBehavioursWithMissingScript(camera.gameObject));
    }

    private static void UpdateCameraAssets(Func<Camera, int> updateCamera)
    {
        var updatedCount = 0;
        var sceneSetup = EditorSceneManager.GetSceneManagerSetup();

        try
        {
            foreach (var sceneGuid in AssetDatabase.FindAssets("t:Scene"))
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                updatedCount += UpdateCameras(scene.GetRootGameObjects(), updateCamera);

                if (scene.isDirty)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(sceneSetup);
        }

        foreach (var prefabGuid in AssetDatabase.FindAssets("t:Prefab"))
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                var updatedInPrefab = UpdateCameras(new[] { prefabRoot }, updateCamera);
                updatedCount += updatedInPrefab;

                if (updatedInPrefab > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Updated {updatedCount} pipeline camera data component(s).");
    }

    private static int UpdateCameras(GameObject[] roots, Func<Camera, int> updateCamera)
    {
        var updatedCount = 0;

        foreach (var root in roots)
        {
            foreach (var camera in root.GetComponentsInChildren<Camera>(true))
            {
                updatedCount += updateCamera(camera);
            }
        }

        return updatedCount;
    }

    private static Type FindType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }
}