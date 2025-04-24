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
    const float northOffset = 225.0f;
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

            if (!rotationToggle.isOn)
            {
                overviewMap.transform.rotation = new Quaternion(0,0,0,0);
            }
        });
    }

    private void Update()
    {
        if (cameraLocationComponent == null || locationComponent == null || mapComponent == null)
        {
            return;
        }

        UpdateLocationAndRotation();

        if (rotationToggle.isOn)
        {
            overviewMap.transform.localEulerAngles = new Vector3(0, 0, (float)cameraLocationComponent.Rotation.Heading);
        }
    }

    private void UpdateLocationAndRotation()
    {
        var cameraPosition = cameraLocationComponent.Position;
        var cameraRotation = cameraLocationComponent.Rotation;

        var newPosition = new ArcGISPoint(cameraPosition.X, cameraPosition.Y, locationComponent.Position.Z, mapComponent.OriginPosition.SpatialReference);
        locationComponent.Position = newPosition;

        var newRotation = new ArcGISRotation(cameraRotation.Heading + northOffset, locationComponent.Rotation.Pitch, locationComponent.Rotation.Roll);
        locationComponent.Rotation = newRotation;

        var cameraComponentLocation = cameraComponent.GetComponent<ArcGISLocationComponent>();

        if (cameraComponentLocation != null)
        {
            cameraComponentLocation.Position = new ArcGISPoint(cameraPosition.X, cameraPosition.Y, cameraComponentLocation.Position.Z, mapComponent.OriginPosition.SpatialReference);
        }
    }
}
