using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ArcGISSamplesPostProcessor : AssetPostprocessor
{
    private static int framesToWait = 60;
    private static readonly HashSet<string> processedPaths = new HashSet<string>();

    static ArcGISSamplesPostProcessor()
    {
        if (!SessionState.GetBool("ShaderFixerRunOnBoot", false))
        {
            SessionState.SetBool("ShaderFixerRunOnBoot", true);
            EditorApplication.update += WaitingForDatabaseIndex;
        }
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

    public static void ForceManualReimport()
    {
        processedPaths.Clear();
        ProcessProjectShaderGraphs();
    }

    public static void ProcessProjectShaderGraphs()
    {
        if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
        {
            return;
        }

        List<string> brokenGraphs = new List<string>();

        if (Directory.Exists("Assets"))
        {
            string[] rawFiles = Directory.GetFiles("Assets", "*.shadergraph", SearchOption.AllDirectories);
            
            foreach (string rawFile in rawFiles)
            {
                string unityPath = rawFile.Replace("\\", "/");

                if (processedPaths.Contains(unityPath))
                {
                    continue;
                }

                if (IsProjectLocalShaderGraph(unityPath))
                {
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(unityPath);

                    if (shader == null || !shader.isSupported || shader.name.Contains("InternalErrorShader"))
                    {
                        brokenGraphs.Add(unityPath);
                    }
                }
            }
        }

        if (brokenGraphs.Count > 0)
        {
            Debug.Log($"[ShaderGraph Fixer] Found {brokenGraphs.Count} local project shader graph(s) needing reimport. Processing...");

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

            Debug.Log("[ShaderGraph Fixer] Local Shader Graph reimport complete!");
        }
    }

    private static bool IsProjectLocalShaderGraph(string path)
    {
        if (!path.EndsWith(".shadergraph")) 
        {
            return false;
        }

        if (!path.StartsWith("Assets/")) 
        {
            return false;
        }

        if (path.Contains("Packages/") || path.Contains("PackageCache/")) 
        {
            return false;
        }

        return true;
    }
}