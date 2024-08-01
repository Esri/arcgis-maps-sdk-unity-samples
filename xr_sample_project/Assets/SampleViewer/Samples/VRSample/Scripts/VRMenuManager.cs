// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

using UnityEngine.InputSystem;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private bool currentlyTeleporting = false;
    private GameObject esriLogo;
    private GameObject esriMenu;
    [Min(0)][SerializeField] private float spawnDistance = 2f;
    [SerializeField] private InputAction toggleMenuButton;
    [SerializeField] private Transform VRhead;

    public void RealignMenu()
    {
        esriMenu.transform.position = VRhead.position + new Vector3(VRhead.forward.x, 0, VRhead.forward.z).normalized * spawnDistance;
    }

    public void SetCurrentlyTeleporting(bool isCurrentlyTeleporting)
    {
        currentlyTeleporting = isCurrentlyTeleporting;
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

    private void OnDisable()
    {
        toggleMenuButton.Disable();
    }

    private void OnEnable()
    {
        toggleMenuButton.Enable();
    }

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
            if (toggleMenuButton.triggered)
            {
                esriMenu.SetActive(!esriMenu.activeSelf);

                esriMenu.transform.position = VRhead.position + new Vector3(VRhead.forward.x, 0, VRhead.forward.z).normalized * spawnDistance;
            }

            esriMenu.transform.LookAt(new Vector3(VRhead.position.x, esriMenu.transform.position.y, VRhead.position.z));
            esriMenu.transform.forward *= -1;
        }
    }
}