using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;

// This class issues a query request to a Feature Layer which it then parses to create GameObjects at accurate locations
// with correct property values. This is a good starting point if you are looking to parse your own feature layer into Unity.
public class FeatureLayerQuery : MonoBehaviour
{
    // The feature layer we are going to query
    public string FeatureLayerURL = "https://services.arcgis.com/P3ePLMYs2RVChkJx/ArcGIS/rest/services/Major_League_Baseball_Stadiums/FeatureServer/0";
    
    // This prefab will be instatiated for each feature we parse
    public GameObject StadiumPrefab;

    // The height where we spawn the stadium before finding the ground height
    private int StadiumSpawnHeight = 10000;

    // This will hold a reference to each feature we created
    public List<GameObject> Stadiums = new List<GameObject>();

    // In the query request we can denote the Spatial Reference we want the return geometries in.
    // It is important that we create the GameObjects with the same Spatial Reference
    private int FeatureSRWKID = 4326;

    // This camera reference will be passed to the stadiums to calculate the distance from the camera to each stadium
    public ArcGISCameraComponent ArcGISCamera;

    public Dropdown StadiumSelector;

    // Get all the features when the script starts
    void Start()
    {
        StartCoroutine(GetFeatures());

        StadiumSelector.onValueChanged.AddListener(delegate
        {
            StadiumSelcted();
        });
    }

    // Sends the Request to get features from the service
    private IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        // f=geojson is the output format
        // where=1=1 gets every feature. geometry based or more intelligent where clauses should be used
        //     with larger datasets
        // outSR=4326 gets the return geometries in the SR 4326
        // outFields=LEAGUE,TEAM,NAME specifies the fields we want in the response
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
            "outSR=" +FeatureSRWKID.ToString(),
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
        bool MoreFeatures = true;

        string RemainingResponse = Response;

        string GeometryPrefix = "coordinates\":[";
        string PropertyPrefix = "properties\":{";

        while (MoreFeatures)
        {
            int GeometryIndex = RemainingResponse.IndexOf(GeometryPrefix);
            int PropertyIndex = RemainingResponse.IndexOf(PropertyPrefix, GeometryIndex);
            int NextGeometryIndex = RemainingResponse.IndexOf(GeometryPrefix, PropertyIndex);

            string GeometryInfo = RemainingResponse.Substring(GeometryIndex, PropertyIndex - GeometryIndex);
            string PropertyInfo;
            if (NextGeometryIndex <= 0)
            {
                PropertyInfo = RemainingResponse.Substring(PropertyIndex);
                MoreFeatures = false;
            }
            else
            {
                PropertyInfo = RemainingResponse.Substring(PropertyIndex, NextGeometryIndex - PropertyIndex);
                RemainingResponse = RemainingResponse.Substring(NextGeometryIndex);
            }

            string[] LonLat = GeometryInfo.Substring(GeometryPrefix.Length, GeometryInfo.IndexOf(']') - GeometryPrefix.Length).Split(',');
            double Longitude = double.Parse(LonLat[0]);
            double Latitude = double.Parse(LonLat[1]);

            GeoPosition Position = new GeoPosition(Longitude, Latitude, StadiumSpawnHeight, FeatureSRWKID);

            var NewStadium = Instantiate(StadiumPrefab, this.transform);
            Stadiums.Add(NewStadium);
            NewStadium.SetActive(true);

            var LocationComponent = NewStadium.GetComponent<ArcGISLocationComponent>();
            LocationComponent.enabled = true;
            LocationComponent.Position = Position;

            string[] Properties = PropertyInfo.Substring(PropertyPrefix.Length, PropertyInfo.IndexOf('}') - PropertyPrefix.Length).Split(',');

            var StadiumInfo = NewStadium.GetComponent<StadiumInfo>();
            foreach (string Property in Properties)
            {
                string[] KeyValue = Property.Split(':');
                string Key = KeyValue[0].Replace("\"", "");
                string Value = KeyValue[1].Replace("\"", "");
                StadiumInfo.SetInfo(Value);

                if (Key.Equals("name", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    NewStadium.name = Value;
                }
            }
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
    private void StadiumSelcted()
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
                var NewPosition = StadiumLocation.Position;
                NewPosition.Z = StadiumSpawnHeight;
                CameraLocation.Position = NewPosition;
                CameraLocation.Rotation = StadiumLocation.Rotation;
            }
        }
    }
}
