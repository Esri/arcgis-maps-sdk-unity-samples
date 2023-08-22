// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.GameEngine.Geometry;
using UnityEngine.EventSystems;
using TMPro;

// The follow System.Serializable classes are used to define the REST API response
// in order to leverage Unity's JsonUtility.
// When implementing your own version of this the Baseball Properties would need to 
// be updated.
[System.Serializable]
public class FeatureCollectionData
{
    public string type;
    public Feature[] features;
}

[System.Serializable]
public class Feature
{
    public string type;
    public Geometry geometry;
    public BaseballProperties properties;
}

[System.Serializable]
public class BaseballProperties
{
    public string LEAGUE;
    public string TEAM;
    public string NAME;
}

[System.Serializable]
public class Geometry
{
    public string type;
    public double[] coordinates;
}

// This class issues a query request to a Feature Layer which it then parses to create GameObjects at accurate locations
// with correct property values. This is a good starting point if you are looking to parse your own feature layer into Unity.
public class FeatureLayerQuery : MonoBehaviour
{
    // The feature layer we are going to query
    [SerializeField] private string FeatureLayerURL = "https://services.arcgis.com/P3ePLMYs2RVChkJx/ArcGIS/rest/services/Major_League_Baseball_Stadiums/FeatureServer/0";

    // This prefab will be instatiated for each feature we parse
    [SerializeField] private GameObject StadiumPrefab;

    // The height where we spawn the stadium before finding the ground height
    private int StadiumSpawnHeight = 10000;

    // This will hold a reference to each feature we created
    [SerializeField] private List<GameObject> Stadiums = new List<GameObject>();

    // In the query request we can denote the Spatial Reference we want the return geometries in.
    // It is important that we create the GameObjects with the same Spatial Reference
    private int FeatureSRWKID = 4326;

    // This camera reference will be passed to the stadiums to calculate the distance from the camera to each stadium
    [SerializeField] private ArcGISCameraComponent ArcGISCamera;

    [SerializeField] private TMP_Dropdown StadiumSelector;

    // Get all the features when the script starts
    private void Start()
    {
        StartCoroutine(GetFeatures());
       
        StadiumSelector.onValueChanged.AddListener(delegate
        {
            StadiumSelected();
        });
    }

    private void Update()
    {
        if (MouseOverUI())
        {
            ArcGISCamera.GetComponent<ArcGISCameraControllerComponent>().enabled = false;
        }
        else
        {
            ArcGISCamera.GetComponent<ArcGISCameraControllerComponent>().enabled = true;
        }
    }

    // Sends the Request to get features from the service
    private IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        string QueryRequestURL = FeatureLayerURL + "/Query?" + MakeRequestHeaders();
        UnityWebRequest Request = UnityWebRequest.Get(QueryRequestURL);
        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(Request.error);
        }
        else
        {
            CreateGameObjectsFromResponse(Request.downloadHandler.text);
            PopulateStadiumDropdown();
        }
    }

    // Creates the Request Headers to be used in our HTTP Request
    // f=geojson is the output format
    // where=1=1 gets every feature. geometry based or more intelligent where clauses should be used
    //     with larger datasets
    // outSR=4326 gets the return geometries in the SR 4326
    // outFields=LEAGUE,TEAM,NAME specifies the fields we want in the response
    private string MakeRequestHeaders()
    {
        string[] OutFields =
        {
            "LEAGUE",
            "TEAM",
            "NAME"
        };

        string OutFieldHeader = "outFields=";
        for (int i = 0; i < OutFields.Length; i++)
        {
            OutFieldHeader += OutFields[i];
            
            if(i < OutFields.Length - 1)
            {
                OutFieldHeader += ",";
            }
        }

        string[] RequestHeaders =
        {
            "f=geojson",
            "where=1=1",
            "outSR=" + FeatureSRWKID.ToString(),
            OutFieldHeader
        };

        string ReturnValue = "";
        for (int i = 0; i < RequestHeaders.Length; i++)
        {
            ReturnValue += RequestHeaders[i];

            if (i < RequestHeaders.Length - 1)
            {
                ReturnValue += "&";
            }
        }

        return ReturnValue;
    }

    // Given a valid response from our query request to the feature layer, this method will parse the response text
    // into geometries and properties which it will use to create new GameObjects and locate them correctly in the world.
    // This logic will differ based on the properties you are trying to parse out of the response.
    private void CreateGameObjectsFromResponse(string Response)
    {
        // Deserialize the JSON response from the query.
        var deserialized = JsonUtility.FromJson<FeatureCollectionData>(Response);

        foreach (Feature feature in deserialized.features)
        {
            double Longitude = feature.geometry.coordinates[0];
            double Latitude = feature.geometry.coordinates[1];

            ArcGISPoint Position = new ArcGISPoint(Longitude, Latitude, StadiumSpawnHeight, new ArcGISSpatialReference(FeatureSRWKID));

            var NewStadium = Instantiate(StadiumPrefab, this.transform);
            NewStadium.name = feature.properties.NAME;
            Stadiums.Add(NewStadium);
            NewStadium.SetActive(true);

            var LocationComponent = NewStadium.GetComponent<ArcGISLocationComponent>();
            LocationComponent.enabled = true;
            LocationComponent.Position = Position;

            var StadiumInfo = NewStadium.GetComponent<StadiumInfo>();

            StadiumInfo.SetInfo(feature.properties.NAME);
            StadiumInfo.SetInfo(feature.properties.TEAM);
            StadiumInfo.SetInfo(feature.properties.LEAGUE);

            StadiumInfo.ArcGISCamera = ArcGISCamera;
            StadiumInfo.SetSpawnHeight(StadiumSpawnHeight);
        }
    }

    // Populates the stadium drown down with all the stadium names from the Stadiums list
    private void PopulateStadiumDropdown()
    {
        //Populate Stadium name drop down
        List<string> StadiumNames = new List<string>();
        foreach (GameObject Stadium in Stadiums)
        {
            StadiumNames.Add(Stadium.name);
        }
        StadiumNames.Sort();
        StadiumSelector.AddOptions(StadiumNames);
    }

    // When a new entry is selected in the stadium dropdown move the camera to the new position
    private void StadiumSelected()
    {
        var StadiumName = StadiumSelector.options[StadiumSelector.value].text;
        foreach (GameObject Stadium in Stadiums)
        {
            if(StadiumName == Stadium.name)
            {
                var StadiumLocation = Stadium.GetComponent<ArcGISLocationComponent>();
                if (StadiumLocation == null)
                {
                    return;
                }
                var CameraLocation = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
                double Longitude = StadiumLocation.Position.X;
                double Latitude  = StadiumLocation.Position.Y;

                ArcGISPoint NewPosition = new ArcGISPoint(Longitude, Latitude, StadiumSpawnHeight, StadiumLocation.Position.SpatialReference);

                CameraLocation.Position = NewPosition;
                CameraLocation.Rotation = StadiumLocation.Rotation;
            }
        }
    }

    private bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
