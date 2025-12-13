// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Layers;
using Esri.GameEngine.MapView;
using Esri.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Identify : MonoBehaviour
{
    [Header("Identify Variables")]
    [SerializeField] private ArcGISMapComponent arcGISMapComponent;
    private ArcGIS3DObjectSceneLayer buildingLayer;
    [SerializeField] private Transform buildingListContentContainer;
    private List<GameObject> buildingListItems = new List<GameObject>();
    [SerializeField] private GameObject buildingToggle;
    [SerializeField] private Transform contentContainer;
    private List<GameObject> ListItems = new List<GameObject>();
    [SerializeField] private GameObject markerGO;
    [SerializeField] private float maxRayLength = 10000000;
    [SerializeField] private GameObject scrollViewItem;
    [SerializeField] private Color selectColor;
    [SerializeField] private Material selectMaterial;

    [Header("UI Elements")]
    [SerializeField] private GameObject buildingsView;
    [SerializeField] private Button changeViewButton;
    [SerializeField] private Button decreaseResult;
    [SerializeField] private Button increaseResult;
    [SerializeField] private GameObject menuBar;
    [SerializeField] private TextMeshProUGUI resultAmount;
    [SerializeField] private GameObject results;
    [SerializeField] private Button showResultsButton;
    [SerializeField] private TextMeshProUGUI totalNumberOfBuildingsText;

    // Select Variables
    private ArcGISImmutableCollection<ArcGISIdentifyLayerResult> identifyLayerResults;
    private ulong resultsLength;
    private float selectedID;
    [HideInInspector] public ulong SelectedResult = 0;

    [Header("Thread Safety")]
    [SerializeField] private float identifyLayersTimeoutSeconds = 5f;
    [SerializeField] private bool cancelOnTimeout = true; // If true, call Cancel() on the future when timing out.
    [SerializeField] private bool verboseResultLogging = true; // Dump full attribute JSON-ish payload; disable for brevity.
    private readonly Queue<System.Action> mainThreadActions = new(); // Actions queued from callback
    private Coroutine activeIdentifyLayersCoroutine;

    private IEnumerator CallbackTimeoutWatcher(ArcGISFuture<ArcGISImmutableCollection<ArcGISIdentifyLayerResult>> future, float start, System.Action onTimeout)
    {
        while (!future.IsDone())
        {
            if (identifyLayersTimeoutSeconds > 0 && (Time.realtimeSinceStartup - start) > identifyLayersTimeoutSeconds)
            {
                Debug.LogWarning("IdentifyLayersAsync (callback) timed out.");
                if (cancelOnTimeout)
                {
                    future.Cancel();
                }
                onTimeout?.Invoke();
                yield break;
            }
            yield return null;
        }
    }

    //Filter out incorrect date format. See known issue (BUG-000181006): https://developers.arcgis.com/unity/release-notes/release-notes-2-2-0/.
    private bool DateTimeCheck(object AttributeValue)
    {
        if (AttributeValue.GetType() != typeof(DateTime))
        {
            return false;
        }

        var DateTime = (DateTime)AttributeValue;
        Int32 year = DateTime.Year;
        return (year < 1800);
    }

    private void DisableButtons(bool enabled)
    {
        increaseResult.interactable = enabled;
        decreaseResult.interactable = enabled;
    }

    public void ResetButton()
    {
        Shader.SetGlobalFloat("_SelectedObjectID", -1);
        identifyLayerResults = null;
        resultAmount.text = "";
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
            menuBar.SetActive(false);
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
            menuBar.SetActive(false);
        }
    }

    public void ParseResults(ulong NumberOfResults, ArcGISImmutableCollection<ArcGISIdentifyLayerResult> identifyLayerResults)
    {
        if (!verboseResultLogging)
        {
            var item = Instantiate(scrollViewItem);
            var tmpObjects = item.GetComponentsInChildren<TextMeshProUGUI>();
            tmpObjects[0].text = $"<IdentifyLayerResults Size={identifyLayerResults.GetSize()}>";
            item.transform.SetParent(contentContainer);
            item.transform.localScale = Vector2.one;
            ListItems.Add(item);
            return;
        }

        if (identifyLayerResults.IsEmpty())
        {
            Debug.LogWarning("No Results Found");
            Shader.SetGlobalFloat("_SelectedObjectID", -1);
            return;
        }

        var elements = identifyLayerResults.At(0).GeoElements;
        resultsLength = elements.GetSize();

        if (resultsLength == 0)
        {
            Debug.LogWarning("No Results Found");
            Shader.SetGlobalFloat("_SelectedObjectID", -1);
            return;
        }
        else if (resultsLength == 1)
        {
            DisableButtons(false);
        }
        else if (resultsLength > 1)
        {
            DisableButtons(true);
        }

        totalNumberOfBuildingsText.text = $"Total: {resultsLength}";
        resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
        var attributes = elements.At(NumberOfResults).Attributes;
        var keys = attributes.Keys;
        menuBar.SetActive(true);
        buildingsView.SetActive(false);
        results.SetActive(true);

        try
        {
            // Grabs objectID and sends it to shader to allow for Highlighting of the buildings.
            // Only one building can be highlighted at a time
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

                if (DateTimeCheck(value))
                {
                    tmpObjects[1].text = "";
                }

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

    private void PopulateBuildingList()
    {
        for (int i = 0; i < (int)resultsLength; i++)
        {
            var item = Instantiate(buildingToggle);
            var tmpObject = item.GetComponentInChildren<TextMeshProUGUI>();
            item.GetComponent<BuildingToggleItem>().BuildingNumber = (ulong)i;
            item.GetComponent<BuildingToggleItem>().IdentifyLayerResults = identifyLayerResults;
            tmpObject.text = $"Building {i + 1}";

            if (i == 0)
            {
                item.GetComponent<BuildingToggleItem>().toggleImage.sprite = item.GetComponent<BuildingToggleItem>().isOn;
            }
            else
            {
                item.GetComponentInChildren<UnityEngine.UI.Image>().sprite = item.GetComponent<BuildingToggleItem>().isOff;
            }

            item.transform.SetParent(buildingListContentContainer);
            item.transform.localScale = Vector2.one;
            buildingListItems.Add(item);
        }
    }

    private void Start()
    {
        if (!arcGISMapComponent)
        {
            return;
        }

        buildingLayer = new ArcGIS3DObjectSceneLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "Building Layer", 1.0f, true, "");
        arcGISMapComponent.Map.Layers.Add(buildingLayer);

        if (!buildingLayer)
        {
            return;
        }

        Setup3DAttributesFloatAndIntegerType(buildingLayer);
        resultAmount.text = "";
        totalNumberOfBuildingsText.text = "";
        buildingsView.SetActive(false);
        results.SetActive(false);
        menuBar.SetActive(false);
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

                foreach (var item in FindObjectsByType<BuildingToggleItem>(FindObjectsSortMode.None))
                {
                    if (item.BuildingNumber == SelectedResult)
                    {
                        item.toggleImage.sprite = item.isOn;
                    }
                    else
                    {
                        item.toggleImage.sprite = item.isOff;
                    }
                }
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
                ++SelectedResult;
                EmptyIdentifyResults();
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, identifyLayerResults);
            }
            else if (SelectedResult == resultsLength - 1)
            {
                EmptyIdentifyResults();
                SelectedResult = 0;
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, identifyLayerResults);
            }
        });

        decreaseResult.onClick.AddListener(delegate
        {
            if (SelectedResult > 0)
            {
                EmptyIdentifyResults();
                --SelectedResult;
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, identifyLayerResults);
            }
            else if (SelectedResult == 0)
            {
                EmptyIdentifyResults();
                SelectedResult = resultsLength - 1;
                resultAmount.text = $"{SelectedResult + 1} of {resultsLength}";
                ParseResults(SelectedResult, identifyLayerResults);
            }
        });

        Shader.SetGlobalFloat("_SelectedObjectID", -1);
        Shader.SetGlobalColor("_HighlightColor", selectColor);
    }

    public void StartRaycast()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#else
        Ray ray = Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>());
