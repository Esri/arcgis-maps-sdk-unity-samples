// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using TMPro;
using UnityEngine;

public class TimeBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (timeText != null)
        {
            timeText.text = DateTime.Now.ToString("M/d/yyyy h:mm");
        }
    }
}