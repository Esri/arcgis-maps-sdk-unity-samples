using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlightController : MonoBehaviour
{
    private bool released;
    private bool isGrounded = false;
    private float rotationX;
    private float rotationY;
    private float rotationZ;
    [Header ("Components")]
    private Rigidbody rb;
    [Header("Rates and Speeds")]
    public float acceleration;
    public float speed;
    public float upSpeed;
    public float glidingSpeed;
    public float rollRate;
    public float yawRate;
    public float pitchRate;
    [Header ("Roll")]
    public float maxRoll;
    public float minRoll;
    [Header ("Yaw")]
    public float maxYaw;
    public float minYaw;
    [Header ("Pitch")]
    public float maxPitch;
    public float minPitch;

    private void Awake()
    {
        Keyboard keyboard = Keyboard.current;
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 rot = transform.localEulerAngles;
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(transform.position.y, -3000f, 17500f);
        transform.position = pos;
        //Clamp X/Pitch Rotation
        rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch);
        rot.x = rotationX;
        //Clamp Y Position
        pos.y = Mathf.Clamp(transform.position.y, -3000f, 16500f);
        transform.position = pos;
        rot.y = rotationY;
        //Clamp Z/Roll Rotation
        rotationZ = Mathf.Clamp(rotationZ, minRoll, maxRoll);
        rot.z = rotationZ;
        transform.localEulerAngles = rot;

        if(speed >= 1000 && !isGrounded)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
        if (!isGrounded && speed > 1000)
        {
            //Input for Roll Left
            if (Input.GetKey(KeyCode.Keypad4))
            {
                rotationZ += rollRate * Time.deltaTime;
            }
            //Input for Roll Right
            if (Input.GetKey(KeyCode.Keypad6))
            {
                rotationZ += -rollRate * Time.deltaTime;
            }
            //Input for Pitch Down
            if (Input.GetKey(KeyCode.Keypad8))
            {
                rotationZ += -rollRate * Time.deltaTime;
            }
            //Input for Pitch Up
            if (Input.GetKey(KeyCode.Keypad5))
            {
                rotationX += -pitchRate * Time.deltaTime;
                transform.position += transform.up * upSpeed * Time.deltaTime;
            }
            //Input for Yaw Right
            if (Input.GetKey(KeyCode.D))
            {
                rotationY += yawRate * Time.deltaTime;
            }
            //Input for Yaw Left
            if (Input.GetKey(KeyCode.A))
            {
                rotationY += -yawRate * Time.deltaTime;
            }
            //Movement Input
            if (Input.GetKey(KeyCode.W))
            {
                released = false;
                if (speed < 5000)
                {
                    speed += acceleration * Time.deltaTime;
                }
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                released = true;
            }
            if (Input.GetKey(KeyCode.S) && speed > 0)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                speed -= 100 * Time.deltaTime;
            }
            if (released && speed > 0)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                speed -= 10 * Time.deltaTime;
            }
        }
        else if(speed < 1000 && !isGrounded)
        {
            Debug.Log("Test");
            released = false;
            transform.Translate(Vector3.forward * glidingSpeed * Time.deltaTime);
            //Input for Roll Left
            if (Input.GetKey(KeyCode.Keypad4))
            {
                rotationZ += rollRate * Time.deltaTime;
            }
            //Input for Roll Right
            if (Input.GetKey(KeyCode.Keypad6))
            {
                rotationZ += -rollRate * Time.deltaTime;
            }
            //Input for Pitch Down
            if (Input.GetKey(KeyCode.Keypad8))
            {
                rotationX += pitchRate * Time.deltaTime;
            }
            //Input for Pitch Up
            if (Input.GetKey(KeyCode.Keypad5))
            {
                rotationX += -pitchRate * Time.deltaTime;
            }
            //Input for Yaw Right
            if (Input.GetKey(KeyCode.D))
            {
                rotationY += yawRate * Time.deltaTime;
            }
            //Input for Yaw Left
            if (Input.GetKey(KeyCode.A))
            {
                rotationY += -yawRate * Time.deltaTime;
            }
        }
        else
        {
            if(speed > 1000)
            {
                //Input for Pitch Up
                if (Input.GetKey(KeyCode.Keypad5))
                {
                    rotationX += -pitchRate * Time.deltaTime;
                    transform.position += transform.up * upSpeed * Time.deltaTime;
                }
            }
            //Input for Yaw Right
            if (Input.GetKey(KeyCode.D))
            {
                rotationY += yawRate * Time.deltaTime;
            }
            //Input for Yaw Left
            if (Input.GetKey(KeyCode.A))
            {
                rotationY += -yawRate * Time.deltaTime;
            }
            //Movement Input
            if (Input.GetKey(KeyCode.W))
            {
                if(speed < 5000)
                {
                    speed += acceleration + Time.deltaTime;
                    Debug.Log("Accelerating");
                }
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Runway")
        {
            isGrounded = true;
            rb.useGravity = true;
            Debug.Log("Grounded");
        }
    }
    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
        Debug.Log("Flying");
    }
}