#endif
        EmptyIdentifyResults();
        EmptyBuildingListResults();
        SelectedResult = 0;

        if (activeIdentifyLayersCoroutine != null)
        {
            StopCoroutine(activeIdentifyLayersCoroutine);
            activeIdentifyLayersCoroutine = null;
        }

        var startGeoPosition = arcGISMapComponent.EngineToGeographic(ray.origin);
        var endPoint = ray.GetPoint(maxRayLength);
        var endGeoPosition = arcGISMapComponent.EngineToGeographic(endPoint);

        var future = arcGISMapComponent.View.IdentifyLayersAsync(startGeoPosition, endGeoPosition, -1);
        var startTime = Time.realtimeSinceStartup;
        var timedOut = false;

        if (identifyLayersTimeoutSeconds > 0)
        {
            StartCoroutine(CallbackTimeoutWatcher(future, startTime, () => timedOut = true));
        }


        future.TaskCompleted = () =>
        {
            mainThreadActions.Enqueue(() =>
            {
                if (timedOut)
                {
                    Debug.LogWarning("IdentifyLayersAsync (callback) completed after timeout; ignoring.");
                    return;
                }

                if (future.GetError() is System.Exception futureError)
                {
                    Debug.LogError($"IdentifyLayersAsync (callback) error: {futureError.Message}\n{futureError}");
                    return;
                }

                var layerResults = future.Get();

                if (layerResults == null)
                {
                    Debug.Log("IdentifyLayersAsync (callback) null results");
                    return;
                }

                identifyLayerResults = future.Get();
                ParseResults(SelectedResult, identifyLayerResults);
                PopulateBuildingList();
            });
        };
    }

    private void Setup3DAttributesFloatAndIntegerType(ArcGIS3DObjectSceneLayer layer)
    {
        var layerAttributes = ArcGISImmutableArray<String>.CreateBuilder();
        layerAttributes.Add("OBJECTID");
        layer.SetAttributesToVisualize(layerAttributes.MoveToArray());
        layer.MaterialReference = selectMaterial;
    }

    private void Update()
    {
        // Drain queued actions from callback completions
        if (mainThreadActions.Count > 0)
        {
            var pendingActionCount = mainThreadActions.Count;

            for (var pendingActionIndex = 0; pendingActionIndex < pendingActionCount; pendingActionIndex++)
            {
                var queuedAction = mainThreadActions.Dequeue();

                try
                {
                    queuedAction?.Invoke();
                }
                catch (Exception callbackException)
                {
                    Debug.LogError($"Callback action error: {callbackException}");
                }
            }
        }
    }
}