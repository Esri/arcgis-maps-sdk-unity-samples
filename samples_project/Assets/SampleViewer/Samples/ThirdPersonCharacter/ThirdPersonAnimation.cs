using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody rb;
    private float maxSpeed = 5;
    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("Speed", rb.velocity.magnitude/maxSpeed);
    }
}
