using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using CommonUsages = UnityEngine.XR.CommonUsages;

[RequireComponent(typeof(CharacterController))]
public class ContinuousMovement : MonoBehaviour
{
    public XRNode LeftInputSource;
    public XRNode RightInputSource;
    public InputAction MenuButtonReference;
    public bool UseSmoothTurning = true;
    private ContinuousTurnProviderBase ContinuousTurnProviderBase;
    private Vector2 leftInputAxis;
    private Vector2 rightInputAxis;
    private CharacterController controller;
    [SerializeField] private float speed;
    [SerializeField] private float upSpeed;
    private XROrigin rig;
    private float fallSpeed;
    private SnapTurnProviderBase SnapTurnProviderBase;
    private float heightOffset = 0.2f;

    private void Awake()
    {
        ContinuousTurnProviderBase = GameObject.Find("Locomotion System").GetComponent<ContinuousTurnProviderBase>();
        SnapTurnProviderBase = GameObject.Find("Locomotion System").GetComponent<SnapTurnProviderBase>();
    }

    private void OnEnable()
    {
        MenuButtonReference.Enable();
    }

    private void OnDisable()
    {
        MenuButtonReference.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
        ToggleSmoothTurn();
    }

    private void FixedUpdate()
    {
        FollowHeadset();
        var headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        var direction = headYaw * new Vector3(leftInputAxis.x, 0, leftInputAxis.y);
        var up = new Vector3(0, rightInputAxis.y, 0);
        controller.Move( speed * Time.fixedDeltaTime * direction);
        controller.Move(upSpeed * Time.fixedDeltaTime * up);

    }

    // Update is called once per frame
    void Update()
    {
        var leftDevice = InputDevices.GetDeviceAtXRNode(LeftInputSource);
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftInputAxis);
        var rightDevice = InputDevices.GetDeviceAtXRNode(RightInputSource);
        rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightInputAxis);
        if (MenuButtonReference.triggered)
        {
            Debug.Log("button pressed");
        }
    }

    void FollowHeadset()
    {
        controller.height = rig.CameraInOriginSpaceHeight + heightOffset;
        var capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        controller.center = new Vector3(capsuleCenter.x, controller.height / 2 + controller.skinWidth, capsuleCenter.z);
    }

    private void ToggleSmoothTurn()
    {
        if (UseSmoothTurning)
        {
            ContinuousTurnProviderBase.enabled = true;
            SnapTurnProviderBase.enabled = false;
        }
        else
        {
            ContinuousTurnProviderBase.enabled = false;
            SnapTurnProviderBase.enabled = true;
        }
    }

}
