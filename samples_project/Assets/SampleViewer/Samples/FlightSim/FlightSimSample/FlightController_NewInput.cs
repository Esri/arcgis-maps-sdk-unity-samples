using UnityEngine;
using TMPro;
using Esri.ArcGISMapsSDK.Components;
using System;

public class FlightController_NewInput : MonoBehaviour
{
    [Header ("Private Variables")]
    private bool isGrounded = false;
    private bool speedIncreasing;
    private float rotationX;
    private float rotationY = 30;
    private float rotationZ;
    private Vector2 accelerate;
    private Vector2 pitch;
    private float maxThrustSpeed = 25000;
    private float thrustMultiplier = 1000;
    [Header("Components")]
    public TextMeshProUGUI speed;
    public TextMeshProUGUI altitude;
    private Rigidbody rb;
    private FlightSimControls flightSimControls;
    private ArcGISLocationComponent location;
    [Header("Rates and Speeds")]
    public float upSpeed;
    public float turnSpeed;
    public float rollRate;
    public float yawRate;
    public float pitchRate;
    private float takeOffSpeed = 5000;
    private float drag = 0.25f;
    [Header("Dynamic Variables")]
    public float thrustSpeed;
    public float currentSpeed;
    [Header("Roll")]
    public float maxRoll;
    public float minRoll;
    [Header("Pitch")]
    public float maxPitch;
    public float minPitch;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        flightSimControls = new FlightSimControls();
        location = GetComponent<ArcGISLocationComponent>();
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
        speed.text = "Speed: " + Mathf.Round(currentSpeed) + " mph";
        altitude.text = "Altitude: " + Mathf.Round(Convert.ToSingle(location.Position.Z)) + " Meters";
        //Get Input
        accelerate = flightSimControls.PlaneMovement.Accelerate.ReadValue<Vector2>();
        pitch = flightSimControls.PlaneMovement.PitchandRoll.ReadValue<Vector2>();
        thrustSpeed = Mathf.Clamp(thrustSpeed, 0, maxThrustSpeed);
        if (accelerate.y > 0)
        {
            thrustSpeed += accelerate.y * Time.deltaTime * thrustMultiplier;
        }
        else if (accelerate.y < 0)
        {
            thrustSpeed += accelerate.y * Time.deltaTime * thrustMultiplier;
        }
        //Set Angles & Position to Vectors
        Vector3 pos = transform.position;
        //Clamp Y Position
        pos.y = Mathf.Clamp(transform.position.y, -3050f, 13500f);
        transform.position = pos;
        Vector3 rot = transform.localEulerAngles;
        rot.x = rotationX;
        rot.y = rotationY;
        rot.z = rotationZ;
        transform.localEulerAngles = rot;
        UpdatePosition();
    }

    void UpdatePosition()
    {
        currentSpeed = Mathf.Clamp(currentSpeed, 0, 1000);
        currentSpeed = Mathf.SmoothStep(currentSpeed, thrustSpeed, Time.deltaTime * drag);
        Vector3 newPosition = transform.forward * currentSpeed * Time.deltaTime;
        if (isGrounded)
        {
            if (currentSpeed > takeOffSpeed)
            {
                rb.AddForce(transform.position + newPosition);
            }
            rb.MovePosition(transform.position + newPosition);
        }
        else
        {
            rb.MovePosition(transform.position + newPosition);
        }
        if (currentSpeed > 100)
        {
            //Input for Pitch Up
            if (pitch.y > 0)
            {
                rotationX += -pitchRate * Time.deltaTime;
                rb.AddForce(upSpeed * Time.deltaTime * Vector3.up);
            }
        }
        //Yaw Rotation
        if (accelerate.x > 0f)
        {
            rotationY += yawRate * Time.deltaTime;
        }
        else if (accelerate.x < 0f)
        {
            rotationY += -yawRate * Time.deltaTime;
        }
        if (!isGrounded)
        {
            //pitch
            if (pitch.y > 0 && speedIncreasing)
            {
                rotationX += -pitchRate * Time.deltaTime;
                rb.AddForce(upSpeed * Time.deltaTime * Vector3.up);
            }
            else if (pitch.y < 0)
            {
                rotationX += pitchRate * Time.deltaTime;
                rb.AddForce(-upSpeed * Time.deltaTime * Vector3.up);
            }
            //Roll
            if (pitch.x > 0f)
            {
                rotationZ += -rollRate * Time.deltaTime;
            }
            else if (pitch.x < 0f)
            {
                rotationZ += rollRate * Time.deltaTime;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Runway")
        {
            isGrounded = true;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
