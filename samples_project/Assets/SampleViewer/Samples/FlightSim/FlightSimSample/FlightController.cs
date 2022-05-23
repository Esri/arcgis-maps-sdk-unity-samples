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
    public float inertia;
    public float speed;
    public float upSpeed;
    private float momentum;
    public float rollRate;
    public float yawRate;
    public float pitchRate;
    [Header("Bools")]
    public bool engineOn;
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

    // Update is called once per frame
    void Update()
    {
        Vector3 rot = transform.localEulerAngles;
        //Clamp X/Pitch Rotation
        rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch);
        rot.x = rotationX;
        //Clamp Y/Yaw Rotation
        //rotationY = Mathf.Clamp(rotationY, minYaw, maxYaw);
        rot.y = rotationY;
        //Clamp Z/Roll Rotation
        rotationZ = Mathf.Clamp(rotationZ, minRoll, maxRoll);
        rot.z = rotationZ;
        transform.localEulerAngles = rot;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            engineOn = true;
        }
        if (engineOn)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
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
        if (Input.GetKeyUp(KeyCode.Keypad8))
        {
            rotationX = 0f;
        }
        //Input for Pitch Up
        if (Input.GetKey(KeyCode.Keypad5))
        {
            rotationX += -pitchRate * Time.deltaTime;
            transform.position += transform.up * upSpeed * Time.deltaTime;
        }
        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            rotationX = 0f;
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
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {

        }
    }
}
