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
    [System.NonSerialized] public ArcGISImmutableCollection<ArcGISIdentifyLayerResult> IdentifyLayerResults;
    public Image toggleImage;
    public Sprite isOn;
    public Sprite isOff;
    
    private void Awake()
    {
        toggle = GetComponentInChildren<Button>();   
        identify = FindAnyObjectByType<Identify>();
    }

    void Start()
    {
        toggle.onClick.AddListener(delegate
        {
#if UNITY_6000_5_OR_NEWER
            foreach (var item in FindObjectsByType<BuildingToggleItem>())
#else
            foreach (var item in FindObjectsByType<BuildingToggleItem>(FindObjectsSortMode.None))
#endif
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
