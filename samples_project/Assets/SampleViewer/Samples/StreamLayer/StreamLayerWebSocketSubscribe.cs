// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEditor;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using Esri.HPFramework;

[System.Serializable]
public class TrackFeature
{
    public TrackGeometry geometry;
    public TrackProperties attributes;
    public TrackGeometry predictedPoint;

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

    public static TrackFeature Create(string name, double x, double y, double z, float heading, float speed, DateTime dateTimeStamp)
    {
        TrackFeature trackFeature = new TrackFeature();
        trackFeature.geometry = new TrackGeometry();
        trackFeature.geometry.x = x;
        trackFeature.geometry.y = y;
        trackFeature.geometry.z = z;
        trackFeature.attributes = new TrackProperties();
        trackFeature.attributes.name = name;
        trackFeature.attributes.heading = heading;
        trackFeature.attributes.speed = speed;
        trackFeature.attributes.dateTimeStamp = dateTimeStamp;
        trackFeature.predictedPoint = new TrackGeometry();
        trackFeature.predictedPoint.x = trackFeature.geometry.x;
        trackFeature.predictedPoint.y = trackFeature.geometry.y;
        trackFeature.predictedPoint.z = trackFeature.geometry.z;
        return trackFeature;
    }
}

[System.Serializable]
public class TrackProperties
{
    public string name;
    public float heading;
    public float speed;
    public DateTime dateTimeStamp;
}

[System.Serializable]
public class TrackGeometry
{
    public double x;
    public double y;
    public double z;
}

// This class issues a query request to a Feature Layer which it then parses to create GameObjects at accurate locations
// with correct property values. This is a good starting point if you are looking to parse your own feature layer into Unity.
public class StreamLayerWebSocketSubscribe : MonoBehaviour
{
    public GameObject trackSymbolPrefab;
    public float symbolScaleFactor = 2000.0f;
    public float timeToLive = 5.0f; //minutes

    // The height where we spawn the flight before finding the actual height
    private int FlightSpawnHeight = 10000;

    public string wsUrl = "ws://geoeventsample1.esri.com:6180/arcgis/ws/services/FAAStream/StreamServer/subscribe";
    public string nameField;
    public string headingField;
    public string speedField;
    public string timeField;

    // In the query request we can denote the Spatial Reference we want the return geometries in.
    // It is important that we create the GameObjects with the same Spatial Reference
    private int SRWKID = 4326;

    private ClientWebSocket wsClient;

    private Dictionary<string, List<TrackFeature>> trackData;

    // This will hold a reference to each feature we created
    public List<GameObject> flights = new List<GameObject>();

    // This camera reference will be passed to the stadiums to calculate the distance from the camera to each stadium
    public ArcGISCameraComponent ArcGISCamera;

    public Dropdown flightSelector;

    // Get all the features when the script starts
    void Start()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
        trackData = new Dictionary<string, List<TrackFeature>>();
        var result = Connect();

        flightSelector.onValueChanged.AddListener(delegate
        {
            FlightSelected();
        });
    }

    private void LateUpdate()
    {
        DisplayTrackData();
    }

    private void HandleMessage(byte[] buffer, int count)
    {
        string data = Encoding.UTF8.GetString(buffer, 0, count);
        TryParseFeedAndUpdateTrack(data);
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
                var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    HandleMessage(buffer, result.Count);
                }
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

    private void TryParseFeedAndUpdateTrack(string data)
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

            var trackList = trackData.ContainsKey(name) ? trackData[name] : new List<TrackFeature>();
            // Don't exceed 10 observations per track
            if (trackList.Count > 10)
            {
                trackList.RemoveAt(0);
            }

            TrackFeature trackFeature = TrackFeature.Create(name, x, y, z, heading, speed, dateTimeStamp);

            trackList.Add(trackFeature);
            trackData[name] = trackList;
        }
        else
        {
            Debug.Log("Unsupported data format");
        }
    }

    private void DisplayTrackData()
    {
        try
        {
            foreach (var track in trackData.Keys.ToArray())
            {
                var trackList = trackData[track];
                var trackFeature = trackList[trackList.Count - 1];
                trackFeature.PredictLocation(Time.deltaTime * 1000.0);
                GameObject gobjTrack = GameObject.Find(trackFeature.attributes.name);
                if (gobjTrack != null)
                {
                    // If elapse time since last update is more than 5 minutes remove the game object to conserve memory
                    TimeSpan timespan = DateTime.Now - trackFeature.attributes.dateTimeStamp.ToLocalTime();
                    if (timespan.TotalMinutes > timeToLive)
                    {
                        Destroy(gobjTrack);
                        trackData.Remove(track);
                        continue;
                    }
                    var locationComponent = gobjTrack.GetComponent<ArcGISLocationComponent>();
                    locationComponent.Position = new ArcGISPoint(trackFeature.predictedPoint.x, trackFeature.predictedPoint.y, trackFeature.predictedPoint.z, new ArcGISSpatialReference(SRWKID));

                    //gobjTrack.transform.localScale = Vector3.one * symbolScaleFactor;
                    HPTransform hpTransform = gobjTrack.GetComponent<HPTransform>();
                    hpTransform.LocalScale = new Vector3(symbolScaleFactor, symbolScaleFactor, symbolScaleFactor);

                    var rotator = locationComponent.Rotation;
                    rotator.Heading = trackFeature.attributes.heading;
                    locationComponent.Rotation = rotator;
                    Transform nameLabelTransform = gobjTrack.transform.GetChild(1);
                    if (nameLabelTransform != null)
                    {
                        GameObject nameLabel = nameLabelTransform.gameObject;
                        NameLabel nameLabelComponent = nameLabel.GetComponent<NameLabel>();
                        nameLabelComponent.slider.maxValue = timeToLive;
                        nameLabelComponent.slider.value = timeToLive - (float)timespan.TotalMinutes;
                    }
                }
                else
                {
                    GameObject clonePrefab = Instantiate(trackSymbolPrefab, this.transform);
                    clonePrefab.name = trackFeature.attributes.name;
                    clonePrefab.SetActive(true);
                    //clonePrefab.transform.localScale = new Vector3(symbolScaleFactor, symbolScaleFactor, symbolScaleFactor);
                    HPTransform hpTransform = clonePrefab.GetComponent<HPTransform>();
                    hpTransform.LocalScale = new Vector3(symbolScaleFactor, symbolScaleFactor, symbolScaleFactor);
                    var locationComponent = clonePrefab.GetComponent<ArcGISLocationComponent>();
                    locationComponent.enabled = true;
                    locationComponent.Position = new ArcGISPoint(trackFeature.geometry.x, trackFeature.geometry.y, trackFeature.geometry.z, new ArcGISSpatialReference(SRWKID));
                    var rotator = locationComponent.Rotation;
                    rotator.Pitch = 90.0;
                    rotator.Heading = trackFeature.attributes.heading;
                    locationComponent.Rotation = rotator;

                    Transform nameLabelTransform = clonePrefab.transform.GetChild(1);
                    if (nameLabelTransform != null)
                    {
                        GameObject nameLabel = nameLabelTransform.gameObject;
                        NameLabel nameLabelComponent = nameLabel.GetComponent<NameLabel>();
                        nameLabelComponent.nameLabel = clonePrefab.name;
                        nameLabelComponent.slider.maxValue = timeToLive;
                        nameLabelComponent.slider.value = timeToLive;
                    }
                }

                // remove trackFeature if it is not updated within a specified time interval
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

    // Populates the stadium drown down with all the stadium names from the Stadiums list
    private void PopulateFlightDropdown()
    {
        //Populate Stadium name drop down
        List<string> flightNames = new List<string>();
        foreach (GameObject Stadium in flights)
        {
            flightNames.Add(Stadium.name);
        }
        flightNames.Sort();
        flightSelector.AddOptions(flightNames);
    }

    // When a new entry is selected in the stadium dropdown move the camera to the new position
    private void FlightSelected()
    {
        var flightName = flightSelector.options[flightSelector.value].text;
        foreach (GameObject flight in flights)
        {
            if(flightName == flight.name)
            {
                var flightLocation = flight.GetComponent<ArcGISLocationComponent>();
                if (flightLocation == null)
                {
                    return;
                }
                var CameraLocation = ArcGISCamera.GetComponent<ArcGISLocationComponent>();
                double Longitude = flightLocation.Position.X;
                double Latitude  = flightLocation.Position.Y;

                ArcGISPoint NewPosition = new ArcGISPoint(Longitude, Latitude, FlightSpawnHeight, flightLocation.Position.SpatialReference);

                CameraLocation.Position = NewPosition;
                CameraLocation.Rotation = flightLocation.Rotation;
            }
        }
    }
}
