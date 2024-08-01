// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;
using UnityEngine.InputSystem;

public class Tooltip : MonoBehaviour
{
    private bool activeState = true;
    [SerializeField] private InputAction toggleMenuButton;
    [SerializeField] private CanvasGroup tooltipUI;

    private void EnableTooltip(InputAction.CallbackContext context)
    {
        activeState = !activeState;

        tooltipUI.alpha = activeState ? 1 : 0;
        tooltipUI.interactable = activeState;
        tooltipUI.blocksRaycasts = activeState;
    }

    private void OnDisable()
    {
        toggleMenuButton.performed -= EnableTooltip;

        toggleMenuButton.Disable();
    }

    private void OnEnable()
    {
        toggleMenuButton.Enable();

        toggleMenuButton.performed += EnableTooltip;
    }
}