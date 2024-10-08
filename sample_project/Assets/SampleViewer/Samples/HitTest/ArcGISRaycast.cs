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
    private int featureId;
    private InputActions inputActions;
    private bool isLeftShiftPressed;
    private JToken[] jFeatures;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private GameObject markerGO;
    private List<string> outfields = new List<string> { "AREA_SQ_FT", "DISTRICT", "Height", "SUBDISTRIC", "ZONE_" };
    [SerializeField] private TextMeshProUGUI resultText;
    private string weblink;

    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void CreateLink(string objectID)
    {
        weblink =
            "https://services.arcgis.com/P3ePLMYs2RVChkJx/ArcGIS/rest/services/Buildings_Boston_USA/FeatureServer/0/query?f=geojson&where=1=1&objectids=" +
            objectID + "&outfields=AREA_SQ_FT,DISTRICT,Height,SUBDISTRIC,ZONE_";
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
            yield break;
        }

        resultText.text = "- FeatureID: " + featureId + "\n";
        
        foreach (var outfield in outfields)
        {
            if (GetObjectIDs(Request.downloadHandler.text, outfield) != "")
            {
                resultText.text += "- " + outfield + ": " + GetObjectIDs(Request.downloadHandler.text, outfield) + "\n";
            }
        }
    }

    private string GetObjectIDs(string response, string outfield)
    {
        var jObject = JObject.Parse(response);
        jFeatures = jObject.SelectToken("features").ToArray();
        var propertyValue = "";

        foreach (var property in jFeatures)
        {
            propertyValue = property.SelectToken("properties").SelectToken(outfield).ToString();
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

                if (layer != null && featureId != -1)
                {
                    CreateLink(featureId.ToString());
                    var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
                    var location = markerGO.GetComponent<ArcGISLocationComponent>();
                    location.Position = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z,
                        geoPosition.SpatialReference);

                    var point = ArcGISGeometryEngine.Project(geoPosition,
                        ArcGISSpatialReference.WGS84()) as ArcGISPoint;
                    locationText.text =
                        $"Lat: {string.Format("{0:0.##}", point.Y)} Long: {string.Format("{0:0.##}", point.X)}";
                }
            }
        }
    }

    private void Start()
    {
        resultText.text = "Select a building to begin.";
    }
}