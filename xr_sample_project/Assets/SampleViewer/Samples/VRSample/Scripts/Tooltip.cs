// Copyright 2022 Esri.
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

    private void EnableTooltip(bool state)
    {
        tooltipUI.alpha = state ? 1 : 0;
        tooltipUI.interactable = state;
        tooltipUI.blocksRaycasts = state;
        activeState = state;
    }

    private void OnDisable()
    {
        toggleMenuButton.Disable();
    }

    private void OnEnable()
    {
        toggleMenuButton.Enable();
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (toggleMenuButton.triggered)
        {
            EnableTooltip(!activeState);
        }
    }
}