using UnityEngine;
using UnityEngine.InputSystem;
public class FlightController_NewInput : MonoBehaviour
{
    //Private Variables
    [SerializeField]private bool isGrounded = false;
    private float rotationX;
    private float rotationY;
    private float rotationZ;
    private Vector2 accelerate;
    private Vector2 pitch;
    [Header("Components")]
    private Rigidbody rb;
    private PlayerInput playerInput;
    private FlightSimControls flightSimControls;
    [Header("Rates and Speeds")]
    public float acceleration;
    public float speed;
    public float upSpeed;
    public float glidingSpeed;
    public float rollRate;
    public float yawRate;
    public float pitchRate;
    [Header("Roll")]
    public float maxRoll;
    public float minRoll;
    [Header("Yaw")]
    public float maxYaw;
    public float minYaw;
    [Header("Pitch")]
    public float maxPitch;
    public float minPitch;

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
        accelerate = flightSimControls.PlaneMovement.Accelerate.ReadValue<Vector2>();
        pitch = flightSimControls.PlaneMovement.KeyboardMovement.ReadValue<Vector2>();
        Vector3 rot = transform.localEulerAngles;
        Vector3 pos = transform.position;
        transform.position = pos;
        //Clamp X/Pitch Rotation
        rotationX = Mathf.Clamp(rotationX, minPitch, maxPitch);
        rot.x = rotationX;
        //Clamp Y Position
        pos.y = Mathf.Clamp(transform.position.y, -3000f, 16500f);
        transform.position = pos;
        rotationY = Mathf.Clamp(rotationY, minYaw, maxYaw);
        rot.y = rotationY;
        //Clamp Z/Roll Rotation
        rotationZ = Mathf.Clamp(rotationZ, minRoll, maxRoll);
        rot.z = rotationZ;
        transform.localEulerAngles = rot;

        if (!isGrounded && speed > 1000)
        {
            AirControls();
        }
        else if (speed < 1000 && !isGrounded)
        {
            //Gliding();
        }
        else
        {
            Grounded();
        }
    }
    public void Grounded()
    {
        if (accelerate.y > 0.1f)
        {
            if (speed < 5000)
            {
                speed += acceleration + Time.deltaTime;
            }
            rb.AddRelativeForce(Vector3.forward * speed * Time.deltaTime);
        }
        if (speed > 1000)
        {
            //Input for Pitch Up
            if (pitch.y > 0)
            {
                rotationX += -pitchRate * Time.deltaTime;
                rb.AddRelativeForce(Vector3.up * 1000 * Time.deltaTime);
                //transform.position += transform.up * upSpeed * Time.deltaTime;
            }    
        }
        //Yaw Rotation
        if (accelerate.x > 0f)
        {
            rotationY += yawRate * Time.deltaTime;
            if(transform.rotation.y > 0f)
            {
                rb.AddRelativeForce(speed * Time.deltaTime * Vector3.right);
            }
        }
        else if (accelerate.x < 0f)
        {
            rb.AddRelativeTorque(speed * Time.deltaTime * Vector3.right);
            rotationY += -yawRate * Time.deltaTime;
        }
        else if(accelerate.x == 0f)
        {
            rotationY = 0f;
        }

    }
    public void AirControls()
    {
        if (pitch.y > 0)
        {
            rotationX += -pitchRate * Time.deltaTime;
            rb.AddRelativeForce(1000 * Time.deltaTime * Vector3.up);
            //transform.position += transform.up * upSpeed * Time.deltaTime;
        }
        else if(pitch.y < 0)
        {
            rotationX += pitchRate * Time.deltaTime;
            rb.AddRelativeForce(-1000 * Time.deltaTime * Vector3.up);
        }
        //Movement Input
        if (accelerate.y > 0.1f)
        {
            if (speed < 5000)
            {
                speed += acceleration * Time.deltaTime;
            }
            rb.AddRelativeForce(speed * Time.deltaTime * Vector3.forward);
            //transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        else
        {
            rb.AddRelativeForce(speed * Time.deltaTime * Vector3.forward);
            speed -= 10 * Time.deltaTime;
        }
        //Yaw Rotation
        if (accelerate.x > 0f)
        {
            rb.AddRelativeTorque(-100 * accelerate.x * Vector3.up);
            rotationY += yawRate * Time.deltaTime;
        }
        else if (accelerate.x < 0f)
        {
            rb.AddRelativeTorque(100 * accelerate.x * Vector3.up);
            rotationY += -yawRate * Time.deltaTime;
        }
    }
    public void Gliding()
    {

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
