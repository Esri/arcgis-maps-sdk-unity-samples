using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ArcGISSamplesPostProcessor : AssetPostprocessor
{
    private static readonly HashSet<string> processedPaths = new HashSet<string>();
    private static int framesToWait = 60;

    static ArcGISSamplesPostProcessor()
    {
        // Hook into the update loop after every assembly reload / compile resolution
        EditorApplication.update += PollAndFixShaderGraphs;
    }

    private static void PollAndFixShaderGraphs()
    {
        // Wait until script compilation fully succeeds
        if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
        {
            return;
        }

        // Give Unity's Shader Graph importer 60 frames after compile errors clear to initialize
        framesToWait--;
        if (framesToWait > 0)
        {
            return;
        }

        // Unhook update loop once processing runs successfully
        EditorApplication.update -= PollAndFixShaderGraphs;
        ProcessProjectShaderGraphs();
    }

    public static void ProcessProjectShaderGraphs()
    {
        if (!Directory.Exists("Assets")) return;

        List<string> brokenGraphs = new List<string>();
        string[] rawFiles = Directory.GetFiles("Assets", "*.shadergraph", SearchOption.AllDirectories);

        foreach (string rawFile in rawFiles)
        {
            string unityPath = rawFile.Replace("\\", "/");

            if (processedPaths.Contains(unityPath) || !IsProjectLocalShaderGraph(unityPath))
            {
                continue;
            }

            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(unityPath);

            // Detect missing, uncompiled, or error fallbacks
            if (shader == null || !shader.isSupported || shader.name.Contains("InternalErrorShader"))
            {
                brokenGraphs.Add(unityPath);
            }
        }

        if (brokenGraphs.Count > 0)
        {
            Debug.Log($"[ShaderGraph Fixer] Found {brokenGraphs.Count} unindexed/broken shader graph(s). Forcing reimport...");

            foreach (string path in brokenGraphs)
            {
                processedPaths.Add(path);
            }

            AssetDatabase.StartAssetEditing();
            foreach (string path in brokenGraphs)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.StopAssetEditing();

            Debug.Log("[ShaderGraph Fixer] Reimport complete!");
        }
    }

    private static bool IsProjectLocalShaderGraph(string path)
    {
        if (!path.EndsWith(".shadergraph")) return false;
        if (!path.StartsWith("Assets/")) return false;
        if (path.Contains("Packages/") || path.Contains("PackageCache/")) return false;

        return true;
    }
}