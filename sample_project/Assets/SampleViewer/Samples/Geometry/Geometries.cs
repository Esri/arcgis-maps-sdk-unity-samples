// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Esri.ArcGISMapsSDK.Samples.Components;

public class Geometries : MonoBehaviour
{
	private const float RaycastHeight = 5000f;
	private const float LineWidth = 5f;

	public GameObject Line;
	public TMP_Text result;
	public GameObject LineMarker;
	public GameObject InterpolationMarker;
	public float InterpolationInterval = 100;
	public Button[] ModeButtons;
	public Button[] UnitButtons;
	public Button ClearButton;
	[SerializeField] float MarkerHeight = 200f;
	private ArcGISMapComponent arcGISMapComponent;
	private List<GameObject> featurePoints = new List<GameObject>();
	private List<GameObject> lastToStartInterpolationPoints = new List<GameObject>();
	private Stack<GameObject> stops = new Stack<GameObject>();
	private double3 lastRootPosition;
	private double geodeticDistance = 0;
	private double polygonArea = 0;
	private double envelopeArea = 0;
	private LineRenderer lineRenderer;
	private ArcGISLinearUnit currentLinearUnit = new ArcGISLinearUnit(ArcGISLinearUnitId.Miles);
	private ArcGISAreaUnit currentAreaUnit = new ArcGISAreaUnit(ArcGISAreaUnitId.SquareMiles);
	private string unitText;
	private bool isPolylineMode = true;
	private bool isPolygonMode = false;
	private bool isEnvelopeMode = false;
	private bool isDragging = false;
	public GameObject EnvelopeMarker = new GameObject();
	private ArcGISPoint startPoint;
	private ArcGISCameraControllerComponent arcGISCameraControllerComponent;

	private void Start()
	{
		arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
		arcGISCameraControllerComponent = FindObjectOfType<ArcGISCameraControllerComponent>();
		arcGISCameraControllerComponent.IgnoreUI = true;
		arcGISCameraControllerComponent.EnableLeftDragging = false;

		lineRenderer = Line.GetComponent<LineRenderer>();
		lineRenderer.widthMultiplier = LineWidth;
		lastRootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;

		ClearButton.onClick.AddListener(delegate
		{
			ClearLine();
		});

		ModeButtons[0].interactable = false;
		UnitButtons[0].interactable = false;
		
	}

