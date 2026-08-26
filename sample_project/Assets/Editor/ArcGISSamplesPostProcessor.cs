using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ArcGISSamplesPostProcessor : AssetPostprocessor
{
    private static int framesToWait = 60;

    static ArcGISSamplesPostProcessor()
    {
        EditorApplication.update += WaitingForDatabaseIndex;
    }

    private static void WaitingForDatabaseIndex()
    {
        if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
        {
            return;
        }

        framesToWait--;

        if (framesToWait > 0)
        {
            return;
        }

        EditorApplication.update -= WaitingForDatabaseIndex;
        ProcessProjectShaderGraphs();
    }

    private static void OnPostprocessAllAssets(
        string[] importedAssets, 
        string[] deletedAssets, 
        string[] movedAssets, 
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path.StartsWith("Assets/") && path.EndsWith(".shadergraph"))
            {
                EditorApplication.delayCall += ProcessProjectShaderGraphs;
                break;
            }
        }
    }

    public static void ProcessProjectShaderGraphs()
    {
        if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
        {
            return;
        }

        List<string> graphPathsOnDisk = new List<string>();
        if (Directory.Exists("Assets"))
        {
            string[] rawFiles = Directory.GetFiles("Assets", "*.shadergraph", SearchOption.AllDirectories);
            
            foreach (string rawFile in rawFiles)
            {
                string unityPath = rawFile.Replace("\\", "/");
                graphPathsOnDisk.Add(unityPath);
            }
        }

        List<string> brokenGraphs = new List<string>();

        foreach (string path in graphPathsOnDisk)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);

            if (shader == null || !shader.isSupported || shader.name.Contains("InternalErrorShader"))
            {
                brokenGraphs.Add(path);
            }
        }

        if (brokenGraphs.Count > 0)
        {
            AssetDatabase.StartAssetEditing();

            foreach (string path in brokenGraphs)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            
            AssetDatabase.StopAssetEditing();
        }
    }
}