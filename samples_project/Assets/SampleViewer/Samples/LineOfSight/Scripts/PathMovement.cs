// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Linq;
using UnityEngine;

public class PathMovement : MonoBehaviour
{
    [SerializeField] private GameObject WaypointParent;

    private Transform[] waypointTransforms;

    [SerializeField] private float Speed = 15f;

    [SerializeField] private float DistanceThreshold = 0.1f;

    private int waypointIndex;

    // Start is called before the first frame update
    private void Start()
    {
        // Get all of the child waypoint transforms.
        Transform[] transforms = WaypointParent.GetComponentsInChildren<Transform>();

        // Remove the first transform (the parent game object's transform).
        waypointTransforms = transforms.Skip(1).ToArray();

        // Start the player at the first waypoint.
        transform.position = waypointTransforms[0].position;
    }

    // Update is called once per frame
    private void Update()
    {
        // Move the object in the direction of the next waypoint.
        transform.position = Vector3.MoveTowards(transform.position, waypointTransforms[waypointIndex].position, Speed * Time.deltaTime);

        // Check if the object is close to the next waypoint.
        if (Vector3.Distance(transform.position, waypointTransforms[waypointIndex].position) < DistanceThreshold)
        {
            // Increment the next waypoint.
            waypointIndex = (waypointIndex + 1) % waypointTransforms.Length;

            // Point the object towards the next waypoint.
            transform.LookAt(waypointTransforms[waypointIndex]);
        }
    }
}