// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

public class SetLightRotation : MonoBehaviour
{
    private GameObject directionalLight;

    public void SetDirectionalLightRotation(float value)
    {
        // Confirm reference to directional light before calling methods within it
        directionalLight = directionalLight ? directionalLight : GameObject.Find("Directional Light");
        if (directionalLight)
        {
            directionalLight.transform.rotation = Quaternion.Euler(value, directionalLight.transform.rotation.eulerAngles.y, directionalLight.transform.rotation.eulerAngles.z);
        }
    }

    private void Start()
    {
        directionalLight = GameObject.Find("Directional Light");
    }
}