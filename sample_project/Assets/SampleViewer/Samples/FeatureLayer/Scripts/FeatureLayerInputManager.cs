// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Samples.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FeatureLayerInputManager : MonoBehaviour
{
    public FeatureData FeatureData;

    [SerializeField] private Transform container;
    [SerializeField] private Material outlineMat;
    [SerializeField] private GameObject properties;
    [SerializeField] private GameObject propertiesView;

    private InputManager inputManager;
    private bool isRunning;
    private List<GameObject> items = new List<GameObject>();
    private FeatureLayerUIManager uiManager;

    private IEnumerator AddItemsToScrollView()
    {
        isRunning = true;

        foreach (var item in items)
        {
            SetScrollViewItems(item);
            yield return new WaitForSeconds(0.01f);
        }

        StopCoroutine("AddItemsToScrollView");
    }

    private void Awake()
    {
        inputManager = FindFirstObjectByType<InputManager>();
    }

    public void ClearAdditionalMaterial(GameObject feature)
    {
        var renderer = feature.GetComponentInChildren<Renderer>();

        if (renderer == null)
        {
            return;
        }

        renderer.materials = renderer.materials.SkipLast(1).ToArray();
    }

    public void EmptyPropertiesDropdown()
    {
        if (items == null)
        {
            return;
        }

        foreach (var item in items)
        {
            Destroy(item.gameObject);
        }

        items.Clear();
    }

    public void OnClick()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        HandleInput(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
#else
        HandleInput(Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>()));
#endif
    }

    private void HandleInput(Ray ray)
    {
        if (isRunning)
        {
            StopCoroutine("AddItemsToScrollView");
            isRunning = false;
        }

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (FeatureData)
            {
                ClearAdditionalMaterial(FeatureData.gameObject);
            }

            FindFirstObjectByType<FeatureLayerUIManager>().DropDownButton.isOn = false;
            EmptyPropertiesDropdown();
            FeatureData = hit.collider.gameObject.GetComponent<FeatureData>();

            if (!FeatureData)
            {
                return;
            }

            if (!FindFirstObjectByType<FeatureLayer>().GetAllOutfields)
            {
                var featureLayer = FindFirstObjectByType<FeatureLayer>();
                featureLayer.RefreshProperties(FeatureData.gameObject);
            }

            foreach (var property in FeatureData.Properties)
            {
                var item = Instantiate(properties);
                items.Add(item);
                item.GetComponentInChildren<TextMeshProUGUI>().text = property;
            }

            StartCoroutine("AddItemsToScrollView");
            SetAdditionalMaterial(outlineMat, hit.collider);
            propertiesView.SetActive(true);
        }
    }

    private void SetAdditionalMaterial(Material outLine, Collider collider)
    {
        var renderer = collider.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material[] materialsArray = new Material[renderer.materials.Length + 1];
        renderer.materials.CopyTo(materialsArray, 0);
        materialsArray[materialsArray.Length - 1] = outLine;
        renderer.materials = materialsArray;
    }

    private void SetScrollViewItems(GameObject item)
    {
        item.transform.SetParent(container);
        item.transform.localScale = Vector2.one;
    }
}