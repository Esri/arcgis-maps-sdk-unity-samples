using Esri.ArcGISMapsSDK.Components;
using UnityEngine;

public class AutoEnableCamera : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<ArcGISCameraComponent>().enabled = true;
        GetComponent<ArcGISRebaseComponent>().enabled = true;
    }
}
