using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using FeatureLayerData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
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
    private FeatureData featureInfo;
    [SerializeField] private GameObject featurePrefab;
    private int featureSRWKID = 4326;
    private ArcGISLocationComponent locationComponent;
    [SerializeField] private List<string> outfields = new List<string>();
    private float stadiumSpawnHeight = 10000.0f;
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
        EmptyOutfieldsDropdown();

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

    private void CreateGameObjectsFromResponse(string response)
    {
        // Deserialize the JSON response from the query.
        var jObject = JObject.Parse(response);
        jFeatures = jObject.SelectToken("features").ToArray();

        if (jFeatures[0].SelectToken("geometry").SelectToken("type").ToString().ToLower() == "point")
        {
            if (GetAllFeatures)
            {
                CreateFeatures(0, jFeatures.Length);
            }
            else
            {
                if (jFeatures.Length < LastValue)
                {
                    CreateFeatures(StartValue, jFeatures.Length);
                }
                else
                {
                    CreateFeatures(StartValue, LastValue);
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
    
     private void CreateFeatures(int min, int max)
    {
        for (int i = min; i < max; i++)
        {
            Feature currentFeature = new Feature();
            var featureItem = Instantiate(featurePrefab, this.transform);
            //Layer 7 because that is the index of the layer created specifically for feature layers so that they ignore themselves for raycasting.
            featureItem.layer = 7;
            featureInfo = featureItem.GetComponent<FeatureData>();
            locationComponent = featureItem.GetComponent<ArcGISLocationComponent>();
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
                    featureInfo.Properties.Add(key);
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
                        featureInfo.Properties.Add(key);
                    }
                }
            }

            foreach (var coordinate in coordinates)
            {
                currentFeature.geometry.coordinates.Add(Convert.ToDouble(coordinate));
                featureInfo.Coordinates.Add(Convert.ToDouble(coordinate));
            }

            featureInfo.ArcGISCamera = arcGISCamera;
            var position = new ArcGISPoint(featureInfo.Coordinates[0], featureInfo.Coordinates[1],
                stadiumSpawnHeight, new ArcGISSpatialReference(featureSRWKID));
            var rotation = new ArcGISRotation(0.0, 90.0, 0.0);
            locationComponent.enabled = true;
            locationComponent.Position = position;
            locationComponent.Rotation = rotation;
            Features.Add(currentFeature);
            FeatureItems.Add(featureItem);
        }
    }

    private void EmptyOutfieldsDropdown()
    {
        if (ListItems != null)
        {
            outfields.Clear();
            foreach (var item in ListItems)
            {
                Destroy(item.gameObject);
            }

            ListItems.Clear();
        }
    }
    
    private void PopulateOutfieldsDropdown(string response)
    {
        var jObject = JObject.Parse(response);
        var jFeatures = jObject.SelectToken("features").ToArray();
        var properties = jFeatures[0].SelectToken("properties");
        //Populate Outfields drop down

        foreach (var outfield in properties)
        {
            var getAllOutfields = outfields.Contains("Get All Outfields");
            var itemText = getAllOutfields ? outfield.ToString().Split(":")[0] : "Get All Outfields";
            outfields.Add(itemText);
            var item = Instantiate(OutfieldItem);
            ListItems.Add(item.GetComponent<Toggle>());
            item.GetComponentInChildren<TextMeshProUGUI>().text = itemText;
            item.transform.SetParent(contentContainer);
            item.transform.localScale = Vector2.one;
        }
    }

    private void MoveCamera()
    {
        if (FeatureItems.Count == 0)
        {
            return;
        }

        var index = GetAllFeatures ? 0 : StartValue;
        var cameraLocationComponent = arcGISCamera.gameObject.GetComponent<ArcGISLocationComponent>();
        var position = new ArcGISPoint(FeatureItems[index].GetComponent<ArcGISLocationComponent>().Position.X,
            FeatureItems[index].GetComponent<ArcGISLocationComponent>().Position.Y, 10000, cameraLocationComponent.Position.SpatialReference);
        cameraLocationComponent.Position = position;
        cameraLocationComponent.Rotation = new ArcGISRotation(cameraLocationComponent.Rotation.Heading, 0.0,
            cameraLocationComponent.Rotation.Roll);
    }

    public void SelectItems()
    {
        foreach (var toggle in ListItems)
        {
            var item = toggle.GetComponent<ScrollViewItem>();

            if (GetAllOutfields && item.Data.enabled && item.Data.name != "Get All Outfields")
            {
                item.Data.enabled = false;
            }
            else if (!GetAllOutfields && item.Data.enabled && item.Data.name == "Get All Outfields")
            {
                item.Data.enabled = false;
            }
        }
    }
    
    private void Update()
    {
        arcGISCamera.gameObject.GetComponent<ArcGISCameraControllerComponent>().enabled = !MouseOverUI();
    }
    
    private bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}