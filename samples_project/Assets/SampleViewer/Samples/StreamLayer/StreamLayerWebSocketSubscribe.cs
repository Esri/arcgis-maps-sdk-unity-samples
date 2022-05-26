using Esri.ArcGISMapsSDK.Components;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;
using UnityEditor;
using UnityEngine.Rendering;
using Esri.GameEngine.Geometry;
using Newtonsoft.Json.Linq;

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

public class StreamLayerWebSocketSubscribe : MonoBehaviour
{
	public GameObject trackSymbolPrefab;
	public float symbolScaleFactor = 2000.0f;
	public float timeToLive = 5.0f; //minutes

	public string wsUrl = "ws://geoeventsample1.esri.com:6180/arcgis/ws/services/FAAStream/StreamServer/subscribe";
	public string nameField;
	public string headingField;
	public string speedField;
	public string timeField;

	// In the query request we can denote the Spatial Reference we want the return geometries in.
	// It is important that we create the GameObjects with the same Spatial Reference
	private int FeatureSRWKID = 4326;

	private ClientWebSocket wsClient;

	private Dictionary<string, List<TrackFeature>> trackData;

	// Start is called before the first frame update
	void Start()
	{
#if UNITY_EDITOR
		EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
		trackData = new Dictionary<string, List<TrackFeature>>();
		var result = Connect();
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

	/*"
	 *	{
	 *		\"geometry\": {
	 *			\"x\":-81.55,
	 *			\"y\":38.76666667,
	 *			\"z\":8132.30691036,
	 *			\"spatialReference\":{\"wkid\":4326}
	 *	},\"attributes\":
	 *		{
	 *			\"FlightID\":11946171,
	 *			\"ACID\":\"JBU669\",
	 *			\"DateTimeStamp\":1652836010718,
	 *			\"Longitude\":-81.55,
	 *			\"Latitude\":38.76666667,
	 *			\"Heading\":266.8079695,
	 *			\"GroundSpeedKnots\":5"
	*/
	private void ParseFeedAndUpdateTrack(string data)
	{
		try
		{
			var trackFeature = JsonUtility.FromJson<TrackFeature>(data);
			trackFeature.predictedPoint.x = trackFeature.geometry.x;
			trackFeature.predictedPoint.y = trackFeature.geometry.y;
			trackFeature.predictedPoint.z = trackFeature.geometry.z;

			//GeoPosition Position = new GeoPosition(Longitude, Latitude, Altitude, FeatureSRWKID);

			var trackList = trackData.ContainsKey(trackFeature.attributes.name) ? trackData[trackFeature.attributes.name] : new List<TrackFeature>();
			// Don't exceed 10 observations per track
			if (trackList.Count > 10)
			{
				trackList.RemoveAt(0);
			}

			trackList.Add(trackFeature);
			trackData[name] = trackList;
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}
	}

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

			trackList.Add(trackFeature);
			trackData[name] = trackList;
		}
		else
		{
			Debug.Log("Unsupported data format");
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
					locationComponent.Position = new ArcGISPoint(trackFeature.predictedPoint.x, trackFeature.predictedPoint.y, trackFeature.predictedPoint.z, new ArcGISSpatialReference(FeatureSRWKID));

					gobjTrack.transform.localScale = Vector3.one * symbolScaleFactor;

					var rotator = locationComponent.Rotation;
					rotator.Heading = trackFeature.attributes.heading;
					locationComponent.Rotation = rotator;
				}
				else
				{
					GameObject clonePrefab = Instantiate(trackSymbolPrefab, this.transform);
					clonePrefab.name = trackFeature.attributes.name;
					clonePrefab.SetActive(true);
					clonePrefab.transform.localScale = new Vector3(symbolScaleFactor, symbolScaleFactor, symbolScaleFactor);
					var locationComponent = clonePrefab.GetComponent<ArcGISLocationComponent>();
					locationComponent.enabled = true;
					locationComponent.Position = new ArcGISPoint(trackFeature.geometry.x, trackFeature.geometry.y, trackFeature.geometry.z, new ArcGISSpatialReference(FeatureSRWKID));
					var rotator = locationComponent.Rotation;
					rotator.Pitch = 90.0;
					rotator.Heading = trackFeature.attributes.heading;
					locationComponent.Rotation = rotator;
				}

				// remove trackFeature if it is not updated within a specified time interval
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to create game object: " + ex.Message);
		}
	}
}
