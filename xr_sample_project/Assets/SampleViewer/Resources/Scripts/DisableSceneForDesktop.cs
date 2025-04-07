// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.UI;

public class DisableSceneForDesktop : MonoBehaviour
{
    void Start()
    {
#if PLATFORM_STANDALONE_WIN || UNITY_EDITOR
        GetComponent<Button>().interactable = false;
#else
        GetComponent<Button>().interactable = true;
#endif
    }
}
