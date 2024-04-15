// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.HPFramework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

public class XRTableTopInteractor : MonoBehaviour
{
    [SerializeField] private Animation anim;
    [SerializeField] private ArcGISMapComponent arcGISMapComponent;
    private Camera camera;
    private Vector3 dragStartPoint = Vector3.zero;
    private double4x4 dragStartWorldMatrix;
    private bool handTrackingEnabled = false;
    [SerializeField] private HPRoot hpRoot;
    private bool isDragging = false;
    private bool rightHanded = true;
    [SerializeField] private float radiusScalar = 50f;
    [SerializeField] private ArcGISTabletopControllerComponent tableTop;
    [SerializeField] private GameObject tableTopWrapper;

    [Header("Material")]
    [SerializeField] private Color color;
    [SerializeField] private Material mat;
    [SerializeField] private bool useTexture;

    [Header("Hand Input")]
    [SerializeField] private XRNode leftInputSource;
    [SerializeField] private XRRayInteractor leftControllerInteractor;
    [SerializeField] private XRRayInteractor leftHandInteractor;
    private Vector2 leftInputAxis;
    private RaycastHit leftHandHit;
    private RaycastHit leftControllerHit;
    [SerializeField] private XRNode rightInputSource;
    [SerializeField] private XRRayInteractor rightControllerInteractor;
    [SerializeField] private XRRayInteractor rightHandInteractor;
    private Vector2 rightInputAxis;
    private RaycastHit rightHandHit;
    private RaycastHit rightControllerHit;
    [SerializeField] private InputActionProperty selectLeft;
    [SerializeField] private InputActionProperty selectRight;

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
        mat.SetColor("_Color", color);
        if (useTexture)
        {
            mat.SetInt("_UseTexture", 1);
        }
        else
        {
            mat.SetInt("_UseTexture", 0);
        }
    }

    public void EndPointDrag()
    {
        isDragging = false;
    }

    public void PlayAnimation()
    {
        anim.Play();
    }

    private void Start()
    {
#if UNITY_EDITOR
        camera.clearFlags = CameraClearFlags.Skybox;
#elif UNITY_STANDALONE_WIN
        camera.clearFlags = CameraClearFlags.Skybox;
#else
        camera.clearFlags = CameraClearFlags.Color;
#endif
    }

    public void StartPointDrag()
    {
        var controllerInteractor = rightHanded ? rightControllerInteractor : leftControllerInteractor;
        var handInteractor = rightHanded ? rightHandInteractor : leftHandInteractor;
        var interactor = handTrackingEnabled ? handInteractor : controllerInteractor;

        Vector3 dragCurrentPoint;
        var dragStartRay = new Ray(interactor.rayOriginTransform.position, interactor.rayEndPoint - interactor.rayOriginTransform.position);
        tableTop.Raycast(dragStartRay, out dragCurrentPoint);
        isDragging = true;
        dragStartPoint = dragCurrentPoint;
        // Save the matrix to go from Local space to Universe space
        // As the origin location will be changing during drag, we keep the transform we had when the action started
        dragStartWorldMatrix = math.mul(math.inverse(hpRoot.WorldMatrix), tableTop.transform.localToWorldMatrix.ToDouble4x4());
    }

    // Update is called once per frame
    private void Update()
    {
        tableTop.Width = Mathf.Clamp((float)tableTop.Width, 0.0f, 4500000.0f);
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftInputAxis);
        leftControllerInteractor.TryGetCurrent3DRaycastHit(out leftControllerHit);
        leftHandInteractor.TryGetCurrent3DRaycastHit(out leftHandHit);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightInputAxis);
        rightControllerInteractor.TryGetCurrent3DRaycastHit(out rightControllerHit);
        rightHandInteractor.TryGetCurrent3DRaycastHit(out rightHandHit);

        var controllerInteractor = rightHanded ? rightControllerInteractor : leftControllerInteractor;
        var handInteractor = rightHanded ? rightHandInteractor : leftHandInteractor;
        var rayInteractor = handTrackingEnabled ? handInteractor : controllerInteractor;

        if (selectLeft.action.triggered)
        {
            rightHanded = false;
        }
        else if (selectRight.action.triggered)
        {
            rightHanded = true;
        }

        if (Mathf.Abs(rightInputAxis.y) > 0.25f)
        {
            var zoom = Mathf.Sign(rightInputAxis.y);
            ZoomMap(zoom);
        }
        else if (Mathf.Abs(leftInputAxis.y) > 0.25f)
        {
            var zoom = Mathf.Sign(leftInputAxis.y);
            ZoomMap(zoom);
        }

        if (isDragging)
        {
            UpdatePointDrag(rayInteractor);
        }
    }

    private void UpdatePointDrag(XRRayInteractor interactor)
    {
        if (isDragging)
        {
            var updateRay = new Ray(interactor.rayOriginTransform.position, interactor.rayEndPoint - interactor.rayOriginTransform.position);

            Vector3 dragCurrentPoint;
            tableTop.Raycast(updateRay, out dragCurrentPoint);

            var diff = dragStartPoint - dragCurrentPoint;
            var newExtentCenterCartesian = dragStartWorldMatrix.HomogeneousTransformPoint(diff.ToDouble3());
            var newExtentCenterGeographic = arcGISMapComponent.View.WorldToGeographic(new double3(newExtentCenterCartesian.x, newExtentCenterCartesian.y, newExtentCenterCartesian.z));

            tableTop.Center = newExtentCenterGeographic;
        }
    }

    public void Toggle()
    {
        handTrackingEnabled = !handTrackingEnabled;
    }

    public void ZoomMap(float zoom)
    {
        if (zoom == 0)
        {
            return;
        }

        var speed = tableTop.Width / radiusScalar;
        // More zoom means smaller extent
        tableTop.Width -= zoom * speed;
    }
}
