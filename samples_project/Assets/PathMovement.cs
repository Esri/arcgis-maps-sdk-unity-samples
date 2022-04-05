using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMovement : MonoBehaviour
{

    public List<Transform> movements = new List<Transform>();

    private float speed = 5f;

    private float distanceThreshold = 0.1f;

    private int currentWaypoint;


    // Start is called before the first frame update
    void Start()
    {
        // Start player at first waypoint.
        transform.position = movements[0].position;

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movements[currentWaypoint].position, speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, movements[currentWaypoint].position) < distanceThreshold)
        {
            currentWaypoint = (currentWaypoint+1)%movements.Count;
        }
    }
}
