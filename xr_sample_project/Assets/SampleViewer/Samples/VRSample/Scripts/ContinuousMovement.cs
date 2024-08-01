// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class ContinuousMovement : MonoBehaviour
{
    private CharacterController controller;

    private float fallSpeed;

    private float finalSpeed;

    [Header("-------------Other-------------")]
    [SerializeField] private LayerMask groundLayer;

    private float heightOffset = 0.2f;

    [SerializeField] private InputActionProperty increaseSpeedAction;

    private Vector2 leftInputAxis;

    [Header("----------Input Sources----------")]
    [SerializeField] private XRNode leftInputSource;

    private GameObject menu;
    [SerializeField] private bool moveInLookDirection = true;
    private XROrigin rig;
    [SerializeField] private ActionBasedController rightController;
    private Vector2 rightInputAxis;
    [SerializeField] private XRNode rightInputSource;

    [Header("----------Movement Variables----------")]
    [Min(0)][SerializeField] private float speed;

    [Min(0)][SerializeField] private float speedAccelerator = 0.2f;
    [Min(0)][SerializeField] private float speedMultiplier = 2f;
    private bool stillGoingInSameDirection = false;
    [Min(0)][SerializeField] private float upSpeed;

    public void SetMoveInLookDirection(bool state)
    {
        moveInLookDirection = state;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetVerticalSpeed(float newSpeed)
    {
        upSpeed = newSpeed;
    }

    private void FixedUpdate()
    {
        FollowHeadset();

        var yawYValue = moveInLookDirection ? rig.Camera.transform.eulerAngles.y : rightController.transform.eulerAngles.y;
        var yaw = Quaternion.Euler(0, yawYValue, 0);
        var direction = yaw * new Vector3(leftInputAxis.x, 0, leftInputAxis.y);

        // Calcuate move vectors based on input actions
        Vector3 up = new Vector3(0, rightInputAxis.y, 0);

        // Calculate speed based on trigger hold
        if (increaseSpeedAction.action.ReadValue<float>() > 0.5f)
        {
            // If player is no longer holding down trigger
            if (!stillGoingInSameDirection)
            {
                finalSpeed = speed * speedMultiplier;
            }

            // If player is still moving in a direction
            if (direction != Vector3.zero)
            {
                finalSpeed += speedAccelerator;
                stillGoingInSameDirection = true;
            }
            else // Player has stopped moving, reset speed
            {
                finalSpeed = speed * speedMultiplier;
                stillGoingInSameDirection = false;
            }
        }
        else // Player is not holding down trigger, default speed
        {
            finalSpeed = speed;
            stillGoingInSameDirection = false;
        }

        // Allow the player to move only when the menu UI is not up
        if (!menu.activeSelf)
        {
            controller.Move(direction * finalSpeed * Time.fixedDeltaTime);
            controller.Move(up * upSpeed * Time.fixedDeltaTime);
        }
    }

    private void FollowHeadset()
    {
        // Set the character to follow the headset
        controller.height = rig.CameraInOriginSpaceHeight + heightOffset;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        controller.center = new Vector3(capsuleCenter.x, controller.height / 2 + controller.skinWidth, capsuleCenter.z);
    }

    private void Start()
    {
        // Cache component references
        controller = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
        menu = GameObject.FindWithTag("VRCanvas");
    }

    private void Update()
    {
        // Obtain input values from input devices
        UnityEngine.XR.InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftInputAxis);
        UnityEngine.XR.InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        rightDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out rightInputAxis);
    }
}