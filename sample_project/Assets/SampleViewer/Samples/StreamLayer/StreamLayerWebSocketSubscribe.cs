// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Samples.Components;
using Esri.GameEngine.Geometry;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using Esri.HPFramework;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class PlaneFeature
{
    public PlaneGeometry geometry;
    public PlaneProperties attributes;
    public PlaneGeometry predictedPoint;

    public void PredictLocation(double intervalMilliseconds)
    {
        var cGroundSpeedKnots = attributes.speed;
        var metersPerSec = cGroundSpeedKnots * 0.51444444444;
        var simulationSpeedFactor = 1.5;
        var timespanSec = (intervalMilliseconds / 1000.0) * simulationSpeedFactor;
        double[] currentPoint = new double[3] { predictedPoint.x, predictedPoint.y, predictedPoint.z };
        var headingDegrees = attributes.heading;
        var drPoint = DeadReckoning.DeadReckoningPoint(metersPerSec, timespanSec, currentPoint, headingDegrees);
        predictedPoint.x = drPoint[0];
        predictedPoint.y = drPoint[1];
        predictedPoint.z = currentPoint[2];
    }

    public static PlaneFeature Create(string name, double x, double y, double z, float heading, float speed, DateTime dateTimeStamp)
    {
        PlaneFeature planeFeature = new PlaneFeature();
        planeFeature.geometry = new PlaneGeometry();
        planeFeature.geometry.x = x;
        planeFeature.geometry.y = y;
        planeFeature.geometry.z = z;
        planeFeature.attributes = new PlaneProperties();
        planeFeature.attributes.name = name;
        planeFeature.attributes.heading = heading;
        planeFeature.attributes.speed = speed;
        planeFeature.attributes.dateTimeStamp = dateTimeStamp;
        planeFeature.predictedPoint = new PlaneGeometry();
        planeFeature.predictedPoint.x = planeFeature.geometry.x;
        planeFeature.predictedPoint.y = planeFeature.geometry.y;
        planeFeature.predictedPoint.z = planeFeature.geometry.z;
        return planeFeature;
    }
}

[Serializable]
public class PlaneProperties
{
    public string name;
    public float heading;
    public float speed;
    public DateTime dateTimeStamp;
}

[Serializable]
public class PlaneGeometry
{
    public double x;
    public double y;
    public double z;
}

// This class issues a query request to a Feature Layer which it then parses to create GameObjects at accurate locations
// with correct property values. This is a good starting point if you are looking to parse your own feature layer into Unity.
public class StreamLayerWebSocketSubscribe : MonoBehaviour
{
    public GameObject planeSymbolPrefab;
    public float symbolScaleFactor = 2000.0f;
    public float timeToLive;

    public string wsUrl = "wss://geoeventsample1.esri.com:6143/arcgis/ws/services/FAAStream/StreamServer/subscribe";
    public string nameField;
    public string headingField;
    public string speedField;
    public string timeField;

    // In the query request we can denote the Spatial Reference we want the return geometries in.
    // It is important that we create the GameObjects with the same Spatial Reference
    private int SRWKID = 4326;

    private ClientWebSocket wsClient;

    private Dictionary<string, List<PlaneFeature>> planeData;

    // This will hold a reference to each feature we created
    public List<GameObject> flights = new List<GameObject>();

    // This camera reference will be passed to the flights to calculate the distance from the camera to each flight
    [SerializeField] private ArcGISCameraComponent ArcGISCamera;

    [SerializeField] private TMP_Dropdown flightSelector;

    [SerializeField] private TextMeshProUGUI connectionStatus;
    [SerializeField] private Image connectionIndicator;

    // Get all the features when the script starts
    void Start()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
        planeData = new Dictionary<string, List<PlaneFeature>>();
        var result = Connect();
        
        flightSelector.onValueChanged.AddListener(delegate
        {
            FlightSelected();
        });
    }

    private void Update()
    {
        if (IsConnected())
        {
            connectionStatus.text = "Connection Status: Connected";
            connectionIndicator.color = Color.green;
        }
        else
        {
            connectionStatus.text = "Connection Status: Not Connected";
            connectionIndicator.color = Color.red;
        }
    }

    private void LateUpdate()
    {
        DisplayPlaneData();
    }

    private void HandleMessage(byte[] buffer, int count)
    {
        string data = Encoding.UTF8.GetString(buffer, 0, count);
        TryParseFeedAndUpdatePlane(data);
    }
   
    public async Task Connect()
    {
        if (wsClient == null)
        {
            wsClient = new ClientWebSocket();
            await wsClient.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
            byte[] buffer = new byte[10240];

            while (wsClient.State == WebSocketState.Open)
            {
                var completeMessage = new List<byte>();
                WebSocketReceiveResult result;

                do
                {
                    result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        return;
                    }

                    for (int i = 0; i < result.Count; i++)
                    {
                        completeMessage.Add(buffer[i]);
                    }

                } while (!result.EndOfMessage);

                HandleMessage(completeMessage.ToArray(), completeMessage.Count);
            }
        }
    }

    public bool IsConnected()
    {
        if (wsClient != null)
        {
            return wsClient.State == WebSocketState.Open;
        }

        return false;
    }

    public async Task Disconnect()
    {
        if (wsClient != null)
        {
            if (wsClient.State != WebSocketState.Closed)
            {
                await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }

    /* Json Data should be in this format
	 *	{
	 *		"geometry": {
	 *			"x":-81.55,
	 *			"y":38.76666667,
	 *			"z":8132.30691036,
	 *			"spatialReference":{"wkid":4326}
	 *		},
	 *		"attributes": {
	 *			"FlightID":11946171,
	 *			"ACID":"JBU669",
	 *			"DateTimeStamp":1652836010718,
	 *			"Longitude":-81.55,
	 *			"Latitude":38.76666667,
	 *			"Heading":266.8079695,
	 *			"GroundSpeedKnots":5
	 *		}
	 *	}
	*/

    private void TryParseFeedAndUpdatePlane(string data)
    {
        //Debug.Log(data);
        var jObject = JObject.Parse(data);
        var jAttributes = jObject.SelectToken("attributes");
        if (jAttributes != null)
        {
            var name = jAttributes.SelectToken(nameField).ToString();
            var geomToken = jObject.SelectToken("geometry");

            //no point to go on
            if (geomToken == null)
            {
                return;
            }

            double x = 0, y = 0, z = 0;
            float heading = 0, speed = 0;
            var xToken = geomToken.SelectToken("x");
            if (xToken == null)
            {
                return;
            }

            double.TryParse(geomToken.SelectToken("x").ToString(), out x);
            var yToken = geomToken.SelectToken("y");
            if (yToken == null)
            {
                return;
            }

            double.TryParse(geomToken.SelectToken("y").ToString(), out y);
            var jt = geomToken.SelectToken("z");
            if (jt != null)
            {
                double.TryParse(jt.ToString(), out z);
            }

            var hdToken = jAttributes.SelectToken(headingField);
            if (hdToken != null)
            {
                float.TryParse(hdToken.ToString(), out heading);
            }

            var spToken = jAttributes.SelectToken(speedField);
            if (spToken != null)
            {
                float.TryParse(spToken.ToString(), out speed);
            }

            long timestampMS = 0;
            var dtToken = jAttributes.SelectToken(timeField);
            if (dtToken != null)
            {
                long.TryParse(dtToken.ToString(), out timestampMS);
            }

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestampMS);
            var dateTimeStamp = dateTimeOffset.DateTime;

            var planeList = planeData.ContainsKey(name) ? planeData[name] : new List<PlaneFeature>();

            // Don't exceed 10 observations per plane
            if (planeList.Count > 10)
            {
                planeList.RemoveAt(0);
            }

            PlaneFeature planeFeature = PlaneFeature.Create(name, x, y, z, heading, speed, dateTimeStamp);

            planeList.Add(planeFeature);
            planeData[name] = planeList;
        }
        else
        {
            Debug.Log("Unsupported data format");
        }
    }

    // Populates the flight drop-down with the flight names from the flights list
    private void PopulateFlightDropdown()
    {
        //Populate flight name drop-down
        List<string> flightNames = new List<string>();
        foreach (GameObject flight in flights)
        {
            flightNames.Add(flight.name);
        }
        flightNames.Sort();
        flightSelector.options.Clear();
        flightSelector.AddOptions(flightNames);
    }

    private void DisplayPlaneData()
    {
        try
        {
            foreach (var plane in planeData.Keys.ToArray())
            {
                var planeList = planeData[plane];
                var planeFeature = planeList[planeList.Count - 1];
                planeFeature.PredictLocation(Time.deltaTime * 1000.0);
                GameObject gobjPlane = GameObject.Find(planeFeature.attributes.name);

                if (gobjPlane != null)
                {
                    // If elapse time since last update is more than 5 minutes remove the game object to conserve memory
                    TimeSpan timespan = DateTime.Now - planeFeature.attributes.dateTimeStamp.ToLocalTime();
                    
                    if (timespan.TotalMinutes > timeToLive)
                    {
                        TMP_Dropdown.OptionData optionToRemove = flightSelector.options.Find(option => option.text == planeFeature.attributes.name);
                        if (optionToRemove != null)
                        {
                            flightSelector.options.Remove(optionToRemove);
                            flightSelector.RefreshShownValue();
                            Debug.Log("remove" + planeFeature.attributes.name);
                        }
                        else
                        {
                            Debug.LogWarning("Option with name: " + optionToRemove + " not found!");
                        }

                        flights.Remove(gobjPlane);
                        Destroy(gobjPlane);
                        planeData.Remove(plane);
                        continue;
                    }

                    var locationComponent = gobjPlane.GetComponent<ArcGISLocationComponent>();
                    locationComponent.Position = new ArcGISPoint(planeFeature.predictedPoint.x, planeFeature.predictedPoint.y, planeFeature.predictedPoint.z, new ArcGISSpatialReference(SRWKID));

                    HPTransform hpTransform = gobjPlane.GetComponent<HPTransform>();
                    hpTransform.LocalScale = new Vector3(symbolScaleFactor, symbolScaleFactor, symbolScaleFactor);

                    var rotator = locationComponent.Rotation;
                    rotator.Heading = planeFeature.attributes.heading;
                    locationComponent.Rotation = rotator;

                    NameLabel nameLabelComponent = gobjPlane.GetComponentInChildren<NameLabel>();
                    if (nameLabelComponent != null)
                    {
                        nameLabelComponent.slider.maxValue = timeToLive;
                        nameLabelComponent.slider.value = timeToLive - (float)timespan.TotalMinutes;
                    }
                }
                else
                {
                    TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(planeFeature.attributes.name);
                    flightSelector.options.Add(newOption);
                    flightSelector.RefreshShownValue();

                    GameObject clonePrefab = Instantiate(planeSymbolPrefab, this.transform);
                    clonePrefab.name = planeFeature.attributes.name;
                    flights.Add(clonePrefab);
                    clonePrefab.SetActive(true);

                    HPTransform hpTransform = clonePrefab.GetComponent<HPTransform>();
                    hpTransform.LocalScale = new Vector3(symbolScaleFactor, symbolScaleFactor, symbolScaleFactor);
                    var locationComponent = clonePrefab.GetComponent<ArcGISLocationComponent>();
                    locationComponent.enabled = true;
                    locationComponent.Position = new ArcGISPoint(planeFeature.geometry.x, planeFeature.geometry.y, planeFeature.geometry.z, new ArcGISSpatialReference(SRWKID));
                    var rotator = locationComponent.Rotation;
                    rotator.Pitch = 90.0;
                    rotator.Heading = planeFeature.attributes.heading;
                    locationComponent.Rotation = rotator;

                    NameLabel nameLabelComponent = clonePrefab.GetComponentInChildren<NameLabel>();
                    if (nameLabelComponent != null)
                    {
                        nameLabelComponent.nameLabel = clonePrefab.name;
                        nameLabelComponent.slider.maxValue = timeToLive;
                        nameLabelComponent.slider.value = timeToLive;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to create game object: " + ex.Message);
        }
    }

#if UNITY_EDITOR
    private async void EditorApplication_playModeStateChanged(PlayModeStateChange playModeState)
    {
        if (playModeState == PlayModeStateChange.ExitingPlayMode)
        {
            if (wsClient != null)
            {
                await Disconnect();
            }
        }
    }
#endif

    // When a new entry is selected in the flight dropdown move the camera to the new position
    private void FlightSelected()
    {
        var flightName = flightSelector.options[flightSelector.value].text;
        foreach (GameObject flight in flights)
        {
            if (flightName == flight.name)
            {
                var flightLocation = flight.GetComponent<ArcGISLocationComponent>();
                if (flightLocation == null)
                {
                    return;
                }
                var CameraLocation = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
                double Longitude = flightLocation.Position.X;
                double Latitude = flightLocation.Position.Y;
                double height = flightLocation.Position.Z + 1500;
                ArcGISPoint NewPosition = new ArcGISPoint(Longitude, Latitude, height, flightLocation.Position.SpatialReference);

                CameraLocation.Position = NewPosition;
                CameraLocation.Rotation = flightLocation.Rotation;
            }
        }
    }
}
