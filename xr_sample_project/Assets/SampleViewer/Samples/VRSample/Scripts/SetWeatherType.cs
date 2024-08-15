// Copyright 2024 Esri.
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
        switch (index)
        {
            case 0:
                weatherSystem?.SetToSunny();
                break;

            case 1:
                weatherSystem?.SetToCloudy();
                break;

            case 2:
                weatherSystem?.SetToRainy();
                break;

            case 3:
                weatherSystem?.SetToSnowy();
                break;

            case 4:
                weatherSystem?.SetToThunder();
                break;
        }
    }

    private void InitializeWeather()
    {
        weatherSystem = FindAnyObjectByType<WeatherSystem>();
        SetWeatherTypeFromIndex(0);
    }

    private void Start()
    {
        // Delay on volume cache to give it time to instantiate
        Invoke(nameof(InitializeWeather), 0.5f);
    }
}