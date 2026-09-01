using Esri.ArcGISMapsSDK.Components;
using UnityEngine;

public class AutoEnableCamera : MonoBehaviour
{
    private void Awake()
    {
        var mapCameraComponent = FindFirstObjectByType<ArcGISCameraComponent>();
        var overviewCameraComponent = GetComponent<ArcGISCameraComponent>();
        var rebaseComponent = FindFirstObjectByType<ArcGISRebaseComponent>();

        if (mapCameraComponent != null) mapCameraComponent.enabled = true;
        if (overviewCameraComponent != null) overviewCameraComponent.enabled = true;
        if (rebaseComponent != null) rebaseComponent.enabled = true;
    }
}
