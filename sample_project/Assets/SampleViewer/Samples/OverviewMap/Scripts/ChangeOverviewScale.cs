using Esri.HPFramework;
using UnityEngine;

public class ChangeOverviewScale : MonoBehaviour
{
    float CameraSize
    {
        get; 
        set;
    }

    float LocationMarkerScale
    {
        get; 
        set; 
    }

    [SerializeField] private Camera camera;
    [SerializeField] private HPTransform locationMarker;
    [SerializeField] private float scale;
    [SerializeField] private float size;
    
    void Start()
    {
        CameraSize = size;
        LocationMarkerScale = scale;
        camera.orthographicSize = CameraSize;
        locationMarker.LocalScale = new Vector3(LocationMarkerScale, LocationMarkerScale, LocationMarkerScale);
    }
}
