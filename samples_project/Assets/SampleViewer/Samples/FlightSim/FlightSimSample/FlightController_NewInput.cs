using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Esri.HPFramework;
using System;
public class FlightController_NewInput : MonoBehaviour
{
    [Header ("Private Variables")]
    private bool isGrounded = false;
    private bool speedIncreasing;
    private float rotationX;
    private float rotationY;
    private float rotationZ;
    private Vector2 accelerate;
    private Vector2 pitch;
    [Header("Components")]
    private Rigidbody rb;
    private PlayerInput playerInput;
    private HPTransform hpTransform;
    private FlightSimControls flightSimControls;
    [Header("Rates and Speeds")]
    public float acceleration;
    public float speed;
    public float reverseSpeed;
    public float upSpeed;
    public float turnSpeed;
    public float rollRate;
    public float yawRate;
    public float pitchRate;
    [Header("Roll")]
    public float maxRoll;
    public float minRoll;
    [Header("Pitch")]
    public float maxPitch;
    public float minPitch;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        flightSimControls = new FlightSimControls();
        hpTransform = GetComponent<HPTransform>();
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
        //Set Angles & Position to Vectors
        Vector3 rot = transform.localEulerAngles;
        Vector3 pos = transform.position;
        //Clamp X/Pitch Rotation
        rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch);
        rot.x = rotationX;
        //Clamp Y Position
        pos.y = Mathf.Clamp(transform.position.y, -3050f, 13500f);
        transform.position = pos;
        //rotationY = Mathf.Clamp(rotationY, minYaw, maxYaw);
        rot.y = rotationY;
        //Clamp Z/Roll Rotation
        rotationZ = Mathf.Clamp(rotationZ, minRoll, maxRoll);
        rot.z = rotationZ;
        transform.localEulerAngles = rot;
        //Check if Plane is on Ground or Not
        if (!isGrounded)
        {
            AirControls();
        }
        else
        {
            Grounded();
        }
    }
    public void Grounded()
    {
        if (accelerate.y > 0f)
        {
            if (speed < 5000)
            {
                speed += acceleration + Time.deltaTime;
                rb.AddForce(speed * Time.deltaTime * transform.forward);
            }
            rb.MovePosition(transform.position + (speed * Time.deltaTime * transform.forward));
        }
        else if(accelerate.y < 0f)
        {
            rb.MovePosition(transform.position + (transform.forward * reverseSpeed * Time.deltaTime));
        }
        else
        {
            speed = 0;
        }
        if (speed > 1000)
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
            //rb.AddForce(turnSpeed * Time.deltaTime * Vector3.up);
            rotationY += yawRate * Time.deltaTime;
        }
        else if (accelerate.x < 0f)
        {
            //rb.AddForce(-turnSpeed * Time.deltaTime * Vector3.up);
            rotationY += -yawRate * Time.deltaTime;
        }
    }
    public void AirControls()
    {
        if (pitch.y > 0 && speedIncreasing)
        {
            rotationX += -pitchRate * Time.deltaTime;
            rb.AddForce(upSpeed * Time.deltaTime * Vector3.up);
        }
        else if(pitch.y < 0)
        {
            rotationX += pitchRate * Time.deltaTime;
            rb.AddForce(-upSpeed * Time.deltaTime * Vector3.up);
        }
        //Movement Input
        if (accelerate.y > 0.1f)
        {
            if (speed < 5000)
            {
                speed += acceleration * Time.deltaTime;
                speedIncreasing = true;
            }
            rb.MovePosition(transform.position + (transform.forward * speed * Time.deltaTime));
        }
        else if (accelerate.y < 0f && speed > 0)
        {
            rb.MovePosition(transform.position + (transform.forward * speed * Time.deltaTime));
            speed += reverseSpeed * Time.deltaTime;
            speedIncreasing = false;
        }
        else if(speed > 0)
        {
            rb.MovePosition(transform.position + (transform.forward * speed * Time.deltaTime));
            speed -= 10 * Time.deltaTime;
            speedIncreasing = false;
        }
        //Yaw
        if(transform.rotation.z != 0)
        {
            if (accelerate.x > 0f)
            {
                rb.AddForce(turnSpeed * Time.deltaTime * Vector3.up);
                rotationY += yawRate * Time.deltaTime;
            }
            else if (accelerate.x < 0f)
            {
                rb.AddForce(-turnSpeed * Time.deltaTime * Vector3.up);
                rotationY += -yawRate * Time.deltaTime;
            }
        }
        //Roll
        if (pitch.x > 0f)
        {
            rotationZ += -rollRate * Time.deltaTime;
        }
        else if(pitch.x < 0f)
        {
            rotationZ += rollRate * Time.deltaTime;
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
