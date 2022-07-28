using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlightController : MonoBehaviour
{
    [Header("Input")]
    private Vector2 accelerate;
    private Vector2 pitch;
    [Header("Components")]
    private Rigidbody rb;
    private PlayerInput playerInput;
    private FlightSimControls flightSimControls;
    [Header("Constants")]
    private float maxThrustSpeed = 100000;
    private float thrustMultiplier = 50000;
    //private float minThrustToNotFall = 4000;
    //private float gravity = 981f;
    private float drag = 0.25f;
    private float startSpeed;
    private float takeOffSpeed = 5000;
    [Header("Dynamic Variables")]
    public float thrustSpeed;
    public float currentSpeed;
    //public float appliedGravity;
    [Header("Control Surfaces")]
    private float maxFlapPitch = 10;
    private float maxElevatorPitch = 25;
    private float maxRudderYaw = 45;
    private float maxAileronPitch = 45;
    [Header("Roll")]
    private float targetRoll;
    private float currentRoll;
    [Header("Pitch")]
    [SerializeField] private float currentPitch;
    private float targetPitch;
    [Header("Yaw")]
    public float currentYaw;
    private float targetYaw;

    private bool grounded;
    private float rotationX;
    private float rotationY;
    private float rotationZ;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        flightSimControls = new FlightSimControls();
    }
    private void OnEnable()
    {
        flightSimControls.PlaneMovement.Enable();
    }
    private void OnDisable()
    {
        flightSimControls.PlaneMovement.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        //Get Input
        accelerate = flightSimControls.PlaneMovement.Accelerate.ReadValue<Vector2>();
        pitch = flightSimControls.PlaneMovement.PitchandRoll.ReadValue<Vector2>();
        thrustSpeed = Mathf.Clamp(thrustSpeed, 0, maxThrustSpeed);
        currentSpeed = Mathf.Clamp(currentSpeed, 0, 100000);
        Vector3 rot = transform.localEulerAngles;
        rot.x = rotationX;
        rot.y = rotationY;
        rot.z = rotationZ;
        transform.localEulerAngles = rot;
        UpdatePosition();
        UpdatePitch(pitch.y);
        UpdateYaw(accelerate.x);
        UpdateRoll(pitch.x);
    }
    void UpdatePosition()
    {
        currentSpeed = Mathf.SmoothStep(currentSpeed, thrustSpeed, Time.deltaTime * drag);
        Vector3 newPosition = transform.forward * currentSpeed * Time.deltaTime;
        if (grounded)
        {
            if(currentSpeed > takeOffSpeed)
            {
                rb.AddForce(transform.position + newPosition);
            }
            rb.MovePosition(transform.position + newPosition);
        }
        else
        {
            rb.MovePosition(transform.position + newPosition);
        }
    }
    void UpdateRoll(float Roll)
    {

    }
    void UpdateYaw(float Yaw)
    {

    }
    void UpdatePitch(float input)
    { 

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Runway")
        {
            grounded = true;
        }
        else
        {
            Debug.Log("Crashed");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.name == "Runway")
        {
            grounded = false;
        }
    }
}
