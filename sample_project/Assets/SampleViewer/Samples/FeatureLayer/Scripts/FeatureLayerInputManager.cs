// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Samples.Components;
using System.Collections;
using System.Collections.Generic;
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
    private GameObject selectionOutline;

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
        inputManager = FindAnyObjectByType<InputManager>();
    }

    private void OnDisable()
    {
        ClearSelectionOutline();
    }

    private void ClearSelectionOutline()
    {
        if (selectionOutline)
        {
            selectionOutline.SetActive(false);
            Destroy(selectionOutline);
            selectionOutline = null;
        }
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
            ClearSelectionOutline();

            FindAnyObjectByType<FeatureLayerUIManager>().DropDownButton.isOn = false;
            EmptyPropertiesDropdown();
            FeatureData = hit.collider.gameObject.GetComponent<FeatureData>();

            if (!FeatureData)
            {
                return;
            }

            if (!FindAnyObjectByType<FeatureLayer>().GetAllOutfields)
            {
                var featureLayer = FindAnyObjectByType<FeatureLayer>();
                featureLayer.RefreshProperties(FeatureData.gameObject);
            }

            foreach (var property in FeatureData.Properties)
            {
                var item = Instantiate(properties);
                items.Add(item);
                item.GetComponentInChildren<TextMeshProUGUI>().text = property;
            }

            StartCoroutine("AddItemsToScrollView");
            ShowSelectionOutline(FeatureData);
            propertiesView.SetActive(true);
        }
    }

    private void ShowSelectionOutline(FeatureData feature)
    {
        var pin = feature.GetComponentInChildren<MeshFilter>();
        if (!pin || !pin.sharedMesh || !outlineMat)
        {
            Debug.LogWarning("Cannot highlight the feature pin without a mesh and outline material.", feature);
            return;
        }

        var mesh = pin.sharedMesh;
        const float outlineScale = 1.05f;
        selectionOutline = new GameObject("Selection Outline");
        selectionOutline.layer = pin.gameObject.layer;
        selectionOutline.transform.SetParent(pin.transform, false);
        selectionOutline.transform.localScale = Vector3.one * outlineScale;
        selectionOutline.transform.localPosition = mesh.bounds.center * (1f - outlineScale);
        selectionOutline.AddComponent<MeshFilter>().sharedMesh = mesh;

        // A separate renderer draws the outline on every submesh without replacing the pin materials.
        var materials = new Material[mesh.subMeshCount];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = outlineMat;
        }

        var renderer = selectionOutline.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = materials;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private void SetScrollViewItems(GameObject item)
    {
        item.transform.SetParent(container);
        item.transform.localScale = Vector2.one;
    }
}