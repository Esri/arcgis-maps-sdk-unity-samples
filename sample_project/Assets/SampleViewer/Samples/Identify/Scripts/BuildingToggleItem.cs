// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.GameEngine.MapView;
using Esri.GameEngine.View;
using Esri.Unity;
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
    public ArcGISImmutableCollection<ArcGISIdentifyLayerResult> IdentifyLayerResults;

    private void Awake()
    {
        toggle = GetComponentInChildren<Button>();   
        identify = FindFirstObjectByType<Identify>();
    }

    void Start()
    {
        toggle.onClick.AddListener(delegate
        {
            foreach (var item in FindObjectsByType<BuildingToggleItem>(FindObjectsSortMode.None))
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
        identify.ParseResults(BuildingNumber, IdentifyLayerResults);
        toggleImage.sprite = isOn;
    }
}
