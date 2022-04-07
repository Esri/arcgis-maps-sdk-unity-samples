using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightCast : MonoBehaviour
{
    public Transform TargetTransform;

    public Transform Cylinder;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetTransform == null) return;
        var HitInfo = new RaycastHit();
        var rayDirection = TargetTransform.position - transform.position;
        if (Physics.Raycast(transform.position, rayDirection, out HitInfo))
        {
            // Check if the raycast hit the target object.
            if (HitInfo.transform == TargetTransform)
            {
                // enemy can see the player!
                Debug.Log("Hit!");
                    
            }
            else
            {
                // there is something obstructing the view
                Debug.Log("No hit!");
            }
            Debug.Log(HitInfo.distance);

            // Get the hit position.
            var direction = transform.position - TargetTransform.position;
            var targetPoint = transform.position + direction.normalized * HitInfo.distance;

            // Rotate and scale the cylinder to show the raycast.
            Cylinder.LookAt(targetPoint);

            Cylinder.position = transform.position - direction.normalized * HitInfo.distance/2;

            //Cylinder.position = (HitInfo.transform.position + transform.position) / 2.0f;
            Cylinder.localScale = new Vector3(Cylinder.localScale.x, Cylinder.localScale.y, HitInfo.distance/transform.localScale.z);
        }
    }
}
