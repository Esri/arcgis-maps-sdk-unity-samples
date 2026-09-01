// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

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
        EditorApplication.update += PollAndFixShaderGraphs;
    }

    private static void PollAndFixShaderGraphs()
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

        EditorApplication.update -= PollAndFixShaderGraphs;
        ProcessProjectShaderGraphs();
    }

    public static void ProcessProjectShaderGraphs()
    {
        if (!Directory.Exists("Assets"))
        {
            return;
        }

        var brokenGraphs = new List<string>();
        var rawFiles = Directory.GetFiles("Assets", "*.shadergraph", SearchOption.AllDirectories);

        foreach (var rawFile in rawFiles)
        {
            var unityPath = rawFile.Replace("\\", "/");

            if (!IsProjectLocalShaderGraph(unityPath))
            {
                continue;
            }

            var shader = AssetDatabase.LoadAssetAtPath<Shader>(unityPath);

            if (shader == null || !shader.isSupported || shader.name.Contains("InternalErrorShader"))
            {
                brokenGraphs.Add(unityPath);
            }
        }

        if (brokenGraphs.Count > 0)
        {
            AssetDatabase.StartAssetEditing();

            foreach (var path in brokenGraphs)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.StopAssetEditing();
            Debug.Log($"[ShaderGraph Fixer] Auto-reimported {brokenGraphs.Count} broken shader graph(s).");
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
