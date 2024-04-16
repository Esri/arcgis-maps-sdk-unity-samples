using Esri.ArcGISMapsSDK.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static UnityEngine.Rendering.DebugUI;

public class SetWeatherType : MonoBehaviour
{
    private Volume volume;
    private VolumetricClouds volumetricClouds; 
    private GameObject weatherParticles;

    private void Start()
    {
        weatherParticles = GameObject.FindWithTag("WeatherParticles");

        // Delay on volume cache to give it time to instantiate
        Invoke(nameof(GetVolume), 0.5f);
    }

    private void GetVolume()
    {
        volume = FindObjectOfType<Volume>();
        if (volume)
        {
            // Start with sunny weather
            SetWeatherTypeFromIndex(0);
        }
    }

    public void SetWeatherTypeFromIndex(int index)
    {

        weatherParticles = weatherParticles ? weatherParticles : GameObject.FindWithTag("WeatherParticles");
        volume = volume ? volume : FindObjectOfType<Volume>();

        if (index == 0) //Sunny
        {
            weatherParticles.SetActive(false);
            if (volume.profile.TryGet<VolumetricClouds>(out volumetricClouds))
            {
                volumetricClouds.enable.overrideState = true;
                volumetricClouds.enable.value = false;
            }
        }
        else if (index == 1) //Rainy
        {
            weatherParticles.SetActive(true);
            if (volume.profile.TryGet<VolumetricClouds>(out volumetricClouds))
            {
                volumetricClouds.enable.overrideState = true;
                volumetricClouds.enable.value = true;
            }
        }
    }
}
