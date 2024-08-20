using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollItem : MonoBehaviour, IPointerClickHandler
{
    public string currentWeather;
    public string skyCondition;
    public string stationName;
    public int tempurature;
    public double longitude;
    public double latitude;
    public Image weatherImage;

    [HideInInspector] public bool isEnabled;
    private ArcGISMapComponent mapComponent;
    [SerializeField] private GameObject weather;
    private WeatherQuery weatherQuery;

    private void CheckDataValues()
    {
        weatherQuery.SelectItems(this);
        GetComponentInChildren<Toggle>().isOn = isEnabled;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CheckDataValues();
        StartCoroutine(weatherQuery.SendCityQuery(longitude, latitude));
        var weatherDataGameObject = FindObjectOfType<WeatherData>();
        if (!weatherDataGameObject)
        {
            if (weatherQuery.notFound)
            {
                weatherQuery.cityText.text = stationName;
            }

            var weatherSystem = Instantiate(weather);
            weatherSystem.transform.SetParent(mapComponent.transform);
            var weatherData = weatherSystem.GetComponent<WeatherData>();

            weatherData.currentWeather = currentWeather;
            weatherData.skyCondition = skyCondition;
            weatherData.tempurature = tempurature;
            weatherData.longitude = longitude;
            weatherData.latitude = latitude;

            var locationComponent = weatherSystem.GetComponent<ArcGISLocationComponent>();
            locationComponent.Position = new ArcGISPoint(longitude, latitude, 3000.0f, ArcGISSpatialReference.WGS84());
            locationComponent.Rotation = new ArcGISRotation(0, 90, 0);
            locationComponent.enabled = true;
            weatherQuery.LocationText.text = "Lat: " + latitude + ", Long: " + longitude;
            weatherQuery.TempText.text = weatherData.ConvertTemp(tempurature, weatherQuery.TempuratureToggle.isOn).ToString();
            weatherData.SetSky();
            weatherData.SetWeather();
        }
        else
        {
            var weatherData = weatherDataGameObject.GetComponent<WeatherData>();

            weatherData.currentWeather = currentWeather;
            weatherData.skyCondition = skyCondition;
            weatherData.tempurature = tempurature;
            weatherData.longitude = longitude;
            weatherData.latitude = latitude;

            weatherData.MoveCamera();
            var locationComponent = weatherDataGameObject.GetComponent<ArcGISLocationComponent>();
            locationComponent.Position = new ArcGISPoint(longitude, latitude, 3000.0f, ArcGISSpatialReference.WGS84());
            locationComponent.enabled = true;
            weatherQuery.LocationText.text = "Lat: " + latitude + ", Long: " + longitude;
            weatherQuery.TempText.text = weatherData.ConvertTemp(tempurature, weatherQuery.TempuratureToggle.isOn).ToString();
            weatherData.SetSky();
            weatherData.SetWeather();
        }

        SetWeatherIcon();
    }

    private void SetWeatherIcon()
    {
        if (currentWeather.ToLower().Contains("thunder"))
        {
            weatherQuery.WeatherIcon.sprite = weatherQuery.Thunder;
        }
        else if (currentWeather.ToLower().Contains("snow"))
        {
            weatherQuery.WeatherIcon.sprite = weatherQuery.Snow;
        }
        else if (currentWeather.ToLower().Contains("rain"))
        {
            weatherQuery.WeatherIcon.sprite = weatherQuery.Rain;
        }
        else if (skyCondition.ToLower().Contains("cloud"))
        {
            weatherQuery.WeatherIcon.sprite = weatherQuery.Cloudy;
        }
        else
        {
            weatherQuery.WeatherIcon.sprite = weatherQuery.Sunny;
        }
    }

    private void Start()
    {
        weatherQuery = FindObjectOfType<WeatherQuery>();
        mapComponent = FindObjectOfType<ArcGISMapComponent>();
    }
}
