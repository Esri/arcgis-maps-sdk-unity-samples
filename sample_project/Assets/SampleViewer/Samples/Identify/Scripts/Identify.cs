// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Layers;
using Esri.GameEngine.MapView;
using Esri.GameEngine.View;
using Esri.Unity;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Identify : MonoBehaviour
{
    [SerializeField] private ArcGISMapComponent arcGISMapComponent;
    private ArcGIS3DObjectSceneLayer buildingLayer;
    [SerializeField] private GameObject buildingToggle;
    [SerializeField] private Transform buildingListContentContainer;
    [SerializeField] private Transform contentContainer;
    private List<GameObject> buildingListItems = new List<GameObject>();
    private List<GameObject> ListItems = new List<GameObject>();
    [SerializeField] private GameObject markerGO;
    [SerializeField] private TextMeshProUGUI resultAmount;
    [SerializeField] private GameObject scrollViewItem;
    [SerializeField] private Color selectColor;
    [SerializeField] private Material selectMaterial;
    [SerializeField] private TextMeshProUGUI totalNumberOfBuildingsText;

    [SerializeField] private Button changeViewButton;
    [SerializeField] private Button showResultsButton;
    [SerializeField] private GameObject results;
    [SerializeField] private GameObject buildingsView;

    [SerializeField] private Button increaseResult;
    [SerializeField] private Button decreaseResult;

    private ArcGISImmutableCollection<ArcGISIdentifyLayerResult> resultValue;
    public ulong resultsLength;
    private float selectedID;
    [HideInInspector] public ulong SelectedResult = 0;

    private void DisableButtons(bool enabled)
    {
        increaseResult.interactable = enabled;
        decreaseResult.interactable = enabled;
    }

    public void ResetButton()
    {
        Shader.SetGlobalFloat("_SelectedObjectID", 0);
        resultValue = null;
        resultAmount.text = "";
        DisableButtons(false);
    }

    public void EmptyIdentifyResults()
    {
        if (ListItems != null)
        {
            foreach (var item in ListItems)
            {
                Destroy(item.gameObject);
            }

            ListItems.Clear();
            results.SetActive(false);
        }
    }

    public void EmptyBuildingListResults()
    {
        if (buildingListItems != null)
        {
            foreach (var item in buildingListItems)
            {
                Destroy(item.gameObject);
            }

            buildingListItems.Clear();
            buildingsView.SetActive(false);
        }
    }

    private void PopulateBuildingList()
    {
        for (int i = 0; i < (int)resultsLength; i++)
        {
            var item = Instantiate(buildingToggle);
            var tmpObject = item.GetComponentInChildren<TextMeshProUGUI>();
            item.GetComponent<BuildingToggleItem>().BuildingNumber = (ulong)i;
            item.GetComponent<BuildingToggleItem>().ResultValue = resultValue;
            tmpObject.text = $"Building {i + 1}";

            if (i == 0)
            {
                item.GetComponent<BuildingToggleItem>().toggleImage.sprite = item.GetComponent<BuildingToggleItem>().isOn;
            }
            else
            {
                item.GetComponentInChildren<Image>().sprite = item.GetComponent<BuildingToggleItem>().isOff;
            }

            item.transform.SetParent(buildingListContentContainer);
            item.transform.localScale = Vector2.one;
            buildingListItems.Add(item);
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

        resultAmount.text = "";
        totalNumberOfBuildingsText.text = "";
        buildingsView.SetActive(false);
        results.SetActive(false);
        DisableButtons(false);

        changeViewButton.onClick.AddListener(delegate
        {
            if (buildingsView.activeInHierarchy)
            {
                buildingsView.SetActive(false);
                results.SetActive(true);
            }
            else
            {
                buildingsView.SetActive(true);
                results.SetActive(false);
            }
        });

        showResultsButton.onClick.AddListener(delegate
        {
            buildingsView.SetActive(false);
            results.SetActive(true);
        });

        increaseResult.onClick.AddListener(delegate
        {
            if (SelectedResult < resultsLength - 1)
            {
                EmptyIdentifyResults();
                ++SelectedResult;
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, resultValue);

                if (SelectedResult == resultsLength - 1)
                {
                    increaseResult.interactable = false;
                    decreaseResult.interactable = true;
                }
                else
                {
                    DisableButtons(true);
                }
            }
        });

        decreaseResult.onClick.AddListener(delegate
        {
            if (SelectedResult > 0)
            {
                EmptyIdentifyResults();
                EmptyBuildingListResults();
                --SelectedResult;
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, resultValue);

                if (SelectedResult == 0)
                {
                    increaseResult.interactable = true;
                    decreaseResult.interactable = false;
                }
                else
                {
                    DisableButtons(true);
                }
            }
        });

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
            EmptyIdentifyResults();
            EmptyBuildingListResults();
            SelectedResult = 0;
            var arcGISRaycastHit = arcGISMapComponent.GetArcGISRaycastHit(hit);
            var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
            var cameraGeoPosition = arcGISMapComponent.EngineToGeographic(Camera.main.transform.position);
            var result = arcGISMapComponent.View.IdentifyLayersAsync(geoPosition, cameraGeoPosition, -1);
            result.Wait();

            if (!result.IsCanceled() && result.GetError() == null)
            {
                resultValue = result.Get();
                ParseResults(SelectedResult, resultValue);
                PopulateBuildingList();
            }
        }
    }

    public void ParseResults(ulong NumberOfResults, ArcGISImmutableCollection<ArcGISIdentifyLayerResult> ResultValue)
    {
        if (ResultValue.IsEmpty())
        {
            Debug.LogWarning("No Results Found");
            return;
        }

        var elements = ResultValue.At(0).GeoElements;
        resultsLength = elements.GetSize();

        if (resultsLength == 0)
        {
            Debug.LogWarning("No Results Found");
            return;
        }
        else if (resultsLength == 1)
        {
            DisableButtons(false);
        }
        else if (resultsLength > 1)
        {

            increaseResult.interactable = true;
            decreaseResult.interactable = false;
        }

        totalNumberOfBuildingsText.text = $"total: {resultsLength}";
        resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
        var attributes = elements.At(NumberOfResults).Attributes;
        var keys = attributes.Keys;
        buildingsView.SetActive(false);
        results.SetActive(true);

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
                var tmpObjects = item.GetComponentsInChildren<TextMeshProUGUI>();
                tmpObjects[0].text = $"<b>{keys.At(k)}</b>";
                tmpObjects[1].text = value.ToString();
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

    private void Setup3DAttributesFloatAndIntegerType(ArcGIS3DObjectSceneLayer layer)
    {
        var layerAttributes = ArcGISImmutableArray<String>.CreateBuilder();
        layerAttributes.Add("OBJECTID");
        layer.SetAttributesToVisualize(layerAttributes.MoveToArray());
        layer.MaterialReference = selectMaterial;
    }
}