using System.Collections.Generic;
using UnityEngine;

public class PathMovement : MonoBehaviour
{
    public List<Transform> movements = new List<Transform>();

    [SerializeField] private float speed = 5f;

    [SerializeField] private float distanceThreshold = 0.1f;

    private int waypointIndex;

    // Start is called before the first frame update
    private void Start()
    {
        // Start player at first waypoint.
        transform.position = movements[0].position;
        transform.LookAt(movements[0].position);
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movements[waypointIndex].position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, movements[waypointIndex].position) < distanceThreshold)
        {
            waypointIndex = (waypointIndex + 1) % movements.Count;
            transform.LookAt(movements[waypointIndex]);
        }
    }
}