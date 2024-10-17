// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.UI;

public class DisableSampleButtonsForURP : MonoBehaviour
{
    private void Awake()
    {
#if !USE_HDRP_PACKAGE
        var sampleButton = GetComponent<Button>();
        sampleButton.interactable = false;
#endif
    }
}
