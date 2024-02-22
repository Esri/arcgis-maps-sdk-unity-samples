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
    public Animation anim;
    [SerializeField] private ArcGISMapComponent arcGISMapComponent;
    [SerializeField] private Camera camera;
    private Vector3 dragStartPoint = Vector3.zero;
    private double4x4 dragStartWorldMatrix;
    [SerializeField] private HPRoot hpRoot;
    private bool isDragging = false;
    [SerializeField] private float radiusScalar = 50f;
    [SerializeField] private ArcGISTabletopControllerComponent tableTop;
    [SerializeField] private GameObject tableTopWrapper;
    private bool paused;

    [Header("Right Hand")]
    [SerializeField] private InputAction pinchR;
    [SerializeField] private XRNode rightInputSource;
    [SerializeField] private XRRayInteractor rightControllerInteractor;
    [SerializeField] private XRRayInteractor rightHandInteractor;
    private Vector2 rightInputAxis;
    private RaycastHit rightHandHit;
    private RaycastHit rightControllerHit;
    private bool triggerPressedR;

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
#if UNITY_EDITOR
        camera.clearFlags = CameraClearFlags.Skybox;
#elif UNITY_STANDALONE_WIN
        camera.clearFlags = CameraClearFlags.Skybox;
#endif
    }

    private void OnEnable()
    {
        pinchR.Enable();;
    }

    private void OnDisable()
    {
        pinchR.Disable();
    }

    // Update is called once per frame
    private void Update()
    {
        tableTop.Width = Mathf.Clamp((float)tableTop.Width, 1000.0f, 4500000.0f);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightInputAxis);
        rightDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressedR);
        rightControllerInteractor.TryGetCurrent3DRaycastHit(out rightControllerHit);
        rightHandInteractor.TryGetCurrent3DRaycastHit(out rightHandHit);

        if (rightControllerHit.collider)
        {
            if (rightControllerHit.collider.name.Contains("ArcGIS"))
            {
                MoveCenter();
            }
        }

        if (rightHandHit.collider)
        {
            if (rightHandHit.collider.name.Contains("ArcGIS"))
            {
                PinchRight();
            }
        }
    }

    private void MoveCenter()
    {
        if (rightInputAxis.y != 0.0f)
        {
            var zoom = Mathf.Sign(rightInputAxis.y);
            ZoomMap(zoom);
        }

        if (triggerPressedR && !isDragging)
        {
            StartPointDragController();
        }
        else if (triggerPressedR && isDragging)
        {
            UpdatePointDragController();
        }
        else if (!triggerPressedR && isDragging)
        {
            EndPointDrag();
        }
    }

    private void PinchRight()
    {
        if (pinchR.IsPressed() && !isDragging)
        {
            StartPointDragHand();
        }
        else if (pinchR.IsPressed() && isDragging)
        {
            UpdatePointDragHand();
        }
        else if (!pinchR.IsPressed() && isDragging)
        {
            EndPointDrag();
        }
    }

    private void StartPointDragController()
    {
        Vector3 dragCurrentPoint;
        var dragStartRay = new Ray(rightControllerInteractor.rayOriginTransform.position, rightControllerInteractor.rayEndPoint - rightControllerInteractor.rayOriginTransform.position);
        tableTop.Raycast(dragStartRay, out dragCurrentPoint);
        isDragging = true;
        dragStartPoint = dragCurrentPoint;
        // Save the matrix to go from Local space to Universe space
        // As the origin location will be changing during drag, we keep the transform we had when the action started
        dragStartWorldMatrix = math.mul(math.inverse(hpRoot.WorldMatrix), tableTop.transform.localToWorldMatrix.ToDouble4x4());
    }

    private void UpdatePointDragController()
    {
        if (isDragging)
        {
            var updateRay = new Ray(rightControllerInteractor.rayOriginTransform.position, rightControllerInteractor.rayEndPoint - rightControllerInteractor.rayOriginTransform.position);

            Vector3 dragCurrentPoint;
            tableTop.Raycast(updateRay, out dragCurrentPoint);

            var diff = dragStartPoint - dragCurrentPoint;
            var newExtentCenterCartesian = dragStartWorldMatrix.HomogeneousTransformPoint(diff.ToDouble3());
            var newExtentCenterGeographic = arcGISMapComponent.View.WorldToGeographic(new double3(newExtentCenterCartesian.x, newExtentCenterCartesian.y, newExtentCenterCartesian.z));

            tableTop.Center = newExtentCenterGeographic;
        }
    }

    private void StartPointDragHand()
    {
        Vector3 dragCurrentPoint;
        var dragStartRay = new Ray(rightHandInteractor.rayOriginTransform.position, rightHandInteractor.rayEndPoint - rightHandInteractor.rayOriginTransform.position);
        tableTop.Raycast(dragStartRay, out dragCurrentPoint);
        isDragging = true;
        dragStartPoint = dragCurrentPoint;
        // Save the matrix to go from Local space to Universe space
        // As the origin location will be changing during drag, we keep the transform we had when the action started
        dragStartWorldMatrix = math.mul(math.inverse(hpRoot.WorldMatrix), tableTop.transform.localToWorldMatrix.ToDouble4x4());
    }

    private void UpdatePointDragHand()
    {
        if (isDragging)
        {
            var updateRay = new Ray(rightHandInteractor.rayOriginTransform.position, rightHandInteractor.rayEndPoint - rightHandInteractor.rayOriginTransform.position);

            Vector3 dragCurrentPoint;
            tableTop.Raycast(updateRay, out dragCurrentPoint);

            var diff = dragStartPoint - dragCurrentPoint;
            var newExtentCenterCartesian = dragStartWorldMatrix.HomogeneousTransformPoint(diff.ToDouble3());
            var newExtentCenterGeographic = arcGISMapComponent.View.WorldToGeographic(new double3(newExtentCenterCartesian.x, newExtentCenterCartesian.y, newExtentCenterCartesian.z));

            tableTop.Center = newExtentCenterGeographic;
        }
    }

    private void EndPointDrag()
    {
        isDragging = false;
    }

    private void ZoomMap(float zoom)
    {
        if (zoom == 0)
        {
            return;
        }

        var speed = tableTop.Width / radiusScalar;
        // More zoom means smaller extent
        tableTop.Width -= zoom * speed;
    }

    public void ZoomInMap()
    {
        if (!isDragging)
        {
            var Speed = tableTop.Width / radiusScalar;
            // More zoom means smaller extent
            tableTop.Width += -1.0 * Speed;
        }
    }

    public void ZoomOutMap()
    {
        if (!isDragging)
        {
            var Speed = tableTop.Width / radiusScalar;
            // More zoom means smaller extent
            tableTop.Width -= -1.0 * Speed;
        }
    }

    public void RotateMapLeft()
    {
        if (!isDragging)
        {
            tableTopWrapper.transform.Rotate(Vector3.up * Time.deltaTime * 45, Space.Self);
        }
    }

    public void RotateMapRight()
    {
        if (!isDragging)
        {
            tableTopWrapper.transform.Rotate(Vector3.up * Time.deltaTime * -45, Space.Self);
        }
    }

    public void PlayAnimation()
    {
        anim.Play();
    }
}
