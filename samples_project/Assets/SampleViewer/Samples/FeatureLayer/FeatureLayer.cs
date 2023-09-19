using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine.Networking;
using UnityEngine;

[System.Serializable]
public struct WebLink
{
    public string Link;
    public string[] RequestHeaders;
}

[System.Serializable]
public class Feature
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

public class FeatureLayer : MonoBehaviour
{
    [SerializeField] private WebLink webLink;
    [SerializeField] private GameObject FeaturePrefab;
    [SerializeField] private List<Feature> features = new List<Feature>();
    [SerializeField] private ArcGISCameraComponent ArcGISCamera;
    private int StadiumSpawnHeight = 10000;
    private int FeatureSRWKID = 4326;

    // Start is called before the first frame update
    void Start()
    {
        CreateLink();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(GetFeatures());
        }
    }
    
    private void CreateLink()
    {
        var requestHeader = "";
        foreach (var header in webLink.RequestHeaders)
        {
            if (!requestHeader.Contains(header))
            {
                requestHeader += header;   
            }
        }
        webLink.Link += requestHeader;
    }
    
    private IEnumerator GetFeatures()
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
    
    private void CreateGameObjectsFromResponse(string Response)
    {
        if (Response != null)
        {
            // Deserialize the JSON response from the query.
            var jObject = JObject.Parse(Response);
            var jFeatures = jObject.SelectToken("features").ToArray();
            foreach (var feature in jFeatures)
            {
                Feature currentFeature = new Feature();
                var featureItem = Instantiate(FeaturePrefab, this.transform);
                var featureInfo = featureItem.GetComponent<FeatureData>();
                var LocationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
                var coordinates = feature.SelectToken("geometry").SelectToken("coordinates").ToArray();
                var properties = feature.SelectToken("properties");

                foreach (var value in properties)
                {
                    var key = value.ToString();
                    var props = key.Split(":");
                    currentFeature.properties.propertyNames.Add(props[0]);
                    currentFeature.properties.data.Add(props[1]);
                    featureInfo.properties.Add(props[1]);
                }

                if (feature.SelectToken("geometry").SelectToken("type").ToString().ToLower() == "point")
                {
                    foreach (var coordinate in coordinates)
                    {
                        currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                        featureInfo.coordinates.Add(Convert.ToDouble(coordinate));
                    }
                }

                featureInfo.ArcGISCamera = ArcGISCamera;
                ArcGISPoint Position = new ArcGISPoint(featureInfo.coordinates[0], featureInfo.coordinates[1],
                    StadiumSpawnHeight, new ArcGISSpatialReference(FeatureSRWKID));
                ArcGISRotation Rotation = new ArcGISRotation(0.0, 90.0, 0.0);
                LocationComponent.enabled = true;
                LocationComponent.Position = Position;
                LocationComponent.Rotation = Rotation;
                features.Add(currentFeature);
            }
        }
    }
}
