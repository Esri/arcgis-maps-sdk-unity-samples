// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

public class LineOfSightCast : MonoBehaviour
{
    [SerializeField] private Transform TargetTransform;

    [SerializeField] private Transform LOSCylinder;

    [SerializeField] private Renderer LineMaterial;

    // Update is called once per frame
    private void Update()
    {
        if (TargetTransform == null || LOSCylinder == null) return;

        // Calculate the direction between the sphere and the target.
        var rayDirection = TargetTransform.position - transform.position;

        // Create a RaycastHit for use in the raycast method.
        var HitInfo = new RaycastHit();

        // Raycast from the sphere to the target.
        if (Physics.Raycast(transform.position, rayDirection, out HitInfo))
        {
            // Check if the raycast hit the target object.
            if (HitInfo.transform == TargetTransform)
            {
                // Set the visible property of the shader graph.
                LineMaterial.material.SetColor("ShaderColor", new Color(0.325f, 0.898f, 0.165f, 1.0f));
            }
            else
            {
                // Set the visible property of the shader graph.
                LineMaterial.material.SetColor("ShaderColor", new Color(0.867f, 0.251f, 0.251f, 1.0f));
            }

            // Rotate the cylinder to the look towards the raycast hit.
            LOSCylinder.LookAt(HitInfo.point);
            LOSCylinder.Rotate(new Vector3(90, 0, 0));

            // Move the cylinder to halfway between the sphere and the hit point.
            LOSCylinder.position = (transform.position + HitInfo.point) / 2;

            // Set the cylinder height to the distance of the ray cast.
            LOSCylinder.localScale = new Vector3(LOSCylinder.localScale.x, HitInfo.distance / 2, LOSCylinder.localScale.z);
        }
    }
}