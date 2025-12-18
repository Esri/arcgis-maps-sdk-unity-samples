using Esri.ArcGISMapsSDK.Components;
using UnityEngine;

public class AutoEnableCamera : MonoBehaviour
{
    private void Awake()
    {
        var cameraComponent = FindFirstObjectByType<ArcGISCameraComponent>();
        var rebaseComponent = FindFirstObjectByType<ArcGISRebaseComponent>();

        if (cameraComponent != null) cameraComponent.enabled = true;
        if (rebaseComponent != null) rebaseComponent.enabled = true;
    }
}
