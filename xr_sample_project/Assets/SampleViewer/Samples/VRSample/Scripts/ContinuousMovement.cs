using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class ContinuousMovement : MonoBehaviour
{
    [Header("----------Input Sources----------")]
    [SerializeField] private XRNode leftInputSource;
    [SerializeField] private XRNode rightInputSource;
    [SerializeField] private InputActionProperty increaseSpeedAction;

    private CharacterController controller;
    private Vector2 leftInputAxis;
    private Vector2 rightInputAxis;

    [Header("----------Movement Variables----------")]
    [Min(0)] [SerializeField] private float speed;
    [Min(0)] [SerializeField] private float speedAccelerator = 0.2f;
    [Min(0)] [SerializeField] private float speedMultiplier = 2f;
    [Min(0)] [SerializeField] private float upSpeed;

    private float fallSpeed;
    private bool stillGoingInSameDirection = false;
    private float finalSpeed;

    [Header("-------------Other-------------")]
    [SerializeField] private LayerMask groundLayer;

    private float heightOffset = 0.2f;
    private GameObject menu;
    private XROrigin rig;

    private void Start()
    {
        // Cache component references
        controller = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
        menu = GameObject.FindWithTag("VRCanvas");
    }

    private void FixedUpdate()
    {

        FollowHeadset();

        // Calcuate move vectors based on input actions
        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(leftInputAxis.x, 0, leftInputAxis.y);
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

    private void Update()
    {
        // Obtain input values from input devices
        UnityEngine.XR.InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftInputAxis);
        UnityEngine.XR.InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        rightDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out rightInputAxis);
    }

    private void FollowHeadset()
    {
        // Set the character to follow the headset
        controller.height = rig.CameraInOriginSpaceHeight + heightOffset;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        controller.center = new Vector3(capsuleCenter.x, controller.height / 2 + controller.skinWidth, capsuleCenter.z);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetVerticalSpeed(float newSpeed)
    {
        upSpeed = newSpeed;
    }
}
