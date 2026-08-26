using Esri.ArcGISMapsSDK.Components;
using UnityEngine;

public class AutoEnableCamera : MonoBehaviour
{
    private void Awake()
    {
        var cameraComponent = FindAnyObjectByType<ArcGISCameraComponent>();
        var rebaseComponent = FindAnyObjectByType<ArcGISRebaseComponent>();

        if (cameraComponent != null) cameraComponent.enabled = true;
        if (rebaseComponent != null) rebaseComponent.enabled = true;
    }
}
