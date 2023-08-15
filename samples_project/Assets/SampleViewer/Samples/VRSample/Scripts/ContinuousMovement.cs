using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class ContinuousMovement : MonoBehaviour
{
    private CharacterController controller;
    private float fallSpeed;
    private float heightOffset = 0.2f;
    [SerializeField] private GameObject Instructions;
    [SerializeField] private InputAction LeftMenuAction;
    private Vector2 leftInputAxis;
    [SerializeField] private XRNode leftInputSource;
    [SerializeField] private XRInteractorLineVisual leftLineVisual;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private GameObject playerCamera;
    private XROrigin rig;
    [SerializeField] private InputAction RightMenuAction;
    private Vector2 rightInputAxis;
    [SerializeField] private XRNode rightInputSource;
    [SerializeField] private XRInteractorLineVisual rightLineVisual;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private ContinuousTurnProviderBase smoothTurn;
    private Toggle smoothTurnToggle;
    [SerializeField] private SnapTurnProviderBase snapTurn;
    [SerializeField] private float speed;
    private bool toggledOn = true;
    [SerializeField] private GameObject UICanvas;
    [SerializeField] private float upSpeed;
    [SerializeField] private bool UseSnapTurn;

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
        smoothTurn = GameObject.Find("Locomotion System").GetComponent<ContinuousTurnProviderBase>();
        snapTurn = GameObject.Find("Locomotion System").GetComponent<SnapTurnProviderBase>();
        smoothTurnToggle = Instructions.GetComponentInChildren<Toggle>();
        smoothTurnToggle.isOn = UseSnapTurn;
        ToggleSmoothTurn();
    }

    private void FixedUpdate()
    {
        FollowHeadset();
        var headYaw = Quaternion.Euler(0, rig.Camera.transform.eulerAngles.y, 0);
        var direction = headYaw * new Vector3(leftInputAxis.x, 0, leftInputAxis.y);
        var up = new Vector3(0, rightInputAxis.y, 0);
        controller.Move(speed * Time.fixedDeltaTime * direction);
        controller.Move(upSpeed * Time.fixedDeltaTime * up);
    }

    private void LateUpdate()
    {
        UICanvas.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 15;
        UICanvas.transform.rotation = new Quaternion(0.0f, playerCamera.transform.rotation.y, 0.0f,
            playerCamera.transform.rotation.w);
    }

    // Update is called once per frame
    void Update()
    {
        var leftDevice = InputDevices.GetDeviceAtXRNode(leftInputSource);
        leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftInputAxis);
        var rightDevice = InputDevices.GetDeviceAtXRNode(rightInputSource);
        rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightInputAxis);

        if (LeftMenuAction.triggered)
        {
            ToggleCanvas();
        }

        if (RightMenuAction.triggered)
        {
            ToggleCanvas();
        }
    }

    void FollowHeadset()
    {
        controller.height = rig.CameraInOriginSpaceHeight + heightOffset;
        var capsuleCenter = transform.InverseTransformPoint(rig.Camera.transform.position);
        controller.center = new Vector3(capsuleCenter.x, controller.height / 2 + controller.skinWidth, capsuleCenter.z);
    }

    public void ToggleSmoothTurn()
    {
        if (smoothTurnToggle.isOn)
        {
            smoothTurn.enabled = true;
            snapTurn.enabled = false;
        }
        else
        {
            smoothTurn.enabled = false;
            snapTurn.enabled = true;
        }
    }

    public void ToggleCanvas()
    {
        if (!toggledOn)
        {
            Instructions.SetActive(true);
            toggledOn = true;
            leftLineVisual.enabled = true;
            leftRayInteractor.enabled = true;
            rightLineVisual.enabled = true;
            rightRayInteractor.enabled = true;
        }
        else
        {
            Instructions.SetActive(false);
            toggledOn = false;
            leftLineVisual.enabled = false;
            leftRayInteractor.enabled = false;
            rightLineVisual.enabled = false;
            rightRayInteractor.enabled = false;
        }
    }
}