using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class CityData
{
    public address address = new address();
}

[System.Serializable]
public class address
{
    public string City;
    public string Region;
}

[System.Serializable]
public class FeatureCollectionData
{
    public Feature[] features;
}

[System.Serializable]
public class Feature
{
    public Geometry geometry = new Geometry();
    public WeatherProperties properties = new WeatherProperties();
}

[System.Serializable]
public class WeatherProperties
{
    public string COUNTRY;
    public string SKY_CONDTN;
    public string STATION_NAME;
    public int TEMP;
    public string WEATHER;
}

public class WeatherQuery : MonoBehaviour
{
    [SerializeField] private ArcGISCameraComponent arcGISCamera;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private Animator dropDownAnim;
    [SerializeField] private Toggle dropDownButton;
    private List<Feature> features = new List<Feature>();
    [SerializeField] private GameObject listItem;
    [SerializeField] private GameObject outfieldsList;
    [SerializeField] private TextMeshProUGUI tempTypeText;
    private string webLink = "https://services9.arcgis.com/RHVPKKiFTONKtxq3/ArcGIS/rest/services/NOAA_METAR_current_wind_speed_direction_v1/FeatureServer/0//query?where=COUNTRY+LIKE+%27%25United+States+of+America%27+AND+WEATHER+NOT+IN(%27%2CAutomated+observation+with+no+human+augmentation%3B+there+may+or+may+not+be+significant+weather+present+at+this+time.%27)&outFields=*&f=pgeojson&orderByFields=STATION_NAME";

    public TextMeshProUGUI cityText;
    [HideInInspector] public List<Toggle> ListItems = new List<Toggle>();
    public TextMeshProUGUI LocationText;
    public bool notFound;
    public Sprite Cloudy;
    public Sprite Rain;
    public Sprite Snow;
    public Sprite Sunny;
    public Sprite Thunder;
    public Toggle TempuratureToggle;
    public TextMeshProUGUI TempText;
    public Image WeatherIcon;

    private void CreateGameObjectsFromResponse(string Response)
    {
        // Deserialize the JSON response from the query.
        var deserialized = JsonUtility.FromJson<FeatureCollectionData>(Response);

        foreach (Feature feature in deserialized.features)
        {
            Feature currentFeature = new Feature();
            var roundedLong = Mathf.Round((float)feature.geometry.coordinates[0] * 100) / 100;
            var roundedLat = Mathf.Round((float)feature.geometry.coordinates[1] * 100) / 100;
            currentFeature.geometry.coordinates.Add(roundedLong);
            currentFeature.geometry.coordinates.Add(roundedLat);
            currentFeature.properties.COUNTRY = feature.properties.COUNTRY;
            currentFeature.properties.SKY_CONDTN = feature.properties.SKY_CONDTN;
            currentFeature.properties.STATION_NAME = feature.properties.STATION_NAME;
            currentFeature.properties.TEMP = feature.properties.TEMP;
            currentFeature.properties.WEATHER = feature.properties.WEATHER;
            features.Add(currentFeature);
        }
        PopulateDropDown();
    }

    private IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.html

        UnityWebRequest Request = UnityWebRequest.Get(webLink);
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

    private void PopulateDropDown()
    {
        foreach (Feature feature in features)
        {
            var name = feature.properties.STATION_NAME.Split(":");
            var item = Instantiate(listItem);
            item.GetComponent<Toggle>();
            var scrollItem = item.GetComponent<ScrollItem>();
            scrollItem.currentWeather = feature.properties.WEATHER;
            scrollItem.skyCondition = feature.properties.SKY_CONDTN;
            scrollItem.stationName = name.ToString() + feature.properties.COUNTRY;
            scrollItem.tempurature = feature.properties.TEMP;
            scrollItem.longitude = Mathf.Round((float)feature.geometry.coordinates[0] * 100) / 100;
            scrollItem.latitude = Mathf.Round((float)feature.geometry.coordinates[1] * 100) / 100;
            item.GetComponentInChildren<TextMeshProUGUI>().text = name[0] + ", USA";
            item.transform.SetParent(contentContainer);
            item.transform.localScale = Vector2.one;
            ListItems.Add(item.GetComponent<Toggle>());
            scrollItem.isEnabled = false;

            if (feature.properties.WEATHER.ToLower().Contains("thunder"))
            {
                scrollItem.weatherImage.sprite = Thunder;
            }
            else if (feature.properties.WEATHER.ToLower().Contains("snow"))
            {
                scrollItem.weatherImage.sprite = Snow;
            }
            else if (feature.properties.WEATHER.ToLower().Contains("rain"))
            {
                scrollItem.weatherImage.sprite = Rain;
            }
            else if (feature.properties.SKY_CONDTN.ToLower().Contains("cloud"))
            {
                scrollItem.weatherImage.sprite = Cloudy;
            }
            else
            {
                scrollItem.weatherImage.sprite = Sunny;
            }
        }
    }

    public void SelectItems(ScrollItem ScrollItem)
    {
        foreach (var toggle in ListItems)
        {
            toggle.isOn = false;
        }

        ScrollItem.isEnabled = true;
    }

    public IEnumerator SendCityQuery(double x, double y)
    {
        string APIToken;
        string query;
        ArcGISMapComponent mapComponent = GameObject.FindObjectOfType<ArcGISMapComponent>();

        if (mapComponent.APIKey != "")
        {
            APIToken = mapComponent.APIKey;
        }
        else
        {
            APIToken = ArcGISProjectSettingsAsset.Instance.APIKey;
        }

        string url = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode";
        string coordinates = x.ToString() + "," + y.ToString();
        query = url + "/?f=json&token=" + APIToken + "&location=" + coordinates;
        UnityWebRequest Request = UnityWebRequest.Get(query);
        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(Request.error);
        }
        else
        {
            var deserialized = JsonUtility.FromJson<CityData>(Request.downloadHandler.text);

            if (deserialized.address.City != "")
            { 
                cityText.text = deserialized.address.City + ", " + deserialized.address.Region; 
                notFound = false;
            }
            else
            {
                notFound = true;
            }
        }
    }

    public void SetTempType()
    {
        if (TempuratureToggle.isOn)
        {
            tempTypeText.text = "C";
        }
        else
        {
            tempTypeText.text = "F";
        }
    }

    private void Start()
    {
        StartCoroutine(GetFeatures());

        dropDownButton.onValueChanged.AddListener(delegate (bool value)
        {
            outfieldsList.SetActive(value);
            if (!value)
            {
                dropDownAnim.Play("ReverseDropDownArrow");
            }
            else
            {
                dropDownAnim.Play("DropDownArrow");
            }
        });

    }
}