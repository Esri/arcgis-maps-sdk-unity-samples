// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct WebLink
{
    public string Link;
    public string[] RequestHeaders;
}

[System.Serializable]
public class FeatureQuery
{
    public Geometry geometry = new Geometry();
    public Properties properties = new Properties();
}

[System.Serializable]
public class Properties
{
    public List<string> propertyNames = new List<string>();
    public List<string> data = new List<string>();
}

[System.Serializable]
public class Geometry
{
    public List<double> coordinates = new List<double>();
}

public class FeatureLayerManager : MonoBehaviour
{
    [SerializeField] private List<FeatureQuery> Features = new List<FeatureQuery>();
    [SerializeField] private List<GameObject> FeatureItems = new List<GameObject>();
    private FeatureData featureInfo;
    [SerializeField] private GameObject featurePrefab;
    private int featureSRWKID = 4326;
    private JToken[] jFeatures;
    private ArcGISLocationComponent locationComponent;
    [SerializeField] private WebLink WebLink;

    private void Start()
    {
        CreateLink(WebLink.Link);
        StartCoroutine(GetFeatures());
    }

    public void CreateLink(string link)
    {
        if (link != null)
        {
            foreach (var header in WebLink.RequestHeaders)
            {
                if (!link.ToLower().Contains(header))
                {
                    link += header;
                }
            }

            WebLink.Link = link;
        }
    }

    public IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        UnityWebRequest Request = UnityWebRequest.Get(WebLink.Link);
        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(Request.error);
        }
        else
        {
            CreateGameObjectsFromResponse(Request.downloadHandler.text);
        }
    }

    private void CreateGameObjectsFromResponse(string response)
    {
        // Deserialize the JSON response from the query.
        var jObject = JObject.Parse(response);
        jFeatures = jObject.SelectToken("features").ToArray();

        if (jFeatures[0].SelectToken("geometry").SelectToken("type").ToString().ToLower() == "point")
        {
                CreateFeatures(0, jFeatures.Length);
        }
    }

    private void CreateFeatures(int min, int max)
    {
        for (int featureIndex = min; featureIndex < max; featureIndex++)
        {
            FeatureQuery currentFeature = new FeatureQuery();
            var featureItem = Instantiate(featurePrefab, this.transform);
            //Layer 7 because that is the index of the layer created specifically for feature layers so that they ignore themselves for raycasting.
            featureItem.layer = 7;
            featureInfo = featureItem.GetComponent<FeatureData>();
            locationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
            var coordinates = jFeatures[featureIndex].SelectToken("geometry").SelectToken("coordinates").ToArray();
            var properties = jFeatures[featureIndex].SelectToken("properties").ToArray();

            foreach (var value in properties)
            {
                var key = value.ToString();
                var props = key.Split(":");
                currentFeature.properties.propertyNames.Add(props[0]);
                currentFeature.properties.data.Add(props[1]);
                featureInfo.Properties.Add(key);
            }

            foreach (var coordinate in coordinates)
            {
                currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                featureInfo.Coordinates.Add(Convert.ToDouble(coordinate));
            }

            var position = new ArcGISPoint(featureInfo.Coordinates[0], featureInfo.Coordinates[1],
                1000.0, new ArcGISSpatialReference(featureSRWKID));
            var rotation = new ArcGISRotation(0.0, 90.0, 0.0);
            locationComponent.enabled = true;
            locationComponent.Position = position;
            locationComponent.Rotation = rotation;
            Features.Add(currentFeature);
            FeatureItems.Add(featureItem);
        }
    }
}
