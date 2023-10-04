using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private ArcGISCameraComponent arcGISCamera;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject featurePrefab;
    private int featureSRWKID = 4326;
    [SerializeField] private List<string> outfields = new List<string>();
    private int stadiumSpawnHeight = 10000;
    private FeatureLayerUIManager UIManager;

    public List<Feature> Features = new List<Feature>();
    public bool GetAllFeatures = true;
    public bool GetAllOutfields = true;
    public JToken[] jFeatures;
    public int LastValue = 1;
    public List<GameObject> FeatureItems = new List<GameObject>();
    public List<Toggle> ListItems = new List<Toggle>();
    public bool NewLink = true;
    public GameObject OutfieldItem;
    public List<string> OutfieldsToGet = new List<string>();
    public int StartValue;
    public WebLink WebLink;

    private void Start()
    {
        CreateLink(WebLink.Link);
        StartCoroutine(GetFeatures());
        UIManager = GetComponent<FeatureLayerUIManager>();
    }

    public void CreateLink(string link)
    {
        if (link != null)
        {
            EmptyOutfieldsDropdown();
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
            UIManager.DisplayText = FeatureLayerUIManager.TextToDisplay.LinkError;
        }
        else
        {
            if (NewLink)
            {
                PopulateOutfieldsDropdown(Request.downloadHandler.text);
            }
            else
            {
                CreateGameObjectsFromResponse(Request.downloadHandler.text);
                MoveCamera();
                if (UIManager.DisplayText != FeatureLayerUIManager.TextToDisplay.CoordinatesError
                    && UIManager.DisplayText != FeatureLayerUIManager.TextToDisplay.IndexOutOfBoundsError)
                {
                    UIManager.DisplayText = FeatureLayerUIManager.TextToDisplay.Information;
                }
            }
            
            NewLink = false;
        }
    }

    private void CreateGameObjectsFromResponse(string Response)
    {
        // Deserialize the JSON response from the query.
        var jObject = JObject.Parse(Response);
        jFeatures = jObject.SelectToken("features").ToArray();

        if (jFeatures[0].SelectToken("geometry").SelectToken("type").ToString().ToLower() == "point")
        {
            if (GetAllFeatures)
            {
                foreach (var feature in jFeatures)
                {
                    Feature currentFeature = new Feature();
                    var featureItem = Instantiate(featurePrefab, this.transform);
                    featureItem.tag = "FeatureItem";
                    featureItem.layer = 7;
                    var featureInfo = featureItem.GetComponent<FeatureData>();
                    var locationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
                    var coordinates = feature.SelectToken("geometry").SelectToken("coordinates").ToArray();
                    var properties = feature.SelectToken("properties").ToArray();

                    if (GetAllOutfields)
                    {
                        foreach (var value in properties)
                        {
                            var key = value.ToString();
                            var props = key.Split(":");
                            currentFeature.properties.propertyNames.Add(props[0]);
                            currentFeature.properties.data.Add(props[1]);
                            featureInfo.properties.Add(props[1]);
                        }
                    }
                    else
                    {
                        for (var j = 0; j < outfields.Count; j++)
                        {
                            if (OutfieldsToGet.Contains(outfields[j]))
                            {
                                var key = properties[j].ToString();
                                var props = key.Split(":");
                                currentFeature.properties.propertyNames.Add(props[0]);
                                currentFeature.properties.data.Add(props[1]);
                                featureInfo.properties.Add(props[1]);
                            }
                        }
                    }

                    foreach (var coordinate in coordinates)
                    {
                        currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                        featureInfo.Coordinates.Add(Convert.ToDouble(coordinate));
                    }

                    featureInfo.ArcGISCamera = arcGISCamera;
                    ArcGISPoint Position = new ArcGISPoint(featureInfo.Coordinates[0], featureInfo.Coordinates[1],
                        stadiumSpawnHeight, new ArcGISSpatialReference(featureSRWKID));
                    ArcGISRotation Rotation = new ArcGISRotation(0.0, 90.0, 0.0);
                    locationComponent.enabled = true;
                    locationComponent.Position = Position;
                    locationComponent.Rotation = Rotation;
                    Features.Add(currentFeature);
                    FeatureItems.Add(featureItem);
                }
            }
            else
            {
                if (jFeatures.Length < LastValue)
                {
                    UIManager.DisplayText = FeatureLayerUIManager.TextToDisplay.IndexOutOfBoundsError;
                    for (int i = StartValue; i < jFeatures.Length; i++)
                    {
                        Feature currentFeature = new Feature();
                        var featureItem = Instantiate(featurePrefab, this.transform);
                        featureItem.tag = "FeatureItem";
                        featureItem.layer = 7;
                        var featureInfo = featureItem.GetComponent<FeatureData>();
                        var locationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
                        var coordinates = jFeatures[i].SelectToken("geometry").SelectToken("coordinates").ToArray();
                        var properties = jFeatures[i].SelectToken("properties").ToArray();

                        if (GetAllOutfields)
                        {
                            foreach (var value in properties)
                            {
                                var key = value.ToString();
                                var props = key.Split(":");
                                currentFeature.properties.propertyNames.Add(props[0]);
                                currentFeature.properties.data.Add(props[1]);
                                featureInfo.properties.Add(props[1]);
                            }
                        }
                        else
                        {
                            for (var j = 0; j < outfields.Count; j++)
                            {
                                if (OutfieldsToGet.Contains(outfields[j]))
                                {
                                    var key = properties[j].ToString();
                                    var props = key.Split(":");
                                    currentFeature.properties.propertyNames.Add(props[0]);
                                    currentFeature.properties.data.Add(props[1]);
                                    featureInfo.properties.Add(props[1]);
                                }
                            }
                        }

                        foreach (var coordinate in coordinates)
                        {
                            currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                            featureInfo.Coordinates.Add(Convert.ToDouble(coordinate));
                        }

                        featureInfo.ArcGISCamera = arcGISCamera;
                        ArcGISPoint Position = new ArcGISPoint(featureInfo.Coordinates[0], featureInfo.Coordinates[1],
                            stadiumSpawnHeight, new ArcGISSpatialReference(featureSRWKID));
                        ArcGISRotation Rotation = new ArcGISRotation(0.0, 90.0, 0.0);
                        locationComponent.enabled = true;
                        locationComponent.Position = Position;
                        locationComponent.Rotation = Rotation;
                        Features.Add(currentFeature);
                        FeatureItems.Add(featureItem);
                    }
                }
                else
                {
                    for (int i = StartValue; i <= LastValue; i++)
                    {
                        Feature currentFeature = new Feature();
                        var featureItem = Instantiate(featurePrefab, this.transform);
                        featureItem.tag = "FeatureItem";
                        featureItem.layer = 7;
                        var featureInfo = featureItem.GetComponent<FeatureData>();
                        var locationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
                        var coordinates = jFeatures[i].SelectToken("geometry").SelectToken("coordinates").ToArray();
                        var properties = jFeatures[i].SelectToken("properties").ToArray();

                        if (GetAllOutfields)
                        {
                            foreach (var value in properties)
                            {
                                var key = value.ToString();
                                var props = key.Split(":");
                                currentFeature.properties.propertyNames.Add(props[0]);
                                currentFeature.properties.data.Add(props[1]);
                                featureInfo.properties.Add(props[1]);
                            }
                        }
                        else
                        {
                            for (var j = 0; j < outfields.Count; j++)
                            {
                                if (OutfieldsToGet.Contains(outfields[j]))
                                {
                                    var key = properties[j].ToString();
                                    var props = key.Split(":");
                                    currentFeature.properties.propertyNames.Add(props[0]);
                                    currentFeature.properties.data.Add(props[1]);
                                    featureInfo.properties.Add(props[1]);
                                }
                            }
                        }

                        foreach (var coordinate in coordinates)
                        {
                            currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                            featureInfo.Coordinates.Add(Convert.ToDouble(coordinate));
                        }

                        featureInfo.ArcGISCamera = arcGISCamera;
                        ArcGISPoint Position = new ArcGISPoint(featureInfo.Coordinates[0], featureInfo.Coordinates[1],
                            stadiumSpawnHeight, new ArcGISSpatialReference(featureSRWKID));
                        ArcGISRotation Rotation = new ArcGISRotation(0.0, 90.0, 0.0);
                        locationComponent.enabled = true;
                        locationComponent.Position = Position;
                        locationComponent.Rotation = Rotation;
                        Features.Add(currentFeature);
                        FeatureItems.Add(featureItem);
                    }
                }
            }
            if (UIManager.DisplayText != FeatureLayerUIManager.TextToDisplay.LinkError
                && UIManager.DisplayText != FeatureLayerUIManager.TextToDisplay.IndexOutOfBoundsError)
            {
                UIManager.DisplayText = FeatureLayerUIManager.TextToDisplay.Information;
            }
        }
        else
        {
            UIManager.DisplayText = FeatureLayerUIManager.TextToDisplay.CoordinatesError;
        }
    }

    private void EmptyOutfieldsDropdown()
    {
        if (ListItems != null)
        {
            outfields.Clear();
            var toggles = GameObject.FindGameObjectsWithTag("ToggleItem");
            foreach (var item in toggles)
            {
                Destroy(item);
            }

            ListItems.Clear();
        }
    }

    private void PopulateOutfieldsDropdown(string Response)
    {
        var jObject = JObject.Parse(Response);
        var jFeatures = jObject.SelectToken("features").ToArray();
        var properties = jFeatures[0].SelectToken("properties");
        //Populate Outfields drop down
        foreach (var outfield in properties)
        {
            if (outfields.Contains("Get All Features"))
            {
                var key = outfield.ToString();
                var props = key.Split(":");
                outfields.Add(props[0]);
                var item = Instantiate(OutfieldItem);
                item.tag = "ToggleItem";
                ListItems.Add(item.GetComponent<Toggle>());
                item.GetComponentInChildren<TextMeshProUGUI>().text = props[0];
                item.transform.SetParent(contentContainer);
                item.transform.localScale = Vector2.one;
            }
            else
            {
                outfields.Add("Get All Features");
                var item = Instantiate(OutfieldItem);
                item.tag = "ToggleItem";
                ListItems.Add(item.GetComponent<Toggle>());
                item.GetComponentInChildren<TextMeshProUGUI>().text = "Get All Features";
                item.transform.SetParent(contentContainer);
                item.transform.localScale = Vector2.one;
            }
        }
    }

    private void MoveCamera()
    {
        if (GetAllFeatures)
        {
            var cameraLocationComponent = arcGISCamera.gameObject.GetComponent<ArcGISLocationComponent>();
            var position = new ArcGISPoint(FeatureItems[0].GetComponent<ArcGISLocationComponent>().Position.X,
                FeatureItems[0].GetComponent<ArcGISLocationComponent>().Position.Y, 20000, cameraLocationComponent.Position.SpatialReference);
            cameraLocationComponent.Position = position;
            cameraLocationComponent.Rotation = new ArcGISRotation(cameraLocationComponent.Rotation.Heading, 0.0,
                cameraLocationComponent.Rotation.Roll);
        }
        else
        {
            var cameraLocationComponent = arcGISCamera.gameObject.GetComponent<ArcGISLocationComponent>();
            var position = new ArcGISPoint(FeatureItems[StartValue].GetComponent<ArcGISLocationComponent>().Position.X,
                FeatureItems[StartValue].GetComponent<ArcGISLocationComponent>().Position.Y, 20000, cameraLocationComponent.Position.SpatialReference);
            cameraLocationComponent.Position = position;
            cameraLocationComponent.Rotation = new ArcGISRotation(cameraLocationComponent.Rotation.Heading, 0.0,
                cameraLocationComponent.Rotation.Roll);
        }
    }
    
    public void SelectItems()
    {
        foreach (var toggle in ListItems)
        {
            var item = toggle.GetComponent<ScrollViewItem>();
            if (GetAllOutfields)
            {
                if (item.Data.enabled && item.Data.name != "Get All Features")
                {
                    item.Data.enabled = false;
                }
            }
            else
            {
                if (item.Data.enabled && item.Data.name == "Get All Features")
                {
                    item.Data.enabled = false;
                }
            }
        }
    }
}