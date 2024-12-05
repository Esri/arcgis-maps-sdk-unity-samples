// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DisableExposure : MonoBehaviour
{
    private Volume skyAndFog;
    private Exposure exposure;
    
    void Start()
    {
        skyAndFog = GetComponent<Volume>();
        skyAndFog.profile.TryGet(out exposure);
        exposure.active = false;
    }
}
