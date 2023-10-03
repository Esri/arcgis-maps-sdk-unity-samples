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
using TMPro;
using UnityEngine.EventSystems;
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
    
    public List<Feature> Features = new List<Feature>();
    public bool GetAllFeatures = true;
    public bool GetAllOutfields = true;
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
    }

    public void CreateLink(string link)
    {
        if (link != null)
        {
            EmptyOutfieldsDropdown();
            foreach (var header in WebLink.RequestHeaders)
            {
                if (!link.Contains(header))
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
            if (NewLink)
            {
                //CreateGameObjectsFromResponse(Request.downloadHandler.text);
                PopulateOutfieldsDropdown(Request.downloadHandler.text);
            }
            else
            {
                CreateGameObjectsFromResponse(Request.downloadHandler.text);
            }

            NewLink = false;
        }
    }
    
    private void CreateGameObjectsFromResponse(string Response)
    {
        // Deserialize the JSON response from the query.
            var jObject = JObject.Parse(Response);
            var jFeatures = jObject.SelectToken("features").ToArray();
            if (GetAllFeatures)
            {
                foreach (var feature in jFeatures)
                {
                    Feature currentFeature = new Feature();
                    var featureItem = Instantiate(featurePrefab, this.transform);
                    featureItem.tag = "FeatureItem";
                    var featureInfo = featureItem.GetComponent<FeatureData>();
                    var LocationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
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

                    if (feature.SelectToken("geometry").SelectToken("type").ToString().ToLower() == "point")
                    {
                        foreach (var coordinate in coordinates)
                        {
                            currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                            featureInfo.coordinates.Add(Convert.ToDouble(coordinate));
                        }
                    }

                    featureInfo.ArcGISCamera = arcGISCamera;
                    ArcGISPoint Position = new ArcGISPoint(featureInfo.coordinates[0], featureInfo.coordinates[1],
                        stadiumSpawnHeight, new ArcGISSpatialReference(featureSRWKID));
                    ArcGISRotation Rotation = new ArcGISRotation(0.0, 90.0, 0.0);
                    LocationComponent.enabled = true;
                    LocationComponent.Position = Position;
                    LocationComponent.Rotation = Rotation;
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
                    var featureInfo = featureItem.GetComponent<FeatureData>();
                    var LocationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
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

                    if (jFeatures[i].SelectToken("geometry").SelectToken("type").ToString().ToLower() == "point")
                    {
                        foreach (var coordinate in coordinates)
                        {
                            currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                            featureInfo.coordinates.Add(Convert.ToDouble(coordinate));
                        }
                    }

                    featureInfo.ArcGISCamera = arcGISCamera;
                    ArcGISPoint Position = new ArcGISPoint(featureInfo.coordinates[0], featureInfo.coordinates[1],
                        stadiumSpawnHeight, new ArcGISSpatialReference(featureSRWKID));
                    ArcGISRotation Rotation = new ArcGISRotation(0.0, 90.0, 0.0);
                    LocationComponent.enabled = true;
                    LocationComponent.Position = Position;
                    LocationComponent.Rotation = Rotation;
                    Features.Add(currentFeature);
                    FeatureItems.Add(featureItem);
                }
            }
    }

    public void EmptyOutfieldsDropdown()
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
    
    public void PopulateOutfieldsDropdown(string Response)
    {
        var jObject = JObject.Parse(Response);
        var jFeatures = jObject.SelectToken("features").ToArray();
        var properties = jFeatures[0].SelectToken("properties");
        //Populate Stadium name drop down
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
                // do something with the instantiated item -- for instance
                item.GetComponentInChildren<TextMeshProUGUI>().text = props[0];
                //item.GetComponent<Image>().color = i % 2 == 0 ? Color.yellow : Color.cyan;
                //parent the item to the content container
                item.transform.SetParent(contentContainer);
                //reset the item's scale -- this can get munged with UI prefabs
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

    public void SelectItems()
    {
        foreach (var toggle in ListItems)
        {
            var item = toggle.GetComponent<ScrollViewItem>();
            if (GetAllOutfields)
            {
                if (item.data.enabled && item.data.name != "Get All Features")
                {
                    item.data.enabled = false;
                }
            }
            else
            {
                if (item.data.enabled && item.data.name == "Get All Features")
                {
                    item.data.enabled = false;
                }
            }
        }
    }
}
