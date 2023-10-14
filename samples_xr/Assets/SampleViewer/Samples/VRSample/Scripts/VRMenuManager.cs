using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private Transform VRhead;
    [Min(0)] [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private InputActionProperty toggleMenuButton;

    private bool currentlyTeleporting = false;
    private GameObject esriLogo;
    private GameObject esriMenu;

    private void Start()
    {
        // Cache member variables
        esriMenu = GameObject.FindWithTag("VRCanvas");
        esriLogo = GameObject.FindWithTag("EsriLogoCanvas");

        // Inset logo after delay in order to get correct XROrigin location reference
        Invoke("InsertLogo", 0.4f);
    }

    private void Update()
    {
        if (esriMenu)
        {
            if (toggleMenuButton.action.WasPressedThisFrame() && !currentlyTeleporting)
            {
                esriMenu.SetActive(!esriMenu.activeSelf);

                esriMenu.transform.position = VRhead.position + new Vector3(VRhead.forward.x, 0, VRhead.forward.z).normalized * spawnDistance;
            }

            esriMenu.transform.LookAt(new Vector3(VRhead.position.x, esriMenu.transform.position.y, VRhead.position.z));
            esriMenu.transform.forward *= -1;
        }

    }

    private void InsertLogo()
    {
        if (esriLogo)
        {
            esriLogo.SetActive(true);
            esriLogo.transform.position = VRhead.position + new Vector3(VRhead.forward.x, 0, VRhead.forward.z).normalized * 300;
            esriLogo.transform.position += new Vector3(0, 100, 0);
            esriLogo.transform.LookAt(new Vector3(VRhead.position.x, esriLogo.transform.position.y, VRhead.position.z));
            esriLogo.transform.forward *= -1;
        }
    }

    public void SetCurrentlyTeleporting(bool isCurrentlyTeleporting)
    {
        currentlyTeleporting = isCurrentlyTeleporting;
    }
}
