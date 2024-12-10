// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;
using UnityEngine.UI;

public class LocationMarker : MonoBehaviour
{
    [SerializeField] private ArcGISCameraComponent cameraComponent;

    private ArcGISCameraControllerComponent cameraController;
    private ArcGISLocationComponent cameraLocationComponent;
    private ArcGISLocationComponent locationComponent;
    private ArcGISMapComponent mapComponent;
    [SerializeField] private GameObject northMarker;
    private float northOffset = 225.0f;
    [SerializeField] private RawImage overviewMap;
    [SerializeField] private Toggle rotationToggle;

    private void Awake()
    {
        cameraController = FindFirstObjectByType<ArcGISCameraControllerComponent>();
        cameraLocationComponent = cameraController.GetComponent<ArcGISLocationComponent>();
        locationComponent = GetComponent<ArcGISLocationComponent>();
        mapComponent = GetComponentInParent<ArcGISMapComponent>();
        northMarker.SetActive(rotationToggle.isOn);
        
        rotationToggle.onValueChanged.AddListener(delegate
        {
            northMarker.SetActive(rotationToggle.isOn);
        });
    }

    private void Update()
    {
        if (!rotationToggle.isOn)
        {
            cameraComponent.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(
                cameraLocationComponent.Position.X,
                cameraLocationComponent.Position.Y,
                cameraComponent.GetComponent<ArcGISLocationComponent>().Position.Z, mapComponent.OriginPosition.SpatialReference);
            locationComponent.Position = new ArcGISPoint(
                cameraLocationComponent.Position.X,
                cameraLocationComponent.Position.Y, locationComponent.Position.Z,
                mapComponent.OriginPosition.SpatialReference);
            locationComponent.Rotation =
                new ArcGISRotation(cameraLocationComponent.Rotation.Heading + northOffset,
                    locationComponent.Rotation.Pitch, locationComponent.Rotation.Roll);   
        }
        else
        {
            locationComponent.Rotation =
                new ArcGISRotation(cameraLocationComponent.Rotation.Heading + northOffset,
                    locationComponent.Rotation.Pitch, locationComponent.Rotation.Roll);   
            overviewMap.transform.localEulerAngles = new Vector3(0, 0, (float)cameraLocationComponent.Rotation.Heading);
            cameraComponent.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(
                cameraLocationComponent.Position.X,
                cameraLocationComponent.Position.Y,
                cameraComponent.GetComponent<ArcGISLocationComponent>().Position.Z, mapComponent.OriginPosition.SpatialReference);
            locationComponent.Position = new ArcGISPoint(
                cameraLocationComponent.Position.X,
                cameraLocationComponent.Position.Y, locationComponent.Position.Z,
                mapComponent.OriginPosition.SpatialReference);
        }
    }
}