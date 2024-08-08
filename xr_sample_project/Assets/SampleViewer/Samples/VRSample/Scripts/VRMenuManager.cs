// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Threading;
using UnityEngine;

using UnityEngine.InputSystem;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private bool currentlyTeleporting = false;
    [SerializeField] private CanvasGroup esriLogo;
    [SerializeField] private CanvasGroup esriCanvas;
    [SerializeField] private CanvasGroup esriMenu;
    [SerializeField] private CanvasGroup esriInstructions;
    public bool menuActive { get; private set; } = true;
    [Min(0)][SerializeField] private float spawnDistance = 2f;
    [SerializeField] private InputAction toggleMenuButton;
    [SerializeField] private Transform VRhead;

    public void RealignMenu()
    {
        esriCanvas.transform.position = VRhead.position + new Vector3(VRhead.forward.x, 0, VRhead.forward.z).normalized * spawnDistance;
        esriCanvas.transform.LookAt(new Vector3(VRhead.position.x, esriMenu.transform.position.y, VRhead.position.z));
        esriCanvas.transform.forward *= -1;
    }

    public void SetCurrentlyTeleporting(bool isCurrentlyTeleporting)
    {
        currentlyTeleporting = isCurrentlyTeleporting;
    }

    private void InsertLogo()
    {
        if (esriLogo)
        {
            esriLogo.alpha = 1;
            esriLogo.interactable = true;
            esriLogo.blocksRaycasts = true;

            esriLogo.transform.position = VRhead.position + new Vector3(VRhead.forward.x, 0, VRhead.forward.z).normalized * 300;
            esriLogo.transform.position += new Vector3(0, 100, 0);
            esriLogo.transform.LookAt(new Vector3(VRhead.position.x, esriLogo.transform.position.y, VRhead.position.z));
            esriLogo.transform.forward *= -1;
        }
    }

    public void ToggleMenu(bool state, bool mainMenu = true)
    {
        CanvasGroup group = mainMenu ? esriMenu : esriInstructions;
        group.alpha = state ? 1 : 0;
        group.interactable = state;
        group.blocksRaycasts = state;
        menuActive = esriMenu.interactable || esriInstructions.interactable;

        if (state)
        {
            RealignMenu();
        }
    }

    public void ExitMenus()
    {
        ToggleMenu(false, false);
        ToggleMenu(false);
    }

    public void OpenInstructions()
    {
        ToggleMenu(true, false);
        ToggleMenu(false);
    }

    public void ReturnToMenu()
    {
        ToggleMenu(false, false);
        ToggleMenu(true);
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
        // Inset logo after delay in order to get correct XROrigin location reference
        Invoke("InsertLogo", 0.4f);

        ToggleMenu(true);
    }
    
    private void Update()
{
        if (esriCanvas)
        {
            if (toggleMenuButton.triggered)
            {
                if(esriInstructions.interactable)
                {
                    ExitMenus();
                }
                else
                {
                    ToggleMenu(!menuActive);
                }
            }

            esriCanvas.transform.LookAt(new Vector3(VRhead.position.x, esriMenu.transform.position.y, VRhead.position.z));
            esriCanvas.transform.forward *= -1;
        }
    }
}