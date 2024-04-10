using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System.Collections;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class WeatherData : MonoBehaviour
{
    public string currentWeather;
    public string skyCondition;
    public int tempurature;
    public double longitude;
    public double latitude;

    [SerializeField] private ArcGISCameraComponent ArcGISCamera;
    [SerializeField] private ArcGISMapComponent ArcGISMap;
    private ArcGISLocationComponent cameraLocationComponent;
    [SerializeField] private Light directionalLight;
    private ArcGISLocationComponent locationComponent;
    public float lightningTimer;
    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile volumeProfile;
    private VolumetricClouds vClouds;
    private WeatherQuery weatherQuery;

    [Header("WeatherVFX")]
    [SerializeField] private GameObject lightning;
    [SerializeField] private GameObject rain;
    [SerializeField] private GameObject snow;

    private void Awake()
    {
        ArcGISMap = FindObjectOfType<ArcGISMapComponent>();
        ArcGISCamera = FindObjectOfType<ArcGISCameraComponent>();
        cameraLocationComponent = ArcGISCamera.gameObject.GetComponent<ArcGISLocationComponent>();
        locationComponent = GetComponent<ArcGISLocationComponent>();
        weatherQuery = FindObjectOfType<WeatherQuery>();
    }

    private void Start()
    {
        MoveCamera();

        weatherQuery.TempuratureToggle.onValueChanged.AddListener(delegate (bool value)
        {
            weatherQuery.TempText.text = ConvertTemp(tempurature, value).ToString() + "°";
        });
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

    private void DisableWeather()
    {
        lightning.SetActive(false);
        rain.SetActive(false);
        snow.SetActive(false);
    }

    private void FollowCamera()
    {
        locationComponent.Position = cameraLocationComponent.Position;
    }

    private void LateUpdate()
    {
        FollowCamera();
    }

    public void MoveCamera()
    {
        ArcGISMap.OriginPosition = new ArcGISPoint(longitude, latitude, 0, ArcGISSpatialReference.WGS84());
        cameraLocationComponent.Position = new ArcGISPoint(longitude, latitude, 3000.0f, ArcGISSpatialReference.WGS84());
    }

    public void SetSky()
    {
        directionalLight = FindObjectOfType<Light>();
        volume = FindObjectOfType<Volume>();
        volumeProfile = volume.profile;

        if (volumeProfile.TryGet(out vClouds))
        {
            if (skyCondition.ToLower().Contains("overcast"))
            {
                vClouds.cloudPreset = VolumetricClouds.CloudPresets.Overcast;
                directionalLight.color = new Color(0.1803922f, 0.1803922f, 0.1803922f, 1.0f);
            }
            else if (skyCondition.ToLower().Contains("Cloud"))
            {
                vClouds.cloudPreset = VolumetricClouds.CloudPresets.Cloudy;
                directionalLight.color = new Color(1, 1, 1, 1);
            }
            else if (currentWeather.ToLower().Contains("thunder"))
            { 
                vClouds.cloudPreset = VolumetricClouds.CloudPresets.Stormy;
                directionalLight.color = new Color(1, 1, 1, 1);
            }
            else
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
            DisableWeather();
            rain.SetActive(true);
            lightning.SetActive(true);
            lightningTimer = Random.Range(2.0f, 30.0f);
            InvokeRepeating("MoveLightning", 0.0f, lightningTimer);
        }
        else if (currentWeather.ToLower().Contains("snow"))
        {
            DisableWeather();
            snow.SetActive(true);
        }
        else if (currentWeather.ToLower().Contains("rain"))
        {
            DisableWeather();
            rain.SetActive(true);
        }
        else
        {
            vClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
            directionalLight.color = new Color(1, 1, 1, 1);
            DisableWeather();
        }
    }

    private void Update()
    {

    }

    public void MoveLightning()
    {
        var randomForward = Camera.main.transform.forward.z * Random.Range(10.0f, 50.0f);
        var randomRight = Camera.main.transform.right.x * Random.Range(-10.0f, 10.0f);
        lightning.transform.position = new Vector3(randomRight, 3000.0f, randomForward);
        var vfx = lightning.GetComponent<VisualEffect>();
        var eventAttribute = vfx.CreateVFXEventAttribute();
        vfx.SendEvent("Lightning", eventAttribute);
        lightningTimer = Random.Range(2.0f, 30.0f);
    }
}