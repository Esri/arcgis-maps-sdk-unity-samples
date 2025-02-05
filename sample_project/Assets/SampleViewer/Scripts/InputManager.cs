// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Samples.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public UnityEvent InputTriggered;
    public UnityEvent InputEnded;
    public TouchControls touchControls;

    private InputActions inputActions;
    private bool isLeftShiftPressed;

    private void Awake()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        inputActions = new InputActions();
#else
        touchControls = new TouchControls();
#endif
    }

    private void OnEnable()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        inputActions.Enable();
        inputActions.DrawingControls.LeftClick.started += OnClick;
        inputActions.DrawingControls.LeftClick.canceled += OnClickEnd;
        inputActions.DrawingControls.LeftShift.performed += OnLeftShiftStarted;
        inputActions.DrawingControls.LeftShift.canceled += OnLeftShiftCanceled;
#else
        touchControls.Enable();
        touchControls.Touch.TouchPress.started += OnTouchStarted;
        touchControls.Touch.TouchPress.canceled += OnTouchEnded;
#endif
    }

    private void OnDisable()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        inputActions.Disable();
        inputActions.DrawingControls.LeftClick.started -= OnClick; 
        inputActions.DrawingControls.LeftClick.canceled -= OnClickEnd;
        inputActions.DrawingControls.LeftShift.performed -= OnLeftShiftStarted;
        inputActions.DrawingControls.LeftShift.canceled -= OnLeftShiftCanceled;
#else
        touchControls.Disable();
        touchControls.Touch.TouchPress.started -= OnTouchStarted;
        touchControls.Touch.TouchPress.canceled -= OnTouchEnded;
#endif
    }

    private void OnClick(InputAction.CallbackContext obj)
    {
        if (isLeftShiftPressed) 
        {
            InputTriggered.Invoke(); 
        }
    }

    private void OnClickEnd(InputAction.CallbackContext obj)
    {
        if (isLeftShiftPressed)
        {
            InputEnded.Invoke();
        }
    }

    private void OnLeftShiftStarted(InputAction.CallbackContext context)
    {
        isLeftShiftPressed = true;
        FindFirstObjectByType<ArcGISCameraControllerComponent>().enabled = false;
    }

    private void OnLeftShiftCanceled(InputAction.CallbackContext context)
    {
        isLeftShiftPressed = false;
        FindFirstObjectByType<ArcGISCameraControllerComponent>().enabled = true;
    }

    private void OnTouchStarted(InputAction.CallbackContext obj)
    {
        InputTriggered.Invoke();
    }

    private void OnTouchEnded(InputAction.CallbackContext obj)
    {
        InputEnded.Invoke();
    }
}
