// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using System.Collections.Generic;
using UnityEngine;

using Esri.HPFramework;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayCastLocation : MonoBehaviour
{
    private readonly string LocationQueryURL = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode";

    [Header("------------Prefabs------------")]
    [SerializeField] private GameObject addressCardTemplate;

    private ArcGISMapComponent arcGISMapComponent;

    [Header("------------Variables------------")]
    [SerializeField] private float locationMarkerScale = 1;

    [SerializeField] private GameObject locationMarkerTemplate;
    private Camera mainCamera;
    [SerializeField] private GameObject predictionRay;
    private GameObject queryLocationGO;

    //[SerializeField] private LayerMask raycastLayer;
    [SerializeField] private XRRayInteractor raycastHand;

    //[SerializeField] private float maxRayDistance = 35;
    [SerializeField] private InputActionProperty raycastInput;

    private string responseAddress = "";

    /// <summary>
    /// Return GeoPosition for an engine location hit by a raycast.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    public ArcGISPoint HitToGeoPosition(RaycastHit hit, float yOffset = 0)
    {
        var worldPosition = math.inverse(arcGISMapComponent.WorldMatrix).HomogeneousTransformPoint(hit.point.ToDouble3());
        var geoPosition = arcGISMapComponent.View.WorldToGeographic(worldPosition);
        var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + yOffset, geoPosition.SpatialReference);

        return GeoUtils.ProjectToSpatialReference(offsetPosition, new ArcGISSpatialReference(4326));
    }

    /// <summary>
    /// Perform a reverse geocoding query (location lookup) and parse the response. If the server returned an error, the message is shown to the user.
    /// The function is called when a location on the map is selected.
    /// </summary>
    /// <param name="location"></param>
    public async Task ReverseGeocode(ArcGISPoint location)
    {
        string results = await SendLocationQuery(location.X.ToString() + "," + location.Y.ToString());

        if (results.Contains("error")) // Server returned an error
        {
            var response = JObject.Parse(results);
            var error = response.SelectToken("error");
            responseAddress = (string)error.SelectToken("message");
        }
        else
        {
            var response = JObject.Parse(results);
            var address = response.SelectToken("address");
            var label = address.SelectToken("LongLabel");
            responseAddress = (string)label;

            if (string.IsNullOrEmpty(responseAddress))
            {
                responseAddress = "Query did not return a valid response.";
            }
            else
            {
                CreateAddressCard();
            }
        }
    }

    /// <summary>
    /// Create a visual cue for showing the address/description returned for the query.
    /// </summary>
    /// <param name="isAddressQuery"></param>
    private void CreateAddressCard()
    {
        GameObject card = Instantiate(addressCardTemplate, queryLocationGO.transform);
        TextMeshProUGUI t = card.GetComponentInChildren<TextMeshProUGUI>();

        // Based on the type of the query set the location, rotation and scale of the text relative to the query location game object
        float localScale = 3.5f / locationMarkerScale;
        card.transform.localPosition = new Vector3(0, 300f / locationMarkerScale, -300f / locationMarkerScale);
        card.transform.localScale = new Vector3(localScale, localScale, localScale);

        if (t != null)
        {
            t.text = responseAddress;
        }
    }

    private async void GetAddress()
    {
        if (Physics.Raycast(raycastHand.rayOriginTransform.position, raycastHand.rayOriginTransform.forward, out RaycastHit hit))
        {
            Vector3 direction = (hit.point - mainCamera.transform.position);
            float distanceFromCamera = Vector3.Distance(mainCamera.transform.position, hit.point);
            float scale = distanceFromCamera * locationMarkerScale / 5000; // Scale the marker based on its distance from camera
            SetupQueryLocationGameObject(locationMarkerTemplate, hit.point, mainCamera.transform.rotation, new Vector3(scale, scale, scale));
            await ReverseGeocode(HitToGeoPosition(hit));
        }
    }

    /// <summary>
    ///  Create and send an HTTP request for a reverse geocoding query and return the received response.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    private async Task<string> SendLocationQuery(string location)
    {
        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("location", location),
            new KeyValuePair<string, string>("langCode", "en"),
            new KeyValuePair<string, string>("f", "json"),
        };

        HttpClient client = new HttpClient();
        HttpContent content = new FormUrlEncodedContent(payload);
        HttpResponseMessage response = await client.PostAsync(LocationQueryURL, content);

        response.EnsureSuccessStatusCode();
        string results = await response.Content.ReadAsStringAsync();
        return results;
    }

    /// <summary>
    /// Create an instance of the template game object and sets the transform based on input arguments. Ensures the game object has an ArcGISLocation component attached.
    /// </summary>
    /// <param name="templateGO"></param>
    /// <param name="location"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    private void SetupQueryLocationGameObject(GameObject templateGO,
        Vector3 location = new Vector3(), Quaternion rotation = new Quaternion(), Vector3 scale = new Vector3())
    {
        ArcGISLocationComponent MarkerLocComp;

        if (queryLocationGO != null)
        {
            Destroy(queryLocationGO);
        }

        queryLocationGO = Instantiate(templateGO, location, rotation, arcGISMapComponent.transform);
        queryLocationGO.transform.localScale = scale;

        if (!queryLocationGO.TryGetComponent<ArcGISLocationComponent>(out MarkerLocComp))
        {
            MarkerLocComp = queryLocationGO.AddComponent<ArcGISLocationComponent>();
        }
        MarkerLocComp.enabled = true;
    }

    private void Start()
    {
        arcGISMapComponent = FindFirstObjectByType<ArcGISMapComponent>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Set Ray to only be visible when you are able to raycast
        predictionRay.SetActive(Physics.Raycast(raycastHand.rayOriginTransform.position, raycastHand.rayOriginTransform.forward, out RaycastHit hit));

        if (raycastInput.action.WasPressedThisFrame())
        {
            GetAddress();
            Debug.Log("Input Triggered");
        }
    }
}