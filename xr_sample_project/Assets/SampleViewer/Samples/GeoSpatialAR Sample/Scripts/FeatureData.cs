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
    private double scale;

    public ArcGISCameraComponent ArcGISCamera;
    public List<double> Coordinates = new List<double>();
    public ArcGISLocationComponent LocationComponent;
    public List<string> Properties = new List<string>();

    private void Start()
    {
        cameraLocationComponent = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
        LocationComponent = transform.GetComponent<ArcGISLocationComponent>();
        featureHP = transform.GetComponent<HPTransform>();
        LocationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.AbsoluteHeight;
    }
}
