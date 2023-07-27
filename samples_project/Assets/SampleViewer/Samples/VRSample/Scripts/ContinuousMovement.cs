using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using CommonUsages = UnityEngine.XR.CommonUsages;

[RequireComponent(typeof(CharacterController))]
public class ContinuousMovement : MonoBehaviour
{
    public XRNode leftInputSource;
    public XRNode rightInputSource;
    public InputAction LeftMenuAction;
    public InputAction RightMenuAction;
    public GameObject UICanvas;
    public bool UseSmoothTurn;
    private Vector2 leftInputAxis;
    private Vector2 rightInputAxis;
    private CharacterController controller;
    [SerializeField] private float speed;
    [SerializeField] private float upSpeed;
    private XROrigin rig;
    private float fallSpeed;
    private float heightOffset = 0.2f;
    private bool toggledOn = true;
    private ContinuousTurnProviderBase smoothTurn;
    private SnapTurnProviderBase snapTurn;

    private void Awake()
    {
        smoothTurn = GameObject.Find("Locomotion System").GetComponent<ContinuousTurnProviderBase>();
        snapTurn = GameObject.Find("Locomotion System").GetComponent<SnapTurnProviderBase>();
        ToggleSmoothTurn();
    }

    private void OnEnable()
    {
        LeftMenuAction.Enable();
        RightMenuAction.Enable();
    }

    private void OnDisable()
    {
        LeftMenuAction.Disable();
        RightMenuAction.Disable();
    }
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rig = GetComponent<XROrigin>();
    }
    private void FixedUpdate()
    {
        FollowHeadset();
        var headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        var direction = headYaw * new Vector3(leftInputAxis.x, 0, leftInputAxis.y);
        var up = new Vector3(0, rightInputAxis.y, 0);
        controller.Move(speed * Time.fixedDeltaTime * direction);
        controller.Move(upSpeed * Time.fixedDeltaTime * up);
        if (LeftMenuAction.triggered)
        {
            ToggleCanvas();
        }
        if (RightMenuAction.triggered)
        {
            ToggleCanvas();
        }

    }
    // Update is called once per frame
    void Update()
    {
        var leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftInputAxis);
        var rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightInputAxis);
    }
    void FollowHeadset()
    {
        controller.height = rig.CameraInOriginSpaceHeight + heightOffset;
        var capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        controller.center = new Vector3(capsuleCenter.x, controller.height / 2 + controller.skinWidth, capsuleCenter.z);
    }

    public void ToggleSmoothTurn()
    {
        if (!UseSmoothTurn)
        {
            smoothTurn.enabled = true;
            snapTurn.enabled = false;
            UseSmoothTurn = true;
        }
        else
        {
            smoothTurn.enabled = false;
            snapTurn.enabled = true;
            UseSmoothTurn = false;
        }
    }

    public void ToggleCanvas()
    {
        if (!toggledOn)
        {
            UICanvas.SetActive(true);
            toggledOn = true;
        }
        else
        {
            UICanvas.SetActive(false);
            toggledOn = false;
        }
    }
}
