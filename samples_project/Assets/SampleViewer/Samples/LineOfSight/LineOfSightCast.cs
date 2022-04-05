using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightCast : MonoBehaviour
{
    public Transform OtherTransform;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OtherTransform == null) return;
        var HitInfo = new RaycastHit();
        var rayDirection = OtherTransform.position - transform.position;
        if (Physics.Raycast(transform.position, rayDirection, out HitInfo))
        {
            if (HitInfo.transform == OtherTransform)
            {
                // enemy can see the player!
                Debug.Log("Hit!");
                    
            }
            else
            {
                // there is something obstructing the view
                Debug.Log("No hit!");
            }
        }
    }
}
