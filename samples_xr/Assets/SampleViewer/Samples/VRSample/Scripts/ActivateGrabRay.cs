using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit;

public class ActivateGrabRay : MonoBehaviour
{
    
    public bool currentlyTransporting = false;

    [Header("-----------Ray Objects-----------")]
    [SerializeField] private GameObject rightGrabRay;
    [SerializeField] private GameObject leftGrabRay;
    
    [Header("--------Interactor Scripts--------")]
    [SerializeField] private XRDirectInteractor leftDirectGrab;
    [SerializeField] private XRDirectInteractor rightDirectGrab;

    private void Update()
    {
        // Only show the raycast grab lines when the player is not traveling to a new location and there is something in the raycast to interact with
        rightGrabRay.SetActive(!currentlyTransporting && rightDirectGrab.interactablesSelected.Count == 0);
        leftGrabRay.SetActive(!currentlyTransporting && leftDirectGrab.interactablesSelected.Count == 0);
    }
}
