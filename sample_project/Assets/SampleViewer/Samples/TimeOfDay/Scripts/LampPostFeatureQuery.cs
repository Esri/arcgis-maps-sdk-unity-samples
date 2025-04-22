using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampPostFeatureQuery : ArcGISFeatureLayerComponent
{
    // Start is called before the first frame update
    void Start()
    {
        CreateLink(webLink.Link);
        StartCoroutine(GetFeatures());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
