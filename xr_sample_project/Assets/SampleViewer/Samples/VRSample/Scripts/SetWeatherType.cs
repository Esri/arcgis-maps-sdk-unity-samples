// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

public class SetWeatherType : MonoBehaviour
{
    [SerializeField] private WeatherSystem weatherSystem;

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

    private void InitializeWeather()
    {
        weatherSystem = FindAnyObjectByType<WeatherSystem>();
        if (weatherSystem)
        {
            SetWeatherTypeFromIndex(0);
        }
    }

    private void Start()
    {
        // Delay on volume cache to give it time to instantiate
        Invoke(nameof(InitializeWeather), 0.5f);
    }
}