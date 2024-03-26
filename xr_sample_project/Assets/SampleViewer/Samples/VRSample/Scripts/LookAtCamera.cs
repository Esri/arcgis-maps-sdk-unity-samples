using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [Header("----------Camera Transform----------")]
    [SerializeField] private Transform target;

    void Update()
    {
        transform.LookAt(target, Vector3.up);
    }
}
