using Esri.ArcGISMapsSDK.Components;
using Esri.HPFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampPostItem : FeatureData
{
    // Start is called before the first frame update
    void Start()
    {
        locationComponent = transform.GetComponent<ArcGISLocationComponent>();
        featureHP = transform.GetComponent<HPTransform>();
        locationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.OnTheGround;
        GetComponentInChildren<Light>().enabled = false;
    }
}
