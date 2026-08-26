using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ArcGISSamplesPostProcessor : AssetPostprocessor
{
    static ArcGISSamplesPostProcessor()
    {
        EditorApplication.delayCall += ProcessProjectShaderGraphs;
    }

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // Check if any imported asset was a shader graph
        foreach (string path in importedAssets)
        {
            if (path.StartsWith("Assets/") && path.EndsWith(".shadergraph"))
            {
                EditorApplication.delayCall += ProcessProjectShaderGraphs;
                break;
            }
        }
    }

    private static void ProcessProjectShaderGraphs()
    {
        if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
        {
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Shader", new[] { "Assets" });
        List<string> brokenGraphs = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path.EndsWith(".shadergraph"))
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);

                if (shader == null || shader.name.Contains("InternalErrorShader"))
                {
                    brokenGraphs.Add(path);
                }
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
            Debug.Log($"[ShaderGraph Fixer] Auto-reimported {brokenGraphs.Count} shader graph(s).");
        }
    }
}