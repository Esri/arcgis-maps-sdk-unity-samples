// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils.Math;
using Esri.GameEngine.Geometry;
using Esri.GameEngine.View;
using Esri.HPFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(HPTransform))]
public sealed class PointCloudAndroidCameraController : MonoBehaviour
{
	private const double MaxCameraHeight = 11000000.0;
	private const double MinCameraHeight = 1.8;
	private const double MaxCameraLatitude = 85.0;

	[SerializeField] private bool ignoreUI;
	[SerializeField] private float pitchSpeed = 90f;
	[SerializeField] private float pinchZoomSpeed = 0.006f;
	[SerializeField] private float rotationSpeed = 1.4f;

	private Camera activeCamera;
	private Behaviour builtInController;
	private double3 lastCartesianPoint = double3.zero;
	private double lastDotVC;
	private ArcGISPoint lastArcGISPoint = new ArcGISPoint(0, 0, 0, ArcGISSpatialReference.WGS84());
	private bool firstDragStep = true;
	private HPTransform hpTransform;
	private int lastTouchCount;
	private float lastTouchLogTime;
	private ArcGISMapComponent mapComponent;
	private bool restoreBuiltInController;

	private double3 Position
	{
		get => hpTransform.UniversePosition;
		set => hpTransform.UniversePosition = value;
	}

	private quaternion Rotation
	{
		get => hpTransform.UniverseRotation;
		set => hpTransform.UniverseRotation = value;
	}

	private void Awake()
	{
		activeCamera = GetComponent<Camera>();
		hpTransform = GetComponent<HPTransform>();
		builtInController = GetComponents<Behaviour>().FirstOrDefault(component => component && component.GetType().Name == "ArcGISCameraControllerComponent");
		Debug.LogFormat("PointCloudAndroidCameraController awake. camera={0}, hpTransform={1}, builtInController={2}", activeCamera != null, hpTransform != null, builtInController != null);
	}

	private void OnEnable()
	{
		mapComponent = GetComponentInParent<ArcGISMapComponent>();
		EnhancedTouchSupport.Enable();
		Debug.LogFormat("PointCloudAndroidCameraController enabled. mobile={0}, mapComponent={1}", Application.isMobilePlatform, mapComponent != null);

		if (Application.isMobilePlatform && builtInController && builtInController.enabled)
		{
			restoreBuiltInController = true;
			builtInController.enabled = false;
		}
	}

	private void OnDisable()
	{
		if (restoreBuiltInController && builtInController)
		{
			builtInController.enabled = true;
		}

		restoreBuiltInController = false;
		EnhancedTouchSupport.Disable();
	}

	private void Update()
	{
		if (!Application.isMobilePlatform || !mapComponent || !mapComponent.HasSpatialReference())
		{
			firstDragStep = true;
			return;
		}

		var touches = Touch.activeTouches;
		if (touches.Count > 0 && Time.unscaledTime - lastTouchLogTime > 1f)
		{
			Debug.LogFormat("PointCloudAndroidCameraController touches={0}, overUI={1}", touches.Count, TouchOverUI(touches[0].screenPosition));
			lastTouchLogTime = Time.unscaledTime;
		}

		if (touches.Count != lastTouchCount)
		{
			firstDragStep = true;
			lastTouchCount = touches.Count;
		}

		if (touches.Count == 1)
		{
			HandlePan(touches[0]);
		}
		else if (touches.Count >= 2)
		{
			HandleTwoFingerGesture(touches[0], touches[1]);
		}
		else
		{
			firstDragStep = true;
			lastTouchCount = 0;
		}
	}

	private void HandlePan(Touch touch)
	{
		if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
		{
			firstDragStep = true;
			return;
		}

		if (touch.phase != UnityEngine.InputSystem.TouchPhase.Moved || TouchOverUI(touch.screenPosition))
		{
			firstDragStep = true;
			return;
		}

		var cartesianPosition = Position;
		var cartesianRotation = Rotation;

		if (mapComponent.MapType == Esri.GameEngine.Map.ArcGISMapType.Global)
		{
			GlobalDragging(ref cartesianPosition, ref cartesianRotation, touch.screenPosition);
		}
		else
		{
			LocalDragging(ref cartesianPosition, touch.screenPosition);
		}

		Position = cartesianPosition;
		Rotation = cartesianRotation;
	}

	private void HandleTwoFingerGesture(Touch touch0, Touch touch1)
	{
		if (TouchOverUI(touch0.screenPosition) || TouchOverUI(touch1.screenPosition))
		{
			firstDragStep = true;
			return;
		}

		var previous0 = touch0.screenPosition - touch0.delta;
		var previous1 = touch1.screenPosition - touch1.delta;
		var previousVector = previous1 - previous0;
		var currentVector = touch1.screenPosition - touch0.screenPosition;

		if (previousVector.sqrMagnitude <= Mathf.Epsilon || currentVector.sqrMagnitude <= Mathf.Epsilon)
		{
			return;
		}

		var midpoint = (touch0.screenPosition + touch1.screenPosition) * 0.5f;
		var previousMidpoint = (previous0 + previous1) * 0.5f;
		var cartesianPosition = Position;
		var cartesianRotation = Rotation;

		var midpointDelta = midpoint - previousMidpoint;
		if (!Mathf.Approximately(midpointDelta.y, 0f))
		{
			RotatePitch(ref cartesianRotation, midpointDelta.y);
		}

		var pinchDelta = currentVector.magnitude - previousVector.magnitude;
		if (!Mathf.Approximately(pinchDelta, 0f))
		{
			var altitude = mapComponent.View.AltitudeAtCartesianPosition(cartesianPosition);
			var zoomDistance = Math.Max(1.0, altitude - MinCameraHeight) * pinchZoomSpeed * pinchDelta;
			MoveCamera(ref cartesianPosition, ref cartesianRotation, GetRayCastDirection(midpoint) * zoomDistance);
		}

		var rotationDelta = Vector2.SignedAngle(previousVector, currentVector);
		if (!Mathf.Approximately(rotationDelta, 0f))
		{
			RotateAround(ref cartesianPosition, ref cartesianRotation, rotationDelta);
		}

		Position = cartesianPosition;
		Rotation = cartesianRotation;
		firstDragStep = false;
	}

	private void LocalDragging(ref double3 cartesianPosition, Vector2 screenPosition)
	{
		var worldRayDir = GetRayCastDirection(screenPosition);
		var isIntersected = Esri.ArcGISMapsSDK.Utils.Math.Geometry.RayPlaneIntersection(cartesianPosition, worldRayDir, double3.zero, math.up(), out var intersection);

		if (isIntersected && intersection >= 0)
		{
			var cartesianCoord = cartesianPosition + worldRayDir * intersection;
			var delta = firstDragStep ? double3.zero : lastCartesianPoint - cartesianCoord;

			lastCartesianPoint = cartesianCoord + delta;
			cartesianPosition += delta;
			firstDragStep = false;
		}
	}

	private void GlobalDragging(ref double3 cartesianPosition, ref quaternion cartesianRotation, Vector2 screenPosition)
	{
		var spheroidData = mapComponent.View.SpatialReference.SpheroidData;
		var worldRayDir = GetRayCastDirection(screenPosition);
		var isIntersected = Esri.ArcGISMapsSDK.Utils.Math.Geometry.RayEllipsoidIntersection(spheroidData, cartesianPosition, worldRayDir, 0, out var intersection);

		if (isIntersected && intersection >= 0)
		{
			var oldENUReference = mapComponent.View.GetENUReference(cartesianPosition);
			var geoPosition = mapComponent.View.WorldToGeographic(cartesianPosition);
			var cartesianCoord = cartesianPosition + worldRayDir * intersection;
			var currentGeoPosition = mapComponent.View.WorldToGeographic(cartesianCoord);
			var visibleHemisphereDir = math.normalize(mapComponent.View.GeographicToWorld(new ArcGISPoint(geoPosition.X, 0, 0, geoPosition.SpatialReference)));
			var dotVC = math.dot(cartesianCoord, visibleHemisphereDir);

			lastDotVC = firstDragStep ? dotVC : lastDotVC;

			var deltaX = firstDragStep ? 0 : lastArcGISPoint.X - currentGeoPosition.X;
			var deltaY = firstDragStep ? 0 : lastArcGISPoint.Y - currentGeoPosition.Y;
			deltaY = Math.Sign(dotVC) != Math.Sign(lastDotVC) ? 0 : deltaY;

			lastArcGISPoint = new ArcGISPoint(currentGeoPosition.X + deltaX, currentGeoPosition.Y + deltaY, lastArcGISPoint.Z, lastArcGISPoint.SpatialReference);

			var y = geoPosition.Y + (dotVC <= 0 ? -deltaY : deltaY);
			y = Math.Abs(y) < MaxCameraLatitude ? y : (y > 0 ? MaxCameraLatitude : -MaxCameraLatitude);
			geoPosition = new ArcGISPoint(geoPosition.X + deltaX, y, geoPosition.Z, geoPosition.SpatialReference);
			cartesianPosition = mapComponent.View.GeographicToWorld(geoPosition);

			var newENUReference = mapComponent.View.GetENUReference(cartesianPosition);
			cartesianRotation = math.mul(math.inverse(oldENUReference.GetRotation()), cartesianRotation);
			cartesianRotation = math.mul(newENUReference.GetRotation(), cartesianRotation);

			firstDragStep = false;
			lastDotVC = dotVC;
		}
	}

	private void MoveCamera(ref double3 cartesianPosition, ref quaternion cartesianRotation, double3 movement)
	{
		var distance = math.length(movement);
		if (distance <= double.Epsilon)
		{
			return;
		}

		var moveDirection = movement / distance;

		if (mapComponent.MapType == Esri.GameEngine.Map.ArcGISMapType.Global)
		{
			var nextArcGISPoint = mapComponent.View.WorldToGeographic(moveDirection + cartesianPosition);

			if (nextArcGISPoint.Z > MaxCameraHeight)
			{
				var point = new ArcGISPoint(nextArcGISPoint.X, nextArcGISPoint.Y, MaxCameraHeight, nextArcGISPoint.SpatialReference);
				cartesianPosition = mapComponent.View.GeographicToWorld(point);
			}
			else if (nextArcGISPoint.Z < MinCameraHeight)
			{
				var point = new ArcGISPoint(nextArcGISPoint.X, nextArcGISPoint.Y, MinCameraHeight, nextArcGISPoint.SpatialReference);
				cartesianPosition = mapComponent.View.GeographicToWorld(point);
			}
			else
			{
				cartesianPosition += moveDirection * distance;
			}

			var oldENUReference = mapComponent.View.GetENUReference(Position);
			var newENUReference = mapComponent.View.GetENUReference(cartesianPosition);
			cartesianRotation = math.mul(math.inverse(oldENUReference.GetRotation()), cartesianRotation);
			cartesianRotation = math.mul(newENUReference.GetRotation(), cartesianRotation);
		}
		else
		{
			cartesianPosition += movement;
		}
	}

	private void RotateAround(ref double3 cartesianPosition, ref quaternion cartesianRotation, float angle)
	{
		var enuReference = mapComponent.View.GetENUReference(cartesianPosition).ToMatrix4x4();
		var rotation = Quaternion.AngleAxis(angle * rotationSpeed, enuReference.GetColumn(1));
		cartesianRotation = rotation * cartesianRotation;
	}

	private void RotatePitch(ref quaternion cartesianRotation, float screenDeltaY)
	{
		var cameraRotation = Matrix4x4.Rotate(cartesianRotation);
		var right = cameraRotation.GetColumn(0);
		var pitchAngle = -(screenDeltaY / Mathf.Max(1f, Screen.height)) * pitchSpeed;
		var rotation = Quaternion.AngleAxis(pitchAngle, right);
		cartesianRotation = rotation * cartesianRotation;
	}

	private double3 GetRayCastDirection(Vector2 screenPosition)
	{
		var forward = hpTransform.Forward.ToDouble3();
		var right = hpTransform.Right.ToDouble3();
		var up = hpTransform.Up.ToDouble3();

		var view = new double4x4
		(
			math.double4(right, 0),
			math.double4(up, 0),
			math.double4(forward, 0),
			math.double4(double3.zero, 1)
		);

		var projection = activeCamera.projectionMatrix.inverse.ToDouble4x4();
		projection.c2.w *= -1;
		projection.c3.z *= -1;

		var ndcCoord = new double3(2.0 * (screenPosition.x / Screen.width) - 1.0, 2.0 * (screenPosition.y / Screen.height) - 1.0, 1);
		var viewRayDir = math.normalize(projection.HomogeneousTransformPoint(ndcCoord));
		return view.HomogeneousTransformVector(viewRayDir);
	}

	private bool TouchOverUI(Vector2 screenPosition)
	{
		if (ignoreUI || EventSystem.current == null)
		{
			return false;
		}

		var eventData = new PointerEventData(EventSystem.current)
		{
			position = screenPosition
		};
		var results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, results);
		return results.Any(IsBlockingUIHit);
	}

	private static bool IsBlockingUIHit(RaycastResult result)
	{
		var selectable = result.gameObject.GetComponentInParent<Selectable>();
		if (selectable && selectable.IsActive() && selectable.IsInteractable())
		{
			return true;
		}

		var scrollRect = result.gameObject.GetComponentInParent<ScrollRect>();
		if (scrollRect && scrollRect.IsActive() && scrollRect.enabled)
		{
			return true;
		}

		var inputField = result.gameObject.GetComponentInParent<InputField>();
		if (inputField && inputField.IsActive() && inputField.IsInteractable())
		{
			return true;
		}

		var graphic = result.gameObject.GetComponent<Graphic>();
		if (!graphic || !graphic.raycastTarget || !graphic.canvasRenderer || graphic.canvasRenderer.GetAlpha() <= 0.01f)
		{
			return false;
		}

		var rectTransform = result.gameObject.transform as RectTransform;
		if (!rectTransform)
		{
			return false;
		}

		var rect = rectTransform.rect;
		var screenArea = Mathf.Max(1f, Screen.width * Screen.height);
		var rectArea = Mathf.Abs(rect.width * rect.height);

		return rectArea / screenArea < 0.8f;
	}
}
