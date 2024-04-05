using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class WeatherData : MonoBehaviour
{
    private ArcGISLocationComponent cameraLocationComponent;
    public string currentWeather;
    public string skyCondition;
    public int tempurature;
    public double longitude;
    public double latitude;

    [SerializeField] private ArcGISCameraComponent ArcGISCamera;
    [SerializeField] private ArcGISMapComponent ArcGISMap;
    [SerializeField] private Light directionalLight;
    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile volumeProfile;
    private VolumetricClouds vClouds;
    private WeatherQuery weatherQuery;

    private void Start()
    {
        ArcGISMap = FindObjectOfType<ArcGISMapComponent>();
        ArcGISCamera = FindObjectOfType<ArcGISCameraComponent>();
        cameraLocationComponent = ArcGISCamera.gameObject.GetComponent<ArcGISLocationComponent>();
        weatherQuery = FindObjectOfType<WeatherQuery>();
        MoveCamera();

        weatherQuery.TempuratureToggle.onValueChanged.AddListener(delegate (bool value)
        {
            weatherQuery.TempText.text = ConvertTemp(tempurature, value).ToString() + "°";
        });
    }

    public void SetSky()
    {
        directionalLight = FindObjectOfType<Light>();
        volume = FindObjectOfType<Volume>();
        volumeProfile = volume.profile;
        if (skyCondition.ToLower().Contains("overcast"))
        {
            if (volumeProfile.TryGet(out vClouds))
            {
                vClouds.cloudPreset = VolumetricClouds.CloudPresets.Overcast;
                directionalLight.color = new Color(0.1803922f, 0.1803922f, 0.1803922f, 1.0f);
            }
        }
        else if (skyCondition.ToLower().Contains("Cloud"))
        {
            if (volumeProfile.TryGet(out vClouds))
            {
                vClouds.cloudPreset = VolumetricClouds.CloudPresets.Cloudy;
                directionalLight.color = new Color(1, 1, 1, 1);
            }
        }
        else if (currentWeather.ToLower().Contains("thunder"))
        {
            vClouds.cloudPreset = VolumetricClouds.CloudPresets.Stormy;
            directionalLight.color = new Color(1, 1, 1, 1);
        }
        else
        {
            if (volumeProfile.TryGet(out vClouds))
            {
                vClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
                directionalLight.color = new Color(1, 1, 1, 1);
            }
        }
    }

    public void SetWeather()
    {
        if (currentWeather.ToLower().Contains("thunder"))
        {

        }
        else if (currentWeather.ToLower().Contains("snow"))
        {

        }
        else if (currentWeather.ToLower().Contains("rain"))
        {

        }
        else
        {
            vClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
            directionalLight.color = new Color(1, 1, 1, 1);
        }
    }

    public void MoveCamera()
    {
        ArcGISMap.OriginPosition = new ArcGISPoint(longitude, latitude, 0, ArcGISSpatialReference.WGS84());
        cameraLocationComponent.Position = new ArcGISPoint(longitude, latitude, 3000.0f, ArcGISSpatialReference.WGS84());
    }

    public float ConvertTemp(float Temp, bool IsOn)
    {
        float currentTemp;
        if (IsOn)
        {
            // To Celcius
            currentTemp = Mathf.Round((Temp - 32) * 0.55555555555f);
        }
        else
        {
            // To Fahrenheit
            currentTemp = Temp;
        }

        return currentTemp;
    }
}