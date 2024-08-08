// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    [SerializeField] private InputActionProperty gripAnimationAction;

    private Animator handAnimator;

    [Header("----------Input Actions----------")]
    [SerializeField] private InputActionProperty pinchAnimationAction;

    private void Start()
    {
        handAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Set Animation values from input actions
        handAnimator = handAnimator ? handAnimator : GetComponent<Animator>();

        if (handAnimator)
        {
            handAnimator.SetFloat("Trigger", pinchAnimationAction.action.ReadValue<float>());
            handAnimator.SetFloat("Grip", gripAnimationAction.action.ReadValue<float>());
        }
    }
}