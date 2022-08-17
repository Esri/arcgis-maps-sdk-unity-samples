
using System.Collections;
using System.Collections.Generic;

using System.Net.Http;
using System.Threading.Tasks;

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class Geocoder : MonoBehaviour
{
    public GameObject AddressMarkerTemplate;
    public float AddressMarkerScale = 1;
    public GameObject LocationMarkerTemplate;
    public float LocationMarkerScale = 1;
    public GameObject InfoCardTemplate;


    private GameObject QueryLocationGO;


    private bool ShouldUpdateMarker = false;
    private bool WaitingForResponse = false;
    private string AddressQueryURL = "https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";
    private string LocationQueryURL = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode";

    private ArcGISMapComponent arcGISMapComponent;
    private HPTransform CameraHPTransform;

    private HttpClient client = new HttpClient();
    private uint FrameCounter = 0;
    private uint MaxMapLoadFrames = 45;
    //private uint MaxRaycasts = 200;
    private double CameraElevationOffset = 4000;

    private string ResponseAddress = "";
    // Start is called before the first frame update
    void Start()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();

        CameraHPTransform = Camera.main.GetComponent<HPTransform>();
    }

    // Update is called once per frame
    void Update()
    {

        if (ShouldUpdateMarker)
        {

            if (FrameCounter < MaxMapLoadFrames)
            {
                FrameCounter++;
            }
            else
            {
                //ArcGISLocationComponent MarkerLocComp = QueryLocationGO.GetComponent<ArcGISLocationComponent>();
                //ArcGISLocationComponent CamLocComp = Camera.main.GetComponent<ArcGISLocationComponent>();

                PlaceOnGround(QueryLocationGO);
                GameObject infoCard = CreateInfoCard(true);
                infoCard.transform.SetParent(QueryLocationGO.transform, false);

                ArcGISPoint MarkerPosition = QueryLocationGO.GetComponent<ArcGISLocationComponent>().Position;
                Camera.main.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(
                    MarkerPosition.X,
                    MarkerPosition.Y,
                    MarkerPosition.Z + CameraElevationOffset,
                    MarkerPosition.SpatialReference);
                Camera.main.GetComponent<Camera>().cullingMask = -1;
            }
        }

        //if (Input.GetKeyDown("space"))
        //{
        //    //WaitingForResponse = false;
        //    //Geocode("NYC");
        //    // = int.MaxValue;
        //    //Debug.Log(Camera.main.GetComponent<Camera>().cullingMask);
        //    //ResponseAddress = "------------";
        //    //CreateInfoCard();

        //}




        // Create a marker when Shift is Held and Mouse is Clicked
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {


                // var locationMarker = Instantiate(AddressMarkerTemplate, hit.point, Quaternion.identity, arcGISMapComponent.transform);
                SetupQueryLocationGameObject(false);
                //QueryLocationGO.transform.position = hit.point;
                var geoPosition = HitToGeoPosition(hit);

                var locationComponent = QueryLocationGO.GetComponent<ArcGISLocationComponent>();
                //locationComponent.enabled = true;
                locationComponent.Position = geoPosition;
                locationComponent.Rotation = new ArcGISRotation(0, 90, 0);

                ReverseGeocode(geoPosition);
            }
        }

    }

    public async void Geocode(string address)
    {
        if (WaitingForResponse || address.Equals(""))
        {
            return;
        }
        WaitingForResponse = true;
        string results = await SendAddressQuery(address);

        if (results.Contains("error"))
        {
            Debug.Log("error");
        }
        else
        {
            var cameraStartHeight = 10000;

            var response = JObject.Parse(results);
            var candidate = response.SelectToken("candidates")[0];

            ResponseAddress = (string) candidate.SelectToken("address");

            var location = candidate.SelectToken("location");
            var lon = location.SelectToken("x");
            var lat = location.SelectToken("y");



            SetupQueryLocationGameObject(true);

            Camera.main.GetComponent<Camera>().cullingMask = 0;
            ArcGISLocationComponent CamLocComp = Camera.main.GetComponent(typeof(ArcGISLocationComponent)) as ArcGISLocationComponent;
            CamLocComp.Rotation = new ArcGISRotation(0, 0, 0);
            CamLocComp.Position = new ArcGISPoint((double)lon, (double)lat, cameraStartHeight, new ArcGISSpatialReference(4326));


            ShouldUpdateMarker = true;
            FrameCounter = 0;
        }
        WaitingForResponse = false;

    }

    public async void ReverseGeocode(ArcGISPoint location)
    {

        if (WaitingForResponse)
        {
            return;
        }

        WaitingForResponse = true;
        string results = await SendLocationQuery(location.X.ToString() + "," + location.Y.ToString());

        if (results.Contains("error"))
        {
            Debug.Log("error");
            //DisplayError(results);
        }
        else
        {
            var response = JObject.Parse(results);
            var address = response.SelectToken("address");
            var label = address.SelectToken("LongLabel");
            Debug.Log(label);

            ResponseAddress = (string)label;
            GameObject infoCard = CreateInfoCard(false);
            infoCard.transform.SetParent(QueryLocationGO.transform, false);
        }

        WaitingForResponse = false;

    }


    private async Task<string> SendAddressQuery(string address)
    {

        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("address", address),
            new KeyValuePair<string, string>("token", arcGISMapComponent.APIKey),
            new KeyValuePair<string, string>("f", "json"),
        };

        HttpContent content = new FormUrlEncodedContent(payload);

        HttpResponseMessage response = await client.PostAsync(AddressQueryURL, content);
        response.EnsureSuccessStatusCode();

        string results = await response.Content.ReadAsStringAsync();
        return results;
    }


    private async Task<string> SendLocationQuery(string location)
    {
        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("location", location),
            new KeyValuePair<string, string>("f", "json"),
        };

        HttpContent content = new FormUrlEncodedContent(payload);

        HttpResponseMessage response = await client.PostAsync(LocationQueryURL, content);
        response.EnsureSuccessStatusCode();

        string results = await response.Content.ReadAsStringAsync();
        return results;
    }


    void PlaceOnGround(GameObject markerGO)
    {
        Vector3 position = Camera.main.transform.position;
        var raycastStart = new Vector3(position.x, position.y, position.z);
        ArcGISLocationComponent CamLocComp = Camera.main.GetComponent<ArcGISLocationComponent>();

        ///////////////////////////////////////////////////////////////////////////////////////////////            
        Debug.DrawRay(raycastStart, Vector3.down * 10000, Color.green, 120, false);
        //////////////////////////////////////////////////////////////////////////////////////////////
        //RaycastHit lastHit = new RaycastHit();
        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo))
        {
            //Debug.Log(markerGO.GetComponent<HPTransform>().UniversePosition);
            //Debug.Log(hitInfo.point);
            //markerGO.GetComponent<HPTransform>().UniversePosition = new double3(hitInfo.point.ToDouble3());

            //if (lastHit.point != hitInfo.point)
            //{
            //    lastHit = hitInfo;
            //    Camera.main.transform.position = hitInfo.point;
            //    Debug.Log("------------");
            //}
            //else
            //{
            //    //markerGO.transform.position = hitInfo.point;
            //    Debug.Log(string.Format("{0}........{1}={2}........{3}", position, lastHit.point, hitInfo.point, hitInfo.collider.gameObject.name));
            //}

            //Debug.Log(string.Format("counter: {0}", stableCount));


            //Debug.Log(markerGO.GetComponent<HPTransform>().UniversePosition);

            markerGO.GetComponent<ArcGISLocationComponent>().Position = HitToGeoPosition(hitInfo, 0);
        }
        else
        {
            markerGO.GetComponent<ArcGISLocationComponent>().Position = Camera.main.GetComponent<ArcGISLocationComponent>().Position;
            Debug.LogWarning("The elevation at the queried location could not be determined.");
        }
        markerGO.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);


        //if (++raycastCount > MaxRaycasts)
        //{
        //    Debug.Log("No hit");
        //    ShouldUpdateMarker = false;
        //    return;
        //}
        //    Debug.Log(raycastCount);

        //markerGO.transform.position = lastHit.point;
        ShouldUpdateMarker = false;
    }

    /// <summary>
    /// Return GeoPosition Based on RaycastHit; I.E. Where the user clicked in the Scene.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private ArcGISPoint HitToGeoPosition(RaycastHit hit, float yOffset = 0)
    {
        var worldPosition = math.inverse(arcGISMapComponent.WorldMatrix).HomogeneousTransformPoint(hit.point.ToDouble3());

        var geoPosition = arcGISMapComponent.View.WorldToGeographic(worldPosition);
        var offsetPosition = new ArcGISPoint(geoPosition.X, geoPosition.Y, geoPosition.Z + yOffset, geoPosition.SpatialReference);

        return GeoUtils.ProjectToSpatialReference(offsetPosition, new ArcGISSpatialReference(4326));
    }


    private void SetupQueryLocationGameObject(bool isAddressQuery)
    {
        ArcGISLocationComponent MarkerLocComp;
        
        if (QueryLocationGO != null)
        {
            Destroy(QueryLocationGO);
        }

        //MarkerLocComp.Position = new ArcGISPoint(0, 0, 0, new ArcGISSpatialReference(4326));

        if (isAddressQuery)
        {
            QueryLocationGO = Instantiate(AddressMarkerTemplate, arcGISMapComponent.transform);
            QueryLocationGO.transform.localScale = new Vector3(AddressMarkerScale, AddressMarkerScale, AddressMarkerScale);
        }
        else
        {
            QueryLocationGO = Instantiate(LocationMarkerTemplate, arcGISMapComponent.transform);
            QueryLocationGO.transform.localScale = new Vector3(LocationMarkerScale, LocationMarkerScale, LocationMarkerScale);
        }
        
        if (!QueryLocationGO.TryGetComponent<ArcGISLocationComponent>(out MarkerLocComp))
        {
            MarkerLocComp = QueryLocationGO.AddComponent<ArcGISLocationComponent>();
        }
        MarkerLocComp.enabled = true;
        //QueryLocationGO.transform.rotation = Quaternion.identity;
        MarkerLocComp.Position = new ArcGISPoint(0,0,0,new ArcGISSpatialReference(4326));
        MarkerLocComp.Rotation = new ArcGISRotation(0, 90, 0);
        //Debug.Log(string.Format("////{0} {1} {2}", MarkerLocComp.Rotation.Pitch, MarkerLocComp.Rotation.Heading, MarkerLocComp.Rotation.Roll ));
        //QueryLocationGO.transform.position = new Vector3(0, 0, 0);
        //QueryLocationGO.transform.rotation = Quaternion.identity;
        //QueryLocationGO.AddComponent(typeof(HPTransform));
        //QueryLocationGO.AddComponent(typeof(ArcGISLocationComponent));

    }


    GameObject CreateInfoCard(bool isAddressQuery)
    {
        GameObject card = Instantiate(InfoCardTemplate);
        TextMeshProUGUI t = card.GetComponentInChildren<TextMeshProUGUI>();
        
        if (isAddressQuery)
        {
            float localScale = (float)3 / AddressMarkerScale;
            card.transform.localPosition = new Vector3(0,250,200);  
            card.transform.localRotation = Quaternion.Euler(90,0,0);
            card.transform.localScale = new Vector3(localScale, localScale, localScale);
        }
        else
        {
            card.transform.localPosition = new Vector3(0, 2, 2);
            card.transform.localRotation = Quaternion.Euler(90, 0, 0);
            float localScale = (float)3 / LocationMarkerScale;
            card.transform.localScale = new Vector3(localScale, localScale, localScale);
        }


        if (t != null)
        {
            t.text = ResponseAddress;
        }

        return card;
    }
}
