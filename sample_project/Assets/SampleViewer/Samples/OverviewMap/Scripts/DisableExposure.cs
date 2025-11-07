// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.Rendering;
#if USE_HDRP_PACKAGE
using UnityEngine.Rendering.HighDefinition;
#endif

public class DisableExposure : MonoBehaviour
{
    private Volume skyAndFog;
#if USE_HDRP_PACKAGE
    private Exposure exposure;
#endif

    private void Start()
    {
        Invoke(nameof(TurnOffExposure), 1.0f);
    }

    private void TurnOffExposure()
    {
        skyAndFog = GetComponentInChildren<Volume>();

        if (!skyAndFog)
        {
            return;
        }
#if USE_HDRP_PACKAGE
        skyAndFog.profile.TryGet(out exposure);

        if (exposure)
        {
            exposure.active = false;
        }
#endif
    }
}