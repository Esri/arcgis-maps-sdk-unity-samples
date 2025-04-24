// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.HPFramework;
using UnityEngine;

public class LampPostItem : FeatureData
{
    // Start is called before the first frame update
    private void Start()
    {
        locationComponent = transform.GetComponent<ArcGISLocationComponent>();
        featureHP = transform.GetComponent<HPTransform>();
        locationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.OnTheGround;
        GetComponentInChildren<Light>().enabled = false;
    }
}
