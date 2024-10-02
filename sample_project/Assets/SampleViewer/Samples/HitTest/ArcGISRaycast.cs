// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ArcGISRaycast : MonoBehaviour
{
    public ArcGISMapComponent arcGISMapComponent;
    public ArcGISCameraComponent arcGISCamera;
    private int featureId;
    private InputActions inputActions;
    private bool isLeftShiftPressed;
    private JToken[] jFeatures;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private GameObject markerGO;
    private List<String> outfields = new List<String>{"AREA_SQ_FT", "DISTRICT", "Height", "SUBDISTRIC", "ZONE_"};
    private List<String> properties = new List<String>{"Area", "District", "Height", "Sub District", "Zone"};
    [SerializeField] private TMP_Dropdown scrollView;
    [SerializeField] private bool supressWarnings;
    [SerializeField] private Image warningImage;
    private string weblink;

    [SerializeField] private TextMeshProUGUI property;

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void CreateLink(string objectID)
    {
        weblink = "https://services.arcgis.com/P3ePLMYs2RVChkJx/ArcGIS/rest/services/Buildings_Boston_USA/FeatureServer/0/query?f=geojson&where=1=1&objectids=" + objectID + "&outfields=*";
        StartCoroutine(GetFeatures(objectID));
    }
    
    private IEnumerator GetFeatures(string objectID)
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        UnityWebRequest Request = UnityWebRequest.Get(weblink);
        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(Request.error);
        }
        else
        {
            if (scrollView.value < 0)
            {
                property.text = "Feature ID: " + objectID;
                warningImage.enabled = true;

                if (!supressWarnings)
                {
                    Debug.LogWarning("Please select a value to get from the dropdown");
                }

            }
            else if (GetObjectIDs(Request.downloadHandler.text).Length == 0)
            {
                property.text = "Feature ID: " + objectID;
                warningImage.enabled = true;

                if (!supressWarnings)
                {
                    Debug.LogWarning(scrollView.captionText.text + ": " + "No value was found for that property here.");
                }
            }
            else if (scrollView.value == 0 || scrollView.value == 2)
            {
                warningImage.enabled = false;
                property.text = scrollView.captionText.text + ": " + GetObjectIDs(Request.downloadHandler.text) + " ft";
            }
            else
            {
                warningImage.enabled = false;
                property.text = scrollView.captionText.text + ": " + GetObjectIDs(Request.downloadHandler.text);                
            }
        }
    }
    
    private String GetObjectIDs(string response)
    {
        var jObject = JObject.Parse(response);
        jFeatures = jObject.SelectToken("features").ToArray();
        var propertyValue = "";
        
        foreach (var property in jFeatures)
        {
            propertyValue = property.SelectToken("properties").SelectToken(outfields[scrollView.value]).ToString();
        }

        return propertyValue;
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.DrawingControls.LeftClick.started += OnLeftClickStart;
        inputActions.DrawingControls.LeftShift.performed += ctx => OnLeftShift(true);
        inputActions.DrawingControls.LeftShift.canceled += ctx => OnLeftShift(false);
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.DrawingControls.LeftClick.started -= OnLeftClickStart;
        inputActions.DrawingControls.LeftShift.performed -= ctx => OnLeftShift(true);
        inputActions.DrawingControls.LeftShift.canceled -= ctx => OnLeftShift(false);
    }

    private void OnLeftShift(bool isPressed)
    {
        isLeftShiftPressed = isPressed;
    }

    private void OnLeftClickStart(InputAction.CallbackContext context)
    {
        if (isLeftShiftPressed && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out hit))
            {
                var arcGISRaycastHit = arcGISMapComponent.GetArcGISRaycastHit(hit);
                var layer = arcGISRaycastHit.layer;
                featureId = arcGISRaycastHit.featureId;
                
                CreateLink(featureId.ToString());

                if (layer != null)
                {
                    var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
                    var location = markerGO.GetComponent<ArcGISLocationComponent>();
                    location.Position = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z, geoPosition.SpatialReference);
                    
                    var point = ArcGISGeometryEngine.Project(geoPosition, ArcGISSpatialReference.WGS84()) as ArcGISPoint;
                    var lat = Mathf.Round((float)point.Y * 1000) / 1000;
                    var lon = Mathf.Round((float)point.X * 1000) / 1000; 
                    locationText.text = "Lat: " + lat + " Long: " + lon;
                }
            }
        }
    }
    
    private void PopulateOutfieldsDropdown()
    {
        scrollView.AddOptions(properties);
    }
    
    private void Start()
    {
        warningImage.enabled = false;
        PopulateOutfieldsDropdown();

        scrollView.onValueChanged.AddListener(delegate
        {
            if (featureId != 0)
            {
                CreateLink(featureId.ToString());
            }
        });
    }
}