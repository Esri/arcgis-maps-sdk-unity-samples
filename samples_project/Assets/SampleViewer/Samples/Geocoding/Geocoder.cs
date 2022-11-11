/* Copyright 2022 Esri
 *
 * Licensed under the Apache License Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Net.Http;
using TMPro;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using Newtonsoft.Json.Linq;
using Esri.HPFramework;
using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using UnityEngine;

public class Geocoder : MonoBehaviour
{
    public GameObject AddressMarkerTemplate;
    public float AddressMarkerScale = 1;
    public GameObject LocationMarkerTemplate;
    public float LocationMarkerScale = 1;
    public GameObject AddressCardTemplate;
    public TextMeshProUGUI InfoField;

    private Camera MainCamera;
    private GameObject QueryLocationGO;
    private ArcGISMapComponent arcGISMapComponent;
    private string ResponseAddress = "";
    private bool ShouldPlaceMarker = false;
    private bool WaitingForResponse = false;
    private float Timer = 0;
    private readonly float MapLoadWaitTime = 1;
    private readonly string AddressQueryURL = "https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";
    private readonly string LocationQueryURL = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode";

    void Start()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
        MainCamera = Camera.main;
    }

    void Update()
    {
        // Create a marker and address card after an address lookup
        if (ShouldPlaceMarker)
        {
            // Wait for a fixed time for the map to load
            if (Timer < MapLoadWaitTime)
            {
                Timer += Time.deltaTime;
            }
            else
            {
                float CameraElevationOffset = 2000; // Height of the camera above the queried address

                SetupQueryLocationGameObject(AddressMarkerTemplate, scale: new Vector3(AddressMarkerScale, AddressMarkerScale, AddressMarkerScale));
                PlaceOnGround(QueryLocationGO);
                CreateAddressCard(true);

                // Place the camera above the marker and start rendering again
                ArcGISPoint MarkerPosition = QueryLocationGO.GetComponent<ArcGISLocationComponent>().Position;
                MainCamera.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(
                    MarkerPosition.X,
                    MarkerPosition.Y,
                    MarkerPosition.Z + CameraElevationOffset,
                    MarkerPosition.SpatialReference);
                MainCamera.GetComponent<Camera>().cullingMask = -1;
            }
        }

        // Determine the location that was clicked on and perform a location lookup
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 direction = (hit.point - MainCamera.transform.position);
                float distanceFromCamera = Vector3.Distance(MainCamera.transform.position, hit.point);
                float scale = distanceFromCamera * LocationMarkerScale / 5000; // Scale the marker based on its distance from camera 
                SetupQueryLocationGameObject(LocationMarkerTemplate, hit.point, MainCamera.transform.rotation, new Vector3(scale, scale, scale));
                ReverseGeocode(HitToGeoPosition(hit));
            }
        }
    }

    /// <summary>
    /// Verify the input text and call the geocoder. This function is called when an address is entered in the text input field.
    /// </summary>
    /// <param name="textInput"></param>
    public void HandleTextInput(string textInput)
    {
        if (!string.IsNullOrWhiteSpace(textInput))
        {
            Geocode(textInput);
        }

        // Deselct the text input field that was used to call this function. 
        // It is required so that the camera controller can be enabled/disabled when the input fieled is deselected/selected 
        var eventSystem = EventSystem.current;
        if (!eventSystem.alreadySelecting)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }


    /// <summary>
    /// Perform a geocoding query (address lookup) and parse the response. If the server returned an error, the message is shown to the user.
    /// </summary>
    /// <param name="address"></param>
    public async void Geocode(string address)
    {
        if (WaitingForResponse)
        {
            return;
        }

        WaitingForResponse = true;
        string results = await SendAddressQuery(address);

        if (results.Contains("error")) // Server returned an error
        {
            var response = JObject.Parse(results);
            var error = response.SelectToken("error");
            InfoField.text = (string)error.SelectToken("message");
        }
        else
        {
            var cameraStartHeight = 10000; // Use a high elevation to do a raycast from

            // Parse the query response
            var response = JObject.Parse(results);
            var candidates = response.SelectToken("candidates");
            if (candidates is JArray array)
            {
                if (array.Count > 0) // Check if the response included any result  
                {
                    var location = array[0].SelectToken("location");
                    var lon = location.SelectToken("x");
                    var lat = location.SelectToken("y");
                    ResponseAddress = (string)array[0].SelectToken("address");

                    // Move the camera to the queried address
                    MainCamera.GetComponent<Camera>().cullingMask = 0; // blacken the camera view until the scene is updated
                    ArcGISLocationComponent CamLocComp = MainCamera.GetComponent(typeof(ArcGISLocationComponent)) as ArcGISLocationComponent;
                    CamLocComp.Rotation = new ArcGISRotation(0, 0, 0);
                    CamLocComp.Position = new ArcGISPoint((double)lon, (double)lat, cameraStartHeight, new ArcGISSpatialReference(4326));

                    ShouldPlaceMarker = true;
                    Timer = 0;
                }
                else
                {
                    Destroy(QueryLocationGO);
                }

                // Update the info field in the UI
                InfoField.text = array.Count switch
                {
                    0 => "Query did not return a valid response.",
                    1 => "Enter an address above to move there or shift+click on a location to see the address / description.",
                    _ => "Query returned multiple results. If the shown location is not the intended one, make your input more specific.",
                };
            }
        }
        WaitingForResponse = false;
    }

    /// <summary>
    /// Perform a reverse geocoding query (location lookup) and parse the response. If the server returned an error, the message is shown to the user.
    /// The function is called when a location on the map is selected.
    /// </summary>
    /// <param name="location"></param>
    public async void ReverseGeocode(ArcGISPoint location)
    {
        if (WaitingForResponse)
        {
            return;
        }

        WaitingForResponse = true;
        string results = await SendLocationQuery(location.X.ToString() + "," + location.Y.ToString());

        if (results.Contains("error")) // Server returned an error
        {
            var response = JObject.Parse(results);
            var error = response.SelectToken("error");
            InfoField.text = (string)error.SelectToken("message");
        }
        else
        {
            var response = JObject.Parse(results);
            var address = response.SelectToken("address");
            var label = address.SelectToken("LongLabel");
            ResponseAddress = (string)label;

            if (string.IsNullOrEmpty(ResponseAddress))
            {
                InfoField.text = "Query did not return a valid response.";
            }
            else
            {
                InfoField.text = "Enter an address above to move there or shift+click on a location to see the address / description.";
                CreateAddressCard(false);
            }
        }
        WaitingForResponse = false;
    }

    /// <summary>
    /// Create and send an HTTP request for a geocoding query and return the received response.
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    private async Task<string> SendAddressQuery(string address)
    {

        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("address", address),
            new KeyValuePair<string, string>("token", arcGISMapComponent.APIKey),
            new KeyValuePair<string, string>("f", "json"),
        };

        HttpClient client = new HttpClient();
        HttpContent content = new FormUrlEncodedContent(payload);
        HttpResponseMessage response = await client.PostAsync(AddressQueryURL, content);

        response.EnsureSuccessStatusCode();
        string results = await response.Content.ReadAsStringAsync();
        return results;
    }

    /// <summary>
    ///  Create and send an HTTP request for a reverse geocoding query and return the received response.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    private async Task<string> SendLocationQuery(string location)
    {
        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("location", location),
            new KeyValuePair<string, string>("langCode", "en"),
            new KeyValuePair<string, string>("f", "json"),
        };

        HttpClient client = new HttpClient();
        HttpContent content = new FormUrlEncodedContent(payload);
        HttpResponseMessage response = await client.PostAsync(LocationQueryURL, content);

        response.EnsureSuccessStatusCode();
        string results = await response.Content.ReadAsStringAsync();
        return results;
    }

    /// <summary>
    /// Perform a raycast from current camera location towards the map to determine the height of earth's surface at that point.
    /// The game object received as input argument is placed on the ground and rotated upright
    /// </summary>
    /// <param name="markerGO"></param>
    void PlaceOnGround(GameObject markerGO)
    {
        Vector3 position = MainCamera.transform.position;
        var raycastStart = new Vector3(position.x, position.y, position.z);

        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo))
        {
            // Detrmine the geographic location of the point hit by the raycast and place the game object there
            markerGO.GetComponent<ArcGISLocationComponent>().Position = HitToGeoPosition(hitInfo, 0);
        }
        else // Raycast didn't hit an object. Print a warning
        {
            markerGO.GetComponent<ArcGISLocationComponent>().Position = MainCamera.GetComponent<ArcGISLocationComponent>().Position;
            Debug.LogWarning("The elevation at the queried location could not be determined.");
        }

        markerGO.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);

        ShouldPlaceMarker = false;
    }

    /// <summary>
    /// Return GeoPosition for an engine location hit by a raycast.
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

    /// <summary>
    /// Create an instance of the template game object and sets the transform based on input arguments. Ensures the game object has an ArcGISLocation component attached.
    /// </summary>
    /// <param name="templateGO"></param>
    /// <param name="location"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    private void SetupQueryLocationGameObject(GameObject templateGO,
        Vector3 location = new Vector3(), Quaternion rotation = new Quaternion(), Vector3 scale = new Vector3())
    {
        ArcGISLocationComponent MarkerLocComp;

        if (QueryLocationGO != null)
        {
            Destroy(QueryLocationGO);
        }

        QueryLocationGO = Instantiate(templateGO, location, rotation, arcGISMapComponent.transform);
        QueryLocationGO.transform.localScale = scale;

        if (!QueryLocationGO.TryGetComponent<ArcGISLocationComponent>(out MarkerLocComp))
        {
            MarkerLocComp = QueryLocationGO.AddComponent<ArcGISLocationComponent>();
        }
        MarkerLocComp.enabled = true;
    }

    /// <summary>
    /// Create a visual cue for showing the address/description returned for the query. 
    /// </summary>
    /// <param name="isAddressQuery"></param>
    void CreateAddressCard(bool isAddressQuery)
    {
        GameObject card = Instantiate(AddressCardTemplate, QueryLocationGO.transform);
        TextMeshProUGUI t = card.GetComponentInChildren<TextMeshProUGUI>();
        // Based on the type of the query set the location, rotation and scale of the text relative to the query location game object  
        if (isAddressQuery)
        {
            float localScale = 1.5f / AddressMarkerScale;
            card.transform.localPosition = new Vector3(0, 150f / AddressMarkerScale, 100f / AddressMarkerScale);
            card.transform.localRotation = Quaternion.Euler(90, 0, 0);
            card.transform.localScale = new Vector3(localScale, localScale, localScale);
        }
        else
        {
            float localScale = 3.5f / LocationMarkerScale;
            card.transform.localPosition = new Vector3(0, 300f / LocationMarkerScale, -300f / LocationMarkerScale);
            card.transform.localScale = new Vector3(localScale, localScale, localScale);
        }

        if (t != null)
        {
            t.text = ResponseAddress;
        }
    }
}
