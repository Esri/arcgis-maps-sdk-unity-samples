using Esri.ArcGISMapsSDK.Components;
using Esri.HPFramework;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class FeatureData : MonoBehaviour
{
    private ArcGISLocationComponent cameraLocationComponent;
    private float distance;
    private HPTransform featureHP;
    private ArcGISLocationComponent locationComponent;
    private double scale;

    public ArcGISCameraComponent ArcGISCamera;
    public List<double> Coordinates = new List<double>();
    public List<string> Properties = new List<string>();

    private void Start()
    {
        cameraLocationComponent = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
        locationComponent = transform.GetComponent<ArcGISLocationComponent>();
        featureHP = transform.GetComponent<HPTransform>();
        locationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.AbsoluteHeight;
    }
}
