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
    private const string HDRPPipelineTypeName = "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset";
    private const float HDRPLightIntensity = 100000f;
    private const float URPLightIntensity = 0.65f;

    private void Start()
    {
        var directionalLight = GetComponent<Light>();
        var pipelineTypeName = GraphicsSettings.defaultRenderPipeline?.GetType().FullName;
        var isHDRP = pipelineTypeName == HDRPPipelineTypeName;

        directionalLight.intensity = isHDRP ? HDRPLightIntensity : URPLightIntensity;
        directionalLight.enabled = isHDRP;
    }
}