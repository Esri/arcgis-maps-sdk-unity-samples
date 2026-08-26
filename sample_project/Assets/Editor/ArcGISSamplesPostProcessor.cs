using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ArcGISSamplesPostProcessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
    {
        if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
        {
            return;
        }

        List<string> brokenGraphs = new List<string>();

        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".shadergraph"))
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);

                if (shader == null || !shader.isSupported)
                {
                    brokenGraphs.Add(path);
                }
            }
        }

        if (brokenGraphs.Count > 0)
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorUtility.scriptCompilationFailed || EditorApplication.isCompiling)
                {
                    return;
                }

                AssetDatabase.StartAssetEditing();

                foreach (string path in brokenGraphs)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }

                AssetDatabase.StopAssetEditing();
                Debug.Log($"[ShaderGraph Fixer] Auto-reimported {brokenGraphs.Count} shader graph(s).");
            };
        }
    }
}