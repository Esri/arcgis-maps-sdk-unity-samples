// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Linq;
using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Components;
using System;

public class ArcGISFeatureLayerComponent : MonoBehaviour
{
    [System.Serializable]
    public struct QueryLink
    {
        public string Link;
        public string[] RequestHeaders;
    }

    [System.Serializable]
    public class GeometryData
    {
        public List<double> coordinates = new List<double>();
    }

    [System.Serializable]
    public class PropertyData
    {
        public List<string> propertyNames = new List<string>();
        public List<string> data = new List<string>();
    }

    [System.Serializable]
    public class FeatureQueryData
    {
        public GeometryData geometry = new GeometryData();
        public PropertyData properties = new PropertyData();
    }

    private List<FeatureQueryData> Features = new List<FeatureQueryData>();
    private FeatureData featureInfo;
    public List<GameObject> FeatureItems = new List<GameObject>();
    [SerializeField] private GameObject featurePrefab;
    private float spawnHeight = 10000.0f;

    public JToken[] jFeatures;
    public QueryLink webLink;

    public void CreateLink(string link)
    {
        if (link != null)
        {
            foreach (var header in webLink.RequestHeaders)
            {
                if (!link.ToLower().Contains(header))
                {
                    link += header;
                }
            }

            webLink.Link = link;
        }
    }

    public IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        UnityWebRequest Request = UnityWebRequest.Get(webLink.Link);
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

        if (jFeatures[0].SelectToken("geometry").SelectToken("type").ToString().ToLower() != "point")
        {
            return;
        }

        CreateFeatures();
    }

    private void CreateFeatures()
    {
        foreach (var feature in jFeatures)
        {
            var currentFeature = new FeatureQueryData();
            var featureItem = Instantiate(featurePrefab, this.transform);
            //Layer 7 because that is the index of the layer created specifically for feature layers so that they ignore themselves for raycasting.
            featureItem.layer = 7;
            featureInfo = featureItem.GetComponent<FeatureData>();
            var locationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
            var coordinates = feature.SelectToken("geometry").SelectToken("coordinates").ToArray();
            var properties = feature.SelectToken("properties").ToArray();

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
                spawnHeight, new ArcGISSpatialReference(4326));
            var rotation = new ArcGISRotation(0.0, 90.0, 0.0);
            locationComponent.enabled = true;
            locationComponent.Position = position;
            locationComponent.Rotation = rotation;
            Features.Add(currentFeature);
            FeatureItems.Add(featureItem);
        }
    }
}
