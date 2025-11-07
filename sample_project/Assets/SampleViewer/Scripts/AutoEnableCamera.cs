using Esri.ArcGISMapsSDK.Components;
using UnityEngine;

public class AutoEnableCamera : MonoBehaviour
{
    private void Awake()
    {
        var cameraComponent = FindObjectOfType<ArcGISCameraComponent>();
        var rebaseComponent = FindObjectOfType<ArcGISRebaseComponent>();

        if (cameraComponent != null) cameraComponent.enabled = true;
        if (rebaseComponent != null) rebaseComponent.enabled = true;
    }
}
