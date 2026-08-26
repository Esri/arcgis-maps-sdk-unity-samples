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
        List<string> brokenGraphs = new List<string>();

        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".shadergraph"))
            {
                // Check if Unity failed to generate a valid Shader object from the graph
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader == null || !shader.isSupported)
                {
                    brokenGraphs.Add(path);
                }
            }
        }

        if (brokenGraphs.Count > 0)
        {
            // Delay the re-import until the initial import loop finishes completely
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.StartAssetEditing();
                foreach (string path in brokenGraphs)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
                AssetDatabase.StopAssetEditing();
                Debug.Log($"[ShaderGraph Fixer] Auto-reimported {brokenGraphs.Count} shader graph(s) to fix initial load exception.");
            };
        }
    }
}
