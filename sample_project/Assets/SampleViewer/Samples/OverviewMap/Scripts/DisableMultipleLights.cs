// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class DisableMultipleLights : MonoBehaviour
{
    private void Start()
    {
        if (GraphicsSettings.defaultRenderPipeline == null)
        {
            return;
        }

        var directionalLight = GetComponent<Light>();
        directionalLight.enabled = GraphicsSettings.defaultRenderPipeline.GetType() == typeof(UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset);
    }
}
