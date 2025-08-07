// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Layers;
using Esri.Unity;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Identify : MonoBehaviour
{
    [SerializeField] private ArcGISMapComponent arcGISMapComponent;
    private ArcGIS3DObjectSceneLayer buildingLayer;
    [SerializeField] private Transform contentContainer;
    private List<GameObject> ListItems = new List<GameObject>();
    [SerializeField] private GameObject markerGO;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject scrollViewItem;
    [SerializeField] private Color selectColor;
    [SerializeField] private Material selectMaterial;
    private float selectedID;

    private void EmptyPropertiesDropdown()
    {
        if (ListItems != null)
        {
            foreach (var item in ListItems)
            {
                Destroy(item.gameObject);
            }

            ListItems.Clear();
        }
    }

    private void Start()
    {
        if (arcGISMapComponent)
        {
            buildingLayer = new ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Building Layer", 1.0f, true, "");
            arcGISMapComponent.Map.Layers.Add(buildingLayer);

            if (buildingLayer)
            {
                Setup3DAttributesFloatAndIntegerType(buildingLayer);
            }
        }

        Shader.SetGlobalFloat("_SelectedObjectID", 0);
        Shader.SetGlobalColor("_HighlightColor", selectColor);
    }

    public void StartRaycast()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#else
        Ray ray = Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>());
#endif

        if (Physics.Raycast(ray, out var hit))
        {
            var arcGISRaycastHit = arcGISMapComponent.GetArcGISRaycastHit(hit);
            var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
            var cameraGeoPosition = arcGISMapComponent.EngineToGeographic(Camera.main.transform.position);
            var result = arcGISMapComponent.View.IdentifyLayersAsync(geoPosition, cameraGeoPosition, -1);
            result.Wait();
            selectedID = 0;
            EmptyPropertiesDropdown();

            if (!result.IsCanceled())
            {
                var resultValue = result.Get();

                for (ulong i = 0; i < resultValue.Size; i++)
                {
                    var elements = resultValue.At(i).GeoElements;

                    for (ulong j = 0; j < elements.GetSize(); j++)
                    {
                        var attributes = elements.At(j).Attributes;
                        var keys = attributes.Keys;

                        try
                        {
                            var id = attributes["OBJECTID"];
                            selectedID = Convert.ToInt32(id.ToString());
                            Shader.SetGlobalFloat("_SelectedObjectID", selectedID);
                        }
                        catch (Exception ex) 
                        {
                            Debug.LogError(ex.ToString());
                        }

                        for (ulong k = 0; k < keys.Size; k++)
                        {
                            try
                            {
                                var value = attributes[keys.At(k)];
                                var item = Instantiate(scrollViewItem);
                                item.GetComponentInChildren<TextMeshProUGUI>().text = $"- <b>{keys.At(k)}</b>:" + value.ToString(); ;
                                item.transform.SetParent(contentContainer);
                                item.transform.localScale = Vector2.one;
                                ListItems.Add(item);
                            }
                            catch
                            {
                                Debug.Log(keys.At(k) + ": <no conversion available>");
                            }
                        }
                    }
                }
            }
        }
    }

    private void Setup3DAttributesFloatAndIntegerType(ArcGIS3DObjectSceneLayer layer)
    {
        var layerAttributes = ArcGISImmutableArray<String>.CreateBuilder();
        layerAttributes.Add("OBJECTID");
        layer.SetAttributesToVisualize(layerAttributes.MoveToArray());
        layer.MaterialReference = selectMaterial;
    }
}