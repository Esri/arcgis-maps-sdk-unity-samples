// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.HPFramework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class FeatureData : MonoBehaviour
{
    private float distance;
    private HPTransform featureHP;
    private ArcGISLocationComponent locationComponent;
    private double scale;

    public List<double> Coordinates = new List<double>();
    public int Index;
    public List<string> Properties = new List<string>();

    private void Start()
    {
        featureHP = transform.GetComponent<HPTransform>();
        featureHP = transform.GetComponent<HPTransform>();
        locationComponent = GetComponent<ArcGISLocationComponent>();
        locationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.OnTheGround;
    }
}
