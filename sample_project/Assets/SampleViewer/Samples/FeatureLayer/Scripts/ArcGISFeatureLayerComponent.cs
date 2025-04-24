// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine.Networking;
using UnityEngine;

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
        public List<double> Coordinates = new List<double>();
    }

    [System.Serializable]
    public class PropertyData
    {
        public List<string> PropertyNames = new List<string>();
        public List<string> Data = new List<string>();
    }

    [System.Serializable]
    public class FeatureQueryData
    {
        public GeometryData Geometry = new GeometryData();
        public PropertyData Properties = new PropertyData();
    }

    private List<FeatureQueryData> Features = new List<FeatureQueryData>();
    private FeatureData featureInfo;
    [SerializeField] private GameObject featurePrefab;
    private JToken[] jFeatures;
    private float spawnHeight = 10000.0f;

    public List<GameObject> FeatureItems = new List<GameObject>();
    public QueryLink WebLink;

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
                currentFeature.Properties.PropertyNames.Add(props[0]);
                currentFeature.Properties.Data.Add(props[1]);
                featureInfo.Properties.Add(key);
            }

            foreach (var coordinate in coordinates)
            {
                currentFeature.Geometry.Coordinates.Add(Convert.ToDouble(coordinate));
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
