using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
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
    public Renderer FeatureRender;
    public List<string> Properties = new List<string>();

    private void Start()
    {
        cameraLocationComponent = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
        locationComponent = transform.GetComponent<ArcGISLocationComponent>();
        featureHP = transform.GetComponent<HPTransform>();
        featureHP = transform.GetComponent<HPTransform>();
        locationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.OnTheGround;
        InvokeRepeating("DynamicScale", 2.0f, 0.5f);
    }

    private void DynamicScale()
    {
        //Based on trial and error, it was deduced to use the following number so that the scale is a 
        //nice size based on distance from the camera.
        scale = cameraLocationComponent.Position.Z * 0.00125f;

        if (scale > 0)
        {
            featureHP.LocalScale = new Vector3((float)scale, (float)scale, (float)scale);   
        }
    }
}