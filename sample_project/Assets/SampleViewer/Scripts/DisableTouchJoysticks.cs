// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

public class DisableTouchJoysticks : MonoBehaviour
{
    public static DisableTouchJoysticks instance;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void ToggleCanvas(bool active)
    {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        gameObject.SetActive(active);
#endif
    }
}
