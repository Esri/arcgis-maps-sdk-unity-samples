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
    [SerializeField] private GameObject mainCamera;
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

    [SerializeField] private Button alignCameraToViewshedButton;
    [SerializeField] private Button alignViewshedToCameraButton;

    private ArcGISLocationComponent viewshedCameraLocationComponent;
    private ArcGISLocationComponent mainCameraLocationComponent;

    private void Start()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not set in Viewshed Menu");
            return;
        }

        if (viewshedCamera == null)
        {
            Debug.LogWarning("Viewshed Camera not set in Viewshed Menu");
            return;
        }

        mainCameraLocationComponent = mainCamera.GetComponent<ArcGISLocationComponent>();

        if (mainCameraLocationComponent == null)
        {
            Debug.LogWarning("ArcGISLocationComponent not found on Main Camera");
            return;
        }

        viewshedCameraLocationComponent = viewshedCamera.GetComponent<ArcGISLocationComponent>();

        if (viewshedCameraLocationComponent == null)
        {
            Debug.LogWarning("ArcGISLocationComponent not found on Viewshed Camera");
            return;
        }

        if (longitudeInputField == null || latitudeInputField == null || altitudeInputField == null ||
        headingSlider == null || pitchSlider == null || heightSlider == null ||
        headingCounter == null || pitchCounter == null || heightCounter == null ||
        updateLocationButton == null || alignCameraToViewshedButton == null || alignViewshedToCameraButton == null)
        {
            Debug.LogWarning("One or more UI components are not set in the inspector");
            return;
        }

        UpdateLocationInputFields();

        UpdateRotationSlidersAndCounters();
    }

    public void UpdateLocation()
    {
        if (double.TryParse(longitudeInputField.text, out double longitude) &&
            double.TryParse(latitudeInputField.text, out double latitude) &&
            double.TryParse(altitudeInputField.text, out double altitude))
        {
            viewshedCameraLocationComponent.Position = new ArcGISPoint(longitude, latitude, altitude, viewshedCameraLocationComponent.Position.SpatialReference);
        }
        else
        {
            Debug.LogWarning("Invalid input for location coordinates");
        }
    }

    private void UpdateLocationInputFields()
    {
        longitudeInputField.text = viewshedCameraLocationComponent.Position.X.ToString();
        latitudeInputField.text = viewshedCameraLocationComponent.Position.Y.ToString();
        altitudeInputField.text = viewshedCameraLocationComponent.Position.Z.ToString();
    }

    public void UpdateHeading(float value)
    {
        viewshedCameraLocationComponent.Rotation = new ArcGISRotation(value, viewshedCameraLocationComponent.Rotation.Pitch, viewshedCameraLocationComponent.Rotation.Roll);
        UpdateCounter(headingSlider, headingCounter);
    }

    public void UpdatePitch(float value)
    {
        viewshedCameraLocationComponent.Rotation = new ArcGISRotation(viewshedCameraLocationComponent.Rotation.Heading, value, viewshedCameraLocationComponent.Rotation.Roll);
        UpdateCounter(pitchSlider, pitchCounter);
    }

    public void UpdateHeight(float value)
    {
        viewshedCameraLocationComponent.Rotation = new ArcGISRotation(viewshedCameraLocationComponent.Rotation.Heading, viewshedCameraLocationComponent.Rotation.Pitch, value);
        UpdateCounter(heightSlider, heightCounter);
    }

    public void AlignCameraToViewshed()
    {
        mainCameraLocationComponent.Position = viewshedCameraLocationComponent.Position;
        mainCameraLocationComponent.Rotation = viewshedCameraLocationComponent.Rotation;
    }

    public void AlignViewshedToCamera()
    {
        viewshedCameraLocationComponent.Position = mainCameraLocationComponent.Position;
        viewshedCameraLocationComponent.Rotation = mainCameraLocationComponent.Rotation;

        UpdateLocationInputFields();
        UpdateRotationSlidersAndCounters();
    }

    private void UpdateCounter(Slider slider, TMP_Text counter)
    {
        counter.text = Mathf.RoundToInt(slider.value).ToString();
    }

    private void UpdateRotationSlidersAndCounters()
    {
        headingSlider.value = Mathf.RoundToInt((float)viewshedCameraLocationComponent.Rotation.Heading);
        pitchSlider.value = Mathf.RoundToInt((float)viewshedCameraLocationComponent.Rotation.Pitch);
        heightSlider.value = Mathf.RoundToInt((float)viewshedCameraLocationComponent.Rotation.Roll);

        headingCounter.text = Mathf.RoundToInt(headingSlider.value).ToString();
        pitchCounter.text = Mathf.RoundToInt(pitchSlider.value).ToString();
        heightCounter.text = Mathf.RoundToInt(heightSlider.value).ToString();
    }
}
