// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using UnityEngine;

public class LocationMarker : MonoBehaviour
{
    private ArcGISGeospatialController cameraController;
    private ArcGISLocationComponent locationComponent;
    private const float northOffset = 225.0f;

    private void Awake()
    {
        cameraController = FindFirstObjectByType<ArcGISGeospatialController>();
        locationComponent = GetComponent<ArcGISLocationComponent>();
    }

    private void Update()
    {
        locationComponent.Position = new ArcGISPoint(cameraController.cameraGeospatialPose.Longitude,
            cameraController.cameraGeospatialPose.Latitude, ArcGISSpatialReference.WGS84());
        locationComponent.Rotation =
            new ArcGISRotation(cameraController.cameraGeospatialPose.EunRotation.eulerAngles.y + northOffset, 180, 0);
    }
}