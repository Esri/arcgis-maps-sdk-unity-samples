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
    private Vector2 leftInputAxis;
    private CharacterController controller;
    [SerializeField] private float speed = 1f;
    private XROrigin rig;
    public LayerMask groundLayer;
    private float fallSpeed;
    private float gravity = -9.81f;
    private float heightOffset = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
    }
    private void FixedUpdate()
    {
        FollowHeadset();
        Quaternion headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(leftInputAxis.x, 0, leftInputAxis.y);
        controller.Move(direction * speed * Time.deltaTime);
        bool isGrounded = Grounded();
        if (isGrounded)
        {
            fallSpeed = 0;
        }
        else
        {
            fallSpeed += gravity * Time.fixedDeltaTime;
        }
        controller.Move(Vector3.up * fallSpeed * Time.deltaTime);
    }
    // Update is called once per frame
    void Update()
    {
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftInputAxis);
    }
    void FollowHeadset()
    {
        controller.height = rig.CameraInOriginSpaceHeight + heightOffset;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        controller.center = new Vector3(capsuleCenter.x, controller.height / 2 + controller.skinWidth, capsuleCenter.z);
    }
    bool Grounded()
    {
        Vector3 rayStart = transform.TransformPoint(controller.center);
        float rayLength = controller.center.y + 0.01f;
        bool hasHit = Physics.SphereCast(rayStart, controller.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
        return hasHit;
    }
}
