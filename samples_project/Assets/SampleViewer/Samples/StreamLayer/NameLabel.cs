// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameLabel : MonoBehaviour
{
    public string nameLabel;
    public TMP_Text text;
    public Slider slider;

    private void Start()
    {
        text.text = nameLabel;
        slider.value = 5;
    }

    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
