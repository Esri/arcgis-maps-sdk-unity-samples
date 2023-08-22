using System.Collections;
using System.Collections.Generic;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using UnityEngine;

[RequireComponent(typeof(ArcGISLocationComponent))]
public class FeatureData : MonoBehaviour
{
    public ArcGISLocationComponent LocationComponent;

    public List<double> coordinates = new List<double>();
    public List<string> properties = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        LocationComponent = GetComponent<ArcGISLocationComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}