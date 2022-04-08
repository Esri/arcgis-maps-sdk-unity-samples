using UnityEngine;

public class LineOfSightCast : MonoBehaviour
{
    public Transform TargetTransform;

    public Transform Cylinder;

    public Renderer LineMaterial;

    // Update is called once per frame
    private void Update()
    {
        if (TargetTransform == null || Cylinder == null) return;

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
                LineMaterial.material.SetInt("Visible", 1);
            }
            else
            {
                // Set the visible property of the shader graph.
                LineMaterial.material.SetInt("Visible", 0);
            }

            // Rotate the cylinder to the look towards the raycast hit.
            Cylinder.LookAt(HitInfo.point);

            // Move the cylinder to halfway between the sphere and the hit point.
            Cylinder.position = (transform.position + HitInfo.point) / 2;

            // Set the cylinder height to the distance of the ray cast.
            Cylinder.localScale = new Vector3(Cylinder.localScale.x, Cylinder.localScale.y, HitInfo.distance);
        }
    }
}