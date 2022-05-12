using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Esri.GameEngine.Location;
using Esri.GameEngine.Camera;
using Esri.GameEngine.View;
using Esri.GameEngine.Map;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;

public class ArcFactory : MonoBehaviour
{
    private float curveAltitude = 200;
    private int curveResolution = 100;
    private float widthMultiplier = 100;
    public Material material = null;
    
    private int FeatureSRWKID = 4326;

    private LineRenderer lineRenderer;
    private GameObject line;
    private GameObject peak;
    private ArcGISLocationComponent midLocation;

    private GameObject arc = null;

    private List<GameObject> vertices = new List<GameObject>();

    void DrawArc() 
    {
        vertices = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Marker(Clone)").ToList();

        if (!arc) {
            arc = new GameObject("Arc") ;
            arc.transform.SetParent(this.transform, false);
        }

        CalculateVertices();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.material = (material != null) ? material : new Material(Shader.Find("Universal Render Pipeline/Lit"));
        DrawArc();
    }

    // Update is called once per frame
    void Update()
    {
        DrawArc();
    }

    Vector3 GenerateBezier (Vector3 start, Vector3 mid, Vector3 end, float t) {
        return Vector3.Lerp(Vector3.Lerp(start, mid, t), Vector3.Lerp(mid, end, t), t);
    }

    void CalculateVertices() 
    {
        if (arc && vertices.Count > 1) {
            List<Vector3> allPoints = new List<Vector3>();
            for (int v = 0; v < (vertices.Count - 1); v++) {
                ArcGISLocationComponent startLocation = vertices[v].GetComponent(typeof(ArcGISLocationComponent)) as ArcGISLocationComponent;
                ArcGISLocationComponent endLocation = vertices[v+1].GetComponent(typeof(ArcGISLocationComponent)) as ArcGISLocationComponent;

                float startLon = (float)startLocation.Position.X;
                float startLat = (float)startLocation.Position.Y;
                float endLon = (float)endLocation.Position.X;
                float endLat = (float)endLocation.Position.Y;
                Vector3 startV = startLocation.transform.position;
                Vector3 endV = endLocation.transform.position;

                Debug.Log(startLat + "," + startLon + " -> " + endLat + "," + endLon);

                float midLat = (endLat + startLat) / 2;
                float midLon = (endLon + startLon) / 2;
                if (line) {
                    lineRenderer = line.GetComponent(typeof(LineRenderer)) as LineRenderer;
                } else {
                    line = new GameObject("arcLine");
                    line.transform.SetParent(this.transform, false);
                    lineRenderer = line.AddComponent<LineRenderer>();
                    lineRenderer.material = material;
                    lineRenderer.widthMultiplier = widthMultiplier;
                    lineRenderer.positionCount = curveResolution;
                }

                if (peak) {
                    midLocation = peak.GetComponent(typeof(ArcGISLocationComponent)) as ArcGISLocationComponent;
                } else {
                    peak = new GameObject("arcPeak");
                    midLocation = peak.AddComponent<ArcGISLocationComponent>();
                    peak.transform.SetParent(this.transform, false);
                }
                midLocation.Position = new GeoPosition(midLat, midLon, curveAltitude, FeatureSRWKID);

                Vector3 midV = midLocation.transform.position;

                float xDif = midV.x - startV.x;
                float yDif = midV.y - startV.y;
                float zDif = midV.z - startV.z;

                for (int i = 0; i < curveResolution; i++) {
                    allPoints.Add(GenerateBezier(startV, midV, endV, (float)((float)i/(float)curveResolution)));
                }
            }

            lineRenderer.SetPositions(allPoints.ToArray());
        }
    }
}