using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Esri.ArcGISMapsSDK.Samples.Components;

// Disable the ArcGISCameraController when the pointer is over a UI panel
// This prevents unwanted camera movement during UI interactions

// Camera controller is disabled when pointer enters UI area, and enabled upon exit
public class UICameraDisabler : MonoBehaviour
{
    private ArcGISCameraControllerComponent camController;
    private int UILayer;
    void Start()
    {
        camController = FindObjectOfType<ArcGISCameraControllerComponent>();
        UILayer = LayerMask.NameToLayer("UI");
    }

    void Update()
    {
        camController.enabled = !IsPointerOverUI();
    }

    bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach(RaycastResult result in results)
        {
            if(result.gameObject.layer == UILayer)
            {
                return true;
            }
        }
        return false;
    }
}
