// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;

public class ViewshedMenu : MonoBehaviour
{
    [SerializeField] private GameObject viewshedCamera;

    [SerializeField] private TMP_InputField longitudeInputField;
    [SerializeField] private TMP_InputField latitudeInputField;
    [SerializeField] private TMP_InputField altitudeInputField;

    [SerializeField] private Button updateLocationButton;

    [SerializeField] private Slider headingSlider;
    [SerializeField] private Slider pitchSlider;
    [SerializeField] private Slider heightSlider;

    [SerializeField] private TMP_Text headingCounter;
    [SerializeField] private TMP_Text pitchCounter;
    [SerializeField] private TMP_Text heightCounter;

    private ArcGISLocationComponent locationComponent;

    private void Start()
    {
        if (viewshedCamera == null)
        {
            Debug.LogWarning("Viewshed Camera not set in Viewshed Menu");
            return;
        }

        locationComponent = viewshedCamera.GetComponent<ArcGISLocationComponent>();

        if (locationComponent == null)
        {
            Debug.LogWarning("ArcGISLocationComponent not found on Viewshed Camera");
            return;
        }

        if (longitudeInputField == null || latitudeInputField == null || altitudeInputField == null ||
        headingSlider == null || pitchSlider == null || heightSlider == null ||
        headingCounter == null || pitchCounter == null || heightCounter == null)
        {
            Debug.LogWarning("One or more UI components are not set in the inspector");
            return;
        }

        longitudeInputField.text = locationComponent.Position.X.ToString();
        latitudeInputField.text = locationComponent.Position.Y.ToString();
        altitudeInputField.text = locationComponent.Position.Z.ToString();

        headingSlider.value = Mathf.RoundToInt((float)locationComponent.Rotation.Heading);
        pitchSlider.value = Mathf.RoundToInt((float)locationComponent.Rotation.Pitch);
        heightSlider.value = Mathf.RoundToInt((float)locationComponent.Rotation.Roll);

        headingCounter.text = Mathf.RoundToInt(headingSlider.value).ToString();
        pitchCounter.text = Mathf.RoundToInt(pitchSlider.value).ToString();
        heightCounter.text = Mathf.RoundToInt(heightSlider.value).ToString();
    }

    public void UpdateLocation()
    {
        if (double.TryParse(longitudeInputField.text, out double longitude) &&
            double.TryParse(latitudeInputField.text, out double latitude) &&
            double.TryParse(altitudeInputField.text, out double altitude))
        {
            locationComponent.Position = new ArcGISPoint(longitude, latitude, altitude, locationComponent.Position.SpatialReference);
        }
        else
        {
            Debug.LogWarning("Invalid input for location coordinates");
        }
    }

    public void UpdateHeading(float value)
    {
        locationComponent.Rotation = new ArcGISRotation(value, locationComponent.Rotation.Pitch, locationComponent.Rotation.Roll);
        UpdateCounter(headingSlider, headingCounter);
    }

    public void UpdatePitch(float value)
    {
        locationComponent.Rotation = new ArcGISRotation(locationComponent.Rotation.Heading, value, locationComponent.Rotation.Roll);
        UpdateCounter(pitchSlider, pitchCounter);
    }

    public void UpdateHeight(float value)
    {
        locationComponent.Rotation = new ArcGISRotation(locationComponent.Rotation.Heading, locationComponent.Rotation.Pitch, value);
        UpdateCounter(heightSlider, heightCounter);
    }

    private void UpdateCounter(Slider slider, TMP_Text counter)
    {
        counter.text = Mathf.RoundToInt(slider.value).ToString();
    }
}
