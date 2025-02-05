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
using UnityEngine.UI;
using Esri.ArcGISMapsSDK.Samples.Components;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Esri.ArcGISMapsSDK.Utils;

public class Geocoder : MonoBehaviour
{
    [SerializeField] private GameObject AddressMarkerTemplate;
    [SerializeField] private float AddressMarkerScale = 1;
    [SerializeField] private GameObject LocationMarkerTemplate;
    [SerializeField] private float LocationMarkerScale = 1;
    [SerializeField] private GameObject AddressCardTemplate;
    [SerializeField] private TextMeshProUGUI InfoField;
    [SerializeField] private Button SearchButton;

    private Camera MainCamera;
    private GameObject QueryLocationGO;
    private ArcGISMapComponent arcGISMapComponent;
    private Animator animator;
    private string ResponseAddress = "";
    private string textInput;
    private bool WaitingForResponse = false;
    private float DistanceFromCamera;
    private readonly string AddressQueryURL = "https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";
    private readonly string LocationQueryURL = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode";

    private InputManager inputManager;
    private string apiKey;

    private void Awake()
    {
        inputManager = FindFirstObjectByType<InputManager>();
    }

    public void Geocode()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
#else
        Ray ray = Camera.main.ScreenPointToRay(inputManager.touchControls.Touch.TouchPosition.ReadValue<Vector2>());
#endif

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = (hit.point - MainCamera.transform.position);
            DistanceFromCamera = Vector3.Distance(MainCamera.transform.position, hit.point);
            float scale = DistanceFromCamera * LocationMarkerScale / 400000; // Scale the marker based on its distance from camera 
            Quaternion markerRotationPerpendicular = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion markerRotationFacingCamera = MainCamera.transform.rotation;
            Quaternion markerRotation = markerRotationPerpendicular * markerRotationFacingCamera;
            SetupQueryLocationGameObject(LocationMarkerTemplate, hit.point, markerRotation, new Vector3(scale, scale, scale));
            ReverseGeocode(HitToGeoPosition(hit));
        }
    }

    void Start()
    {
        arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
        MainCamera = Camera.main;
        animator = GameObject.Find("InfoMenu").GetComponent<Animator>();
        SearchButton.onClick.AddListener(delegate { HandleTextInput(textInput); });
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

    private void PlacePin()
    {
        SetupQueryLocationGameObject(AddressMarkerTemplate, scale: new Vector3(AddressMarkerScale, AddressMarkerScale, AddressMarkerScale));
        PlaceOnGround(QueryLocationGO);
        CreateAddressCard(true);
        MainCamera.GetComponent<Camera>().cullingMask = -1;
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
            var cameraStartHeight = 1500; // Use a high elevation to do a raycast from

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
                }
                else
                {
                    Destroy(QueryLocationGO);
                }

                // Update the info field in the UI
                InfoField.text = array.Count switch
                {
                    0 => "Query did not return a valid response.",
                    1 => "Enter an address above to move there or Shift+Click on a location to see the address / description.",
                    _ => "Query returned multiple results. If the shown location is not the intended one, make your input more specific.",
                };

                if (array.Count == 0 || array.Count == 50)
                {
                    animator.Play("NotificationAnim");
                }
            }
        }
        WaitingForResponse = false;
        PlacePin();
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
                InfoField.text = "Enter an address above to move there or Shift+Click on a location to see the address / description.";
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

        apiKey = arcGISMapComponent.APIKey;

        if (apiKey == "")
        {
            apiKey = ArcGISProjectSettingsAsset.Instance.APIKey;
        }

        IEnumerable<KeyValuePair<string, string>> payload = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("address", address),
            new KeyValuePair<string, string>("token", apiKey),
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
        markerGO.GetComponent<ArcGISLocationComponent>().Position = MainCamera.GetComponent<ArcGISLocationComponent>().Position;
        markerGO.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);
        markerGO.GetComponent<ArcGISLocationComponent>().SurfacePlacementMode = ArcGISSurfacePlacementMode.OnTheGround;
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
            float localScale = LocationMarkerScale / 40;
            card.transform.localPosition = new Vector3(0, 300f / LocationMarkerScale + 400, -300f / LocationMarkerScale);
            card.transform.localScale = new Vector3(localScale, localScale, localScale);
        }

        if (t != null)
        {
            t.text = ResponseAddress;
        }
    }
}