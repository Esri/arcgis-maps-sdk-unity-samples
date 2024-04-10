using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Codice.Client.Common.WebApi.WebApiEndpoints;
using static Codice.CM.Triggers.TriggerRunner;

public class ScrollItem : MonoBehaviour, IPointerClickHandler
{
    public string currentWeather;
    public string skyCondition;
    public int tempurature;
    public double longitude;
    public double latitude;

    public Image weatherImage;

    private ArcGISMapComponent mapComponent;
    [SerializeField] private GameObject weather;
    private WeatherQuery weatherQuery;

    private void Start()
    {
        weatherQuery = FindObjectOfType<WeatherQuery>();
        mapComponent = FindObjectOfType<ArcGISMapComponent>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(weatherQuery.SendCityQuery(longitude, latitude));
        var weatherDataGameObject = FindObjectOfType<WeatherData>();
        if (!weatherDataGameObject)
        {
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
}
