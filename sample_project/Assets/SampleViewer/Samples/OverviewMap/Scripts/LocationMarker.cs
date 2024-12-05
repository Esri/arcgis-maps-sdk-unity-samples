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

public class LocationMarker : MonoBehaviour
{
    [SerializeField] private ArcGISCameraComponent cameraComponent;
    
    private ArcGISCameraControllerComponent cameraController;
    private ArcGISLocationComponent locationComponent;
    private float northOffset = 225.0f;

    private void Awake()
    {
        cameraController = FindFirstObjectByType<ArcGISCameraControllerComponent>();
        locationComponent = GetComponent<ArcGISLocationComponent>();
    }

    private void Update()
    {
        cameraComponent.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(cameraController.GetComponent<ArcGISLocationComponent>().Position.X, cameraController.GetComponent<ArcGISLocationComponent>().Position.Y, cameraComponent.GetComponent<ArcGISLocationComponent>().Position.Z, ArcGISSpatialReference.WGS84()); 
        locationComponent.Position = new ArcGISPoint(cameraController.GetComponent<ArcGISLocationComponent>().Position.X, cameraController.GetComponent<ArcGISLocationComponent>().Position.Y, locationComponent.Position.Z, ArcGISSpatialReference.WGS84());
        locationComponent.Rotation = new ArcGISRotation(cameraController.GetComponent<ArcGISLocationComponent>().Rotation.Heading + northOffset, locationComponent.Rotation.Pitch, locationComponent.Rotation.Roll);    
    }
}