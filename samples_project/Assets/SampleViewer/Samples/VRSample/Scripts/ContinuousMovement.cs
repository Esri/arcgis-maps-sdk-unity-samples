using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterController))]
public class ContinuousMovement : MonoBehaviour
{
    public XRNode leftInputSource;
    public XRNode rightInputSource;
    private Vector2 leftInputAxis;
    private Vector2 rightInputAxis;
    private CharacterController controller;
    [SerializeField] private float speed;
    [SerializeField] private float upSpeed;
    public bool useSmoothTurn = true;
    private ContinuousTurnProviderBase continuousTurnProvider;
    private SnapTurnProviderBase snapTurnProvider;
    private XROrigin rig;
    public LayerMask groundLayer;
    private float fallSpeed;
    private float heightOffset = 0.2f;

    private void Awake()
    {
        continuousTurnProvider = GameObject.Find("Locomotion System").GetComponent<ContinuousTurnProviderBase>();
        snapTurnProvider = GameObject.Find("Locomotion System").GetComponent<SnapTurnProviderBase>();
    }
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
        ToggleSnapTurn();
    }
    private void FixedUpdate()
    {
        FollowHeadset();
        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(leftInputAxis.x, 0, leftInputAxis.y);
        Vector3 up = new Vector3(0, rightInputAxis.y, 0);
        controller.Move(direction * speed * Time.fixedDeltaTime);
        controller.Move(up * upSpeed * Time.fixedDeltaTime);

    }
    // Update is called once per frame
    void Update()
    {
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftInputAxis);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightInputAxis);
    }
    void FollowHeadset()
    {
        controller.height = rig.CameraInOriginSpaceHeight + heightOffset;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        controller.center = new Vector3(capsuleCenter.x, controller.height / 2 + controller.skinWidth, capsuleCenter.z);
    }
    void ToggleSnapTurn()
    {
        if (useSmoothTurn)
        {
            snapTurnProvider.enabled = false;
            continuousTurnProvider.enabled = true;
        }
        else
        {
            snapTurnProvider.enabled = true;
            continuousTurnProvider.enabled = false;
        }
    }
}
