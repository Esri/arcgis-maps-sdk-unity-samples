using Esri.ArcGISMapsSDK.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static UnityEngine.Rendering.DebugUI;

public class SetWeatherType : MonoBehaviour
{
    [SerializeField] WeatherSystem weatherSystem;
    private void Start()
    {
        // Delay on volume cache to give it time to instantiate
        Invoke(nameof(InitializeWeather), 0.5f);
    }

    private void InitializeWeather()
    {
        weatherSystem = FindAnyObjectByType<WeatherSystem>();
        if(weatherSystem)
        {
            SetWeatherTypeFromIndex(0);
        }
    }

    public void SetWeatherTypeFromIndex(int index)
    {
        if (index == 0) //Sunny
        {
            weatherSystem.SetToSunny();
        }
        else if (index == 1) //Cloudy
        {
            weatherSystem.SetToCloudy();
        }
        else if (index == 2) //Rainy
        {
            weatherSystem.SetToRainy();
        }
        else if (index == 3) //Snowy
        {
            weatherSystem.SetToSnowy();
        }
        else if (index == 4) //Thunder
        {
            weatherSystem.SetToThunder();
        }
    }
}
