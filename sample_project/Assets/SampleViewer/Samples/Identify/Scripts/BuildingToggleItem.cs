// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.GameEngine.View;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BuildingToggleItem : MonoBehaviour
{
    private Identify identify;
    private Button toggle;

    [HideInInspector] public ulong BuildingNumber;
    public Image toggleImage;
    public Sprite isOn;
    public Sprite isOff;
    public ArcGISIdentifyLayerResultImmutableCollection ResultValue;

    private void Awake()
    {
        toggle = GetComponentInChildren<Button>();   
        identify = FindFirstObjectByType<Identify>();
    }

    void Start()
    {
        toggle.onClick.AddListener(delegate
        {
            foreach (var item in FindObjectsOfType<BuildingToggleItem>())
            {
                item.toggleImage.sprite = isOff;
            }

            UpdateToggles();
        });
    }

    private void UpdateToggles()
    {
        identify.SelectedResult = BuildingNumber;
        identify.EmptyIdentifyResults();
        identify.ParseResults(BuildingNumber, ResultValue);
        toggleImage.sprite = isOn;
    }
}
