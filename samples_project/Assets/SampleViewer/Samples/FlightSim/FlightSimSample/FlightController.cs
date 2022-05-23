using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Components;

public class FlightController : MonoBehaviour
{
    [Header ("Components")]
    public Rigidbody rb;
    [Header("Rates and Speeds")]
    public float acceleration;
    public float initialVelocity;
    public float mass;
    public float speed;
    public float upSpeed;
    public float rollRate;
    public float yawRate;
    public float pitchRate;
    [Header("Bools")]
    public bool engineOn;
    public bool released;
    [Header ("Roll")]
    public float maxRoll;
    public float minRoll;
    [Header ("Yaw")]
    public float maxYaw;
    public float minYaw;
    [Header ("Pitch")]
    public float maxPitch;
    public float minPitch;
    [Header ("Rotation Fields")]
    public float rotationX;
    public float rotationY;
    public float rotationZ;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 rot = transform.localEulerAngles;
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(transform.position.y, -100f, 17500f);
        transform.position = pos;
        //Clamp X/Pitch Rotation
        rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch);
        rot.x = rotationX;
        //Clamp Y Position
        pos.y = Mathf.Clamp(transform.position.y, -3000, 14000);
        transform.position = pos;
        rot.y = rotationY;
        //Clamp Z/Roll Rotation
        rotationZ = Mathf.Clamp(rotationZ, minRoll, maxRoll);
        rot.z = rotationZ;
        transform.localEulerAngles = rot;

        if(speed > 100)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            engineOn = true;
            speed = 0.1f;
        }
        if (engineOn && speed > 0)
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
                rotationX += pitchRate * Time.deltaTime;
                transform.position += transform.up * -upSpeed * Time.deltaTime;
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
                speed = initialVelocity + acceleration;
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                Debug.Log("Accelerating");
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                released = true;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                speed -= 100;
                Debug.Log("braking");
            }
            if (released)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                speed -= 10;
            }
        }
    }
}
