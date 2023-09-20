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
    public WebLink webLink;
    [SerializeField] private GameObject FeaturePrefab;
    [SerializeField] private List<Feature> features = new List<Feature>();
    [SerializeField] private ArcGISCameraComponent ArcGISCamera;

    public bool GetAllOutfields;
    public bool GetAllFeatures = true;
    public int StartValue;
    public int LastValue = 1;
    public GameObject outfieldItem;
    public Toggle outfieldItemToggle;
    public Transform contentContainer;
    [SerializeField] private List<string> outfields = new List<string>();
    private int StadiumSpawnHeight = 10000;
    private int FeatureSRWKID = 4326;
    public List<string> outfieldsToGet = new List<string>();
    public List<Toggle> listItems = new List<Toggle>();

    // Start is called before the first frame update
    private void Start()
    {
        CreateLink(webLink.Link);
        StartCoroutine(GetFeatures());
    }

    public void CreateLink(string link)
    {
        if (link != null)
        {
            var requestHeader = "";
            foreach (var header in webLink.RequestHeaders)
            {
                if (!requestHeader.Contains(header))
                {
                    requestHeader += header;   
                }
            }
            link += requestHeader;
            webLink.Link = link;   
        }
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
            PopulateStadiumDropdown(Request.downloadHandler.text);
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
                    var featureItem = Instantiate(FeaturePrefab, this.transform);
                    var featureInfo = featureItem.GetComponent<FeatureData>();
                    var LocationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
                    var coordinates = feature.SelectToken("geometry").SelectToken("coordinates").ToArray();
                    var properties = feature.SelectToken("properties");

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
                        foreach (var value in outfieldsToGet)
                        {
                            var key = value;
                            var props = key.Split(":");
                            currentFeature.properties.propertyNames.Add(props[0]);
                            currentFeature.properties.data.Add(props[1]);
                            featureInfo.properties.Add(props[1]);
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
            else
            {
                for (int i = StartValue; i <= LastValue; i++)
                {
                    Feature currentFeature = new Feature();
                    var featureItem = Instantiate(FeaturePrefab, this.transform);
                    var featureInfo = featureItem.GetComponent<FeatureData>();
                    var LocationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
                    var coordinates = jFeatures[i].SelectToken("geometry").SelectToken("coordinates").ToArray();
                    var properties = jFeatures[i].SelectToken("properties");

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
                        foreach (var value in outfieldsToGet)
                        {
                            var key = value;
                            var props = key.Split(":");
                            currentFeature.properties.propertyNames.Add(props[0]);
                            currentFeature.properties.data.Add(props[1]);
                            featureInfo.properties.Add(props[1]);
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
    
    private void PopulateStadiumDropdown(string Response)
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
                var item = Instantiate(outfieldItem);
                listItems.Add(item.GetComponent<Toggle>());
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
                var item = Instantiate(outfieldItem);
                listItems.Add(item.GetComponent<Toggle>());
                item.GetComponentInChildren<TextMeshProUGUI>().text = "Get All Features";
                item.transform.SetParent(contentContainer);
                item.transform.localScale = Vector2.one;
            }
        }
    }

    public void SelectItems()
    {
        foreach (var toggle in listItems)
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
