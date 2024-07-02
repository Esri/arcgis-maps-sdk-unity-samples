using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewshedObserver : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void RaycastTest()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }

    private void FixedUpdate()
    {
        RaycastTest();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
