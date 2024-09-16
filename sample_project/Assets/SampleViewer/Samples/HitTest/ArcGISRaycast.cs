// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Linq;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class ArcGISRaycast : MonoBehaviour
{
    [SerializeField] private InputAction inputAction;
    private const float offSet = 200f;

    public ArcGISMapComponent arcGISMapComponent;
    public ArcGISCameraComponent arcGISCamera;
    public Canvas canvas;
    public TextMeshProUGUI featureText;
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
    private InputActions inputActions;
#else
    private TouchControls touchControls;
#endif
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
        inputActions.DrawingControls.LeftClick.started += OnLeftClickStart;
        inputActions.DrawingControls.LeftShift.performed += ctx => OnLeftShift(true);
        inputActions.DrawingControls.LeftShift.canceled += ctx => OnLeftShift(false);
#else
        TouchSimulation.Enable();
        touchControls.Enable();
        touchControls.Touch.TouchPress.started += ctx => OnTouchInputStarted(ctx);
        touchControls.Touch.TouchPress.canceled += ctx => OnTouchInputEnded(ctx);
#endif
    }
    
    private void OnDisable()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        inputActions.Disable();
        inputActions.DrawingControls.LeftClick.started -= OnLeftClickStart;
        inputActions.DrawingControls.LeftShift.performed -= ctx => OnLeftShift(true);
        inputActions.DrawingControls.LeftShift.canceled -= ctx => OnLeftShift(false);
#else
        touchControls.Disable();
#endif
    }

    private void OnLeftShift(bool isPressed)
    {
        isLeftShiftPressed = isPressed;
    }

    private void OnLeftClickStart(InputAction.CallbackContext context)
    {
        if (isLeftShiftPressed)
        {
            if (!canvas.enabled)
            {
                canvas.enabled = true;
            }
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out hit))
            {
                var arcGISRaycastHit = arcGISMapComponent.GetArcGISRaycastHit(hit);
                var layer = arcGISRaycastHit.layer;
                var featureId = arcGISRaycastHit.featureId;

                if (layer != null && featureId != -1)
                {
                    featureText.text = featureId.ToString();

                    var geoPosition = arcGISMapComponent.EngineToGeographic(hit.point);
                    var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + offSet, geoPosition.SpatialReference);

                    var rotation = arcGISCamera.GetComponent<ArcGISLocationComponent>().Rotation;
                    var location = canvas.GetComponent<ArcGISLocationComponent>();
                    location.Position = offsetPosition;
                    location.Rotation = rotation;
                }
            }
        }
    }

    private void OnTouchInputStarted(InputAction.CallbackContext ctx)
    {
        Debug.LogWarning("Touch Started: " + touchControls.Touch.TouchPosition.ReadValue<Vector2>());
    }

    private void OnTouchInputEnded(InputAction.CallbackContext ctx)
    {
        Debug.LogWarning("Touch Ended: " + touchControls.Touch.TouchPosition.ReadValue<Vector2>());
    }
    
    private void Start()
    {
        canvas.enabled = false;
    }
}