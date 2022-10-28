using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Esri.ArcGISMapsSDK.Samples.Components;

// Disable the ArcGISCameraController when the pointer is over a UI panel
// This prevents unwanted camera movement during UI interactions
public class UICameraDisabler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ArcGISCameraControllerComponent camController;
    void Start()
    {
        camController = FindObjectOfType<ArcGISCameraControllerComponent>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        camController.enabled = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        camController.enabled = true;
    }
}
