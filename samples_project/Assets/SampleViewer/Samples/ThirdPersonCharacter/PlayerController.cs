using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls controls;
    public InputAction move;

    private Animator anim;
    private Rigidbody rb;
    [SerializeField] private Camera cam;
    public float movementForce = 1f;
    public float jumpForce = 5f;
    private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new PlayerControls();
        Cursor.visible = false;
    }
    private void OnEnable()
    {
        controls.Controls.Jump.started += DoJump;
        move = controls.Controls.Movement;
        controls.Controls.Enable();
    }
    private void OnDisable()
    {
        controls.Controls.Jump.started -= DoJump;
        controls.Controls.Disable();
    }
    private void FixedUpdate()
    {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(cam) * movementForce;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(cam) * movementForce;

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if(rb.velocity.y < 0f)
        {
            rb.velocity += Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;
        }
        Vector3 horixontalVelocity = rb.velocity;
        horixontalVelocity.y = 0f;
        if(horixontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = horixontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }
        LookAt();
    }
    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }
    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;
        if(move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.25f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private Vector3 GetCameraForward(Camera cam)
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera cam)
    {
        Vector3 right = cam.transform.right;
        right.y = 0;
        return right.normalized;
    }
}