	private void Update()
	{
		if (isEnvelopeMode)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				if (Input.GetMouseButtonDown(0))
				{
					ClearLine();
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

					if (Physics.Raycast(ray, out hit))
					{
						hit.point += new Vector3(0, MarkerHeight, 0);
						startPoint = arcGISMapComponent.EngineToGeographic(hit.point);

						isDragging = true;
					}
				}
				else if (Input.GetMouseButtonUp(0) && isDragging)
				{
					isDragging = false;
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

					if (Physics.Raycast(ray, out hit))
					{
						hit.point += new Vector3(0, MarkerHeight, 0);
						var endPoint = arcGISMapComponent.EngineToGeographic(hit.point);
						CreateAndCalculateEnvelope(startPoint, endPoint);
					}
				}
			}
		}
		else
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0))
			{
				if (isPolygonMode)
				{
					//clear interpolation points of last segments 
					foreach (var point in lastToStartInterpolationPoints)
					{
						Destroy(point);
						continue;
					}
					lastToStartInterpolationPoints.Clear();
				}

				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit))
				{
					hit.point += new Vector3(0, MarkerHeight, 0);
					var lineMarker = Instantiate(LineMarker, hit.point, Quaternion.identity, arcGISMapComponent.transform);
					var thisPoint = arcGISMapComponent.EngineToGeographic(hit.point);

					var location = lineMarker.GetComponent<ArcGISLocationComponent>();
					location.enabled = true;
					location.Position = thisPoint;
					location.Rotation = new ArcGISRotation(0, 90, 0);

					if (stops.Count > 0)
					{
						GameObject lastStop = stops.Peek();
						var lastPoint = lastStop.GetComponent<ArcGISLocationComponent>().Position;
						if(isPolylineMode)
						{
							// Calculate distance from last point to this point.
							geodeticDistance += ArcGISGeometryEngine.DistanceGeodetic(lastPoint, thisPoint, currentLinearUnit, new ArcGISAngularUnit(ArcGISAngularUnitId.Degrees), ArcGISGeodeticCurveType.Geodesic).Distance;
							UpdateDisplay();
						}
						featurePoints.Add(lastStop);
						// Interpolate middle points between last point and this point.
						Interpolate(lastStop, lineMarker, featurePoints);
						featurePoints.Add(lineMarker);
					}

					// Add this point to stops and also to feature points where stop is user-drawed, and feature points is a collection of user-drawed and interpolated.
					stops.Push(lineMarker);

					if (isPolygonMode)
					{
						Interpolate(lineMarker, featurePoints[0], lastToStartInterpolationPoints);
						CreateandCalculatePolygon();
					}
					
					RenderLine(ref featurePoints);
					RebaseLine();
				}
			}
		}

	}
	
	private void CreateAndCalculateEnvelope(ArcGISPoint start, ArcGISPoint end)
	{
		var spatialReference = new ArcGISSpatialReference(3857);

		var southwestPoint = new ArcGISPoint(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), spatialReference);
		var northeastPoint = new ArcGISPoint(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), spatialReference);

		var envelope = new ArcGISEnvelope(southwestPoint, northeastPoint);

		VisualizeEnvelope(envelope);

		envelopeArea = ArcGISGeometryEngine.AreaGeodetic(envelope, currentAreaUnit, ArcGISGeodeticCurveType.Geodesic);
		UpdateDisplay();
	}

	private void VisualizeEnvelope(ArcGISEnvelope envelope)
	{
		var bottomLeft = arcGISMapComponent.GeographicToEngine(new ArcGISPoint(envelope.XMin, envelope.YMin, envelope.SpatialReference));
		var bottomRight = arcGISMapComponent.GeographicToEngine(new ArcGISPoint(envelope.XMax, envelope.YMin, envelope.SpatialReference));
		var topLeft = arcGISMapComponent.GeographicToEngine(new ArcGISPoint(envelope.XMin, envelope.YMax, envelope.SpatialReference));
		var topRight = arcGISMapComponent.GeographicToEngine(new ArcGISPoint(envelope.XMax, envelope.YMax, envelope.SpatialReference));

		bottomLeft.y += MarkerHeight;
		bottomRight.y += MarkerHeight;
		topLeft.y += MarkerHeight;
		topRight.y += MarkerHeight;

		var bottomLeftMarker = Instantiate(LineMarker, bottomLeft, Quaternion.identity, arcGISMapComponent.transform);
		var bottomRightMarker = Instantiate(LineMarker, bottomRight, Quaternion.identity, arcGISMapComponent.transform);
		var topLeftMarker = Instantiate(LineMarker, topLeft, Quaternion.identity, arcGISMapComponent.transform);
		var topRightMarker = Instantiate(LineMarker, topRight, Quaternion.identity, arcGISMapComponent.transform);		
		
		SetSurfacePlacement(bottomLeftMarker, MarkerHeight);
		SetSurfacePlacement(bottomRightMarker, MarkerHeight);
		SetSurfacePlacement(topLeftMarker, MarkerHeight);
		SetSurfacePlacement(topRightMarker, MarkerHeight);
		
		SetElevation(bottomLeftMarker);
		SetElevation(bottomRightMarker);
		SetElevation(topLeftMarker);
		SetElevation(topRightMarker);

		stops.Push(topLeftMarker);
		stops.Push(topRightMarker);
		stops.Push(bottomRightMarker);
		stops.Push(bottomLeftMarker);

		featurePoints.Add(topLeftMarker);
		Interpolate(topLeftMarker, topRightMarker, featurePoints);
		featurePoints.Add(topRightMarker);
		Interpolate(topRightMarker, bottomRightMarker, featurePoints);
		featurePoints.Add(bottomRightMarker);
		Interpolate(bottomRightMarker, bottomLeftMarker, featurePoints);
		featurePoints.Add(bottomLeftMarker);
		Interpolate(bottomLeftMarker, topLeftMarker, featurePoints);

		RenderLine(ref featurePoints);
		RebaseLine();
	
		featurePoints.Add(topRightMarker);
	}

	private void SetSurfacePlacement(GameObject marker, double offset)
	{
		var locationComponent = marker.GetComponent<ArcGISLocationComponent>();
		locationComponent.SurfacePlacementMode = ArcGISSurfacePlacementMode.RelativeToGround;
		locationComponent.SurfacePlacementOffset = offset;
	}

	private void CreateandCalculatePolygon()
	{
		var spatialReference = new ArcGISSpatialReference(3857);
		var polygonBuilder = new ArcGISPolygonBuilder(spatialReference);

		foreach (var stop in stops)
		{
			var location = stop.GetComponent<ArcGISLocationComponent>().Position;
			polygonBuilder.AddPoint(location);
		}

		var polygon = polygonBuilder.ToGeometry();

		polygonArea = ArcGISGeometryEngine.AreaGeodetic(polygon, currentAreaUnit, ArcGISGeodeticCurveType.Geodesic);
		UpdateDisplay();
	}
	private void Interpolate(GameObject start, GameObject end, List<GameObject> featurePoints)
	{

		float lengthOfLine = Vector3.Distance(start.transform.position, end.transform.position);
		float n = Mathf.Floor(lengthOfLine / InterpolationInterval);
		double dx = (end.transform.position.x - start.transform.position.x) / n;
		double dy = (end.transform.position.y - start.transform.position.y) / n;
		double dz = (end.transform.position.z - start.transform.position.z) / n;

		var previousInterpolation = start.transform.position;

		// Calculate n-1 intepolation points/n-1 segments because the last segment is already created by the end point.
		for (int i = 0; i < n - 1; i++)
		{
			GameObject nextInterpolation = Instantiate(InterpolationMarker, arcGISMapComponent.transform);

			// Calculate transform of nextInterpolation point.
			float nextInterpolationX = previousInterpolation.x + (float)dx;
			float nextInterpolationY = previousInterpolation.y + (float)dy;
			float nextInterpolationZ = previousInterpolation.z + (float)dz;
			nextInterpolation.transform.position = new Vector3(nextInterpolationX, nextInterpolationY, nextInterpolationZ);

			// Set default location component of nextInterpolation point.
			nextInterpolation.GetComponent<ArcGISLocationComponent>().enabled = true;
			nextInterpolation.GetComponent<ArcGISLocationComponent>().Rotation = new ArcGISRotation(0, 90, 0);

			var location = nextInterpolation.GetComponent<ArcGISLocationComponent>();
			location.Position = arcGISMapComponent.EngineToGeographic(nextInterpolation.transform.position);

			featurePoints.Add(nextInterpolation);
			previousInterpolation = nextInterpolation.transform.position;
		}
	}

	// Set height for point transform and location component.
	private void SetElevation(GameObject stop)
	{
		// Start the raycast in the air at an arbitrary to ensure it is above the ground.
		var position = stop.transform.position;
		var raycastStart = new Vector3(position.x, position.y + RaycastHeight, position.z);
		if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo))
		{
			var location = stop.GetComponent<ArcGISLocationComponent>();
			location.Position = arcGISMapComponent.EngineToGeographic(hitInfo.point);
		}
	}

	private void RenderLine(ref List<GameObject> featurePoints)
	{
		var allPoints = new List<Vector3>();

		foreach (var point in featurePoints)
		{
			if (point.transform.position.Equals(Vector3.zero))
			{
				Destroy(point);
				continue;
			}
			allPoints.Add(point.transform.position);
		}

		if (isPolygonMode)
		{
			//add the first point to line renderer so that polygon can be closed
			allPoints.Add(allPoints[0]);
			foreach (var point in lastToStartInterpolationPoints)
			{
				if (point.transform.position.Equals(Vector3.zero))
				{
					Destroy(point);
					continue;
				}
				allPoints.Add(point.transform.position);
			}
		}

		lineRenderer.positionCount = allPoints.Count;
		lineRenderer.SetPositions(allPoints.ToArray());
	}

	public void ClearLine()
	{
		foreach (var stop in featurePoints)
		{
			Destroy(stop);
		}

		foreach (var point in lastToStartInterpolationPoints)
		{
			Destroy(point);
		}

		featurePoints.Clear();
		stops.Clear();

		geodeticDistance = 0;
		polygonArea = 0;
		envelopeArea = 0;
		UpdateDisplay();

		if (lineRenderer)
		{
			lineRenderer.positionCount = 0;
		}
	}

	private void RebaseLine()
	{
		var rootPosition = arcGISMapComponent.GetComponent<HPRoot>().RootUniversePosition;
		var delta = (lastRootPosition - rootPosition).ToVector3();
		if (delta.magnitude > 1) // 1km
		{
			if (lineRenderer != null)
			{
				Vector3[] points = new Vector3[lineRenderer.positionCount];
				lineRenderer.GetPositions(points);
				for (int i = 0; i < points.Length; i++)
				{
					points[i] += delta;
				}
				lineRenderer.SetPositions(points);
			}
			lastRootPosition = rootPosition;
		}
	}
	public void SetPolylineMode()
	{
		ClearLine();
		isPolylineMode = true;
		isPolygonMode = false;
		isEnvelopeMode = false;
		ModeButtons[0].interactable = false;
		ModeButtons[1].interactable = true;
		ModeButtons[2].interactable = true;

		UnitButtons[0].GetComponentInChildren<TMP_Text>().text = "mi";
		UnitButtons[1].GetComponentInChildren<TMP_Text>().text = "ft";
		UnitButtons[2].GetComponentInChildren<TMP_Text>().text = "m";
		UnitButtons[3].GetComponentInChildren<TMP_Text>().text = "km";
	}

	public void SetPolygonMode()
	{
		ClearLine();
		isPolygonMode = true;
		isEnvelopeMode = false;
		isPolylineMode = false;
		ModeButtons[0].interactable = true;
		ModeButtons[1].interactable = false;
		ModeButtons[2].interactable = true;

		UnitButtons[0].GetComponentInChildren<TMP_Text>().text = "mi²";
		UnitButtons[1].GetComponentInChildren<TMP_Text>().text = "ft²";
		UnitButtons[2].GetComponentInChildren<TMP_Text>().text = "m²";
		UnitButtons[3].GetComponentInChildren<TMP_Text>().text = "km²";
	}
	public void SetEnvelopeMode()
	{
		ClearLine();
		isEnvelopeMode = true;
		isPolygonMode= false;
		isPolylineMode = false;
		ModeButtons[0].interactable = true;
		ModeButtons[1].interactable = true;
		ModeButtons[2].interactable = false;

		UnitButtons[0].GetComponentInChildren<TMP_Text>().text = "mi²";
		UnitButtons[1].GetComponentInChildren<TMP_Text>().text = "ft²";
		UnitButtons[2].GetComponentInChildren<TMP_Text>().text = "m²";
		UnitButtons[3].GetComponentInChildren<TMP_Text>().text = "km²";
	}

	private void UnitChanged()
	{
		if(isPolylineMode)
		{
			var newLinearUnit = new ArcGISLinearUnit(Enum.Parse<ArcGISLinearUnitId>(unitText));
			geodeticDistance = currentLinearUnit.ConvertTo(newLinearUnit, geodeticDistance);
			currentLinearUnit = newLinearUnit;
			UpdateDisplay();
		}
		else 
		{
			var newAreaUnit = new ArcGISAreaUnit(Enum.Parse<ArcGISAreaUnitId>(unitText));
			polygonArea = currentAreaUnit.ConvertTo(newAreaUnit, polygonArea);
			envelopeArea = currentAreaUnit.ConvertTo(newAreaUnit, envelopeArea);
			currentAreaUnit = newAreaUnit;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		if (isPolylineMode)
		{
			result.text = $"{Math.Round(geodeticDistance, 3)}";
		}
		else if (isPolygonMode)
		{
			result.text = $"{Math.Round(polygonArea, 3)}";
		}
		else if (isEnvelopeMode)
		{
			result.text = $"{Math.Round(envelopeArea, 3)}";
		}
	}
	public void SetUnitText(string text)
	{
		if(isPolylineMode)
		{
			unitText = text;
		}
		else 
		{
			unitText = "Square" + text;
		}
		
	}

	public void UnitButtonOnClick()
	{
		UnitChanged();
	}

	public void OnUnitButtonClicked(Button clickedButton)
	{
		int btnIndex = System.Array.IndexOf(UnitButtons, clickedButton);

		if (btnIndex == -1)
		{
			return;
		}

		foreach (Button btn in UnitButtons)
		{
			btn.interactable = true;
		}

		clickedButton.interactable = false;
	}
}