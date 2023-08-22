using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine.Networking;
using UnityEngine;

[System.Serializable]
public struct WebLink
{
    public string Link;
    public string RequestHeaders;
}

[System.Serializable]
public class FeatureCollectionData
{
    public Feature[] features;
}

[System.Serializable]
public class Feature
{
    public Geometry geometry;
    public Properties propertyData;
}

[System.Serializable]
public class Properties
{
    public string[] data;
}

[System.Serializable]
public class Geometry
{
    public double[] coordinates;
}

public class FeatureLayer : MonoBehaviour
{
    [SerializeField] private WebLink webLink;
    [SerializeField] private Properties prop;
    [SerializeField] private GameObject FeaturePrefab;

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
        webLink.Link += webLink.RequestHeaders;
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
        // Deserialize the JSON response from the query.
        var deserialized = JsonUtility.FromJson<FeatureCollectionData>(Response);

        foreach (Feature feature in deserialized.features)
        {
            var NewFeatureObject = Instantiate(FeaturePrefab, this.transform);
            var LocationComponent = NewFeatureObject.GetComponent<ArcGISLocationComponent>();

            double Longitude = feature.geometry.coordinates[0];
            double Latitude = feature.geometry.coordinates[1];
            ArcGISPoint Position = new ArcGISPoint(Longitude, Latitude, 1000.0f, new ArcGISSpatialReference(4326));
            LocationComponent.enabled = true;
            LocationComponent.Position = Position;
            NewFeatureObject.GetComponent<FeatureData>().coordinates.Add(Longitude);
            NewFeatureObject.GetComponent<FeatureData>().coordinates.Add(Latitude);
            
            foreach (var outfield in prop.data)
            {
                //NewFeatureObject.GetComponent<FeatureData>().properties.Add(outFieldData);
            }
            
        }
    }
}
