// COPYRIGHT 1995-2022 ESRI
// TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
// Unpublished material - all rights reserved under the
// Copyright Laws of the United States and applicable international
// laws, treaties, and conventions.
//
// For additional information, contact:
// Attn: Contracts and Legal Department
// Environmental Systems Research Institute, Inc.
// 380 New York Street
// Redlands, California 92373
// USA
//
// email: legal@esri.com
using Esri.ArcGISMapsSDK.Utils.GeoCoord;
using Esri.ArcGISMapsSDK.Utils.Math;
using Esri.GameEngine.Geometry;
using Esri.GameEngine.MapView;
using Esri.HPFramework;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Components
{
	[DisallowMultipleComponent]
	[ExecuteAlways]
	[RequireComponent(typeof(Camera))]

	[AddComponentMenu("ArcGIS Maps SDK/ArcGIS Camera")]
	public class FlightSimArcGISCamera : MonoBehaviour
	{
		public bool UpdateClippingPlanes = true;

		private ArcGISMapComponent arcGISMapComponent;
		private Camera cameraComponent;

		private float vFov = 0;
		private uint viewportSizeX = 0;
		private uint viewportSizeY = 0;

		ArcGISPoint lastPosition = null;
		ArcGISRotation lastRotation;

		private void Initialize()
		{
			arcGISMapComponent = gameObject.GetComponentInParent<ArcGISMapComponent>();

			if (arcGISMapComponent == null)
			{
				Debug.LogError("Unable to find a parent ArcGISMapComponent.");

				enabled = false;
				return;
			}

			cameraComponent = GetComponent<Camera>();

#if UNITY_EDITOR
			// If the camera component is added after the map component this component
			// needs to determine if it should be active in editor mode
			if (!Application.isPlaying)
			{
				arcGISMapComponent.EnableMainCameraView(!(arcGISMapComponent.DataFetchWithSceneView));
			}
#endif
			arcGISMapComponent.CheckNumArcGISCameraComponentsEnabled();
		}

		private void OnEnable()
		{
			Initialize();
		}

		private void OnTransformParentChanged()
		{
			Initialize();

			Update();
		}

		private void PushPosition()
		{
			if (!arcGISMapComponent)
			{
				return;
			}

			var map = arcGISMapComponent.View.Map;
			var spatialReference = arcGISMapComponent.View.SpatialReference;

			if (map == null || spatialReference == null)
			{
				return;
			}

			var hpTransform = GetComponentInParent<HPTransform>();

			var worldPosition = hpTransform.UniversePosition;
			var worldRotation = hpTransform.UniverseRotation.ToQuaterniond();

			var newGeographicPosition = arcGISMapComponent.View.WorldToGeographic(worldPosition);

			if (!newGeographicPosition.IsValid)
			{
				return;
			}

			var newGeographicRotation = GeoUtils.FromCartesianRotation(worldPosition, worldRotation, spatialReference, map.MapType);

			if (lastPosition == null || lastRotation == null || !lastRotation.Equals(newGeographicRotation) || !lastPosition.Equals(newGeographicPosition))
			{
				lastPosition = newGeographicPosition;
				lastRotation = newGeographicRotation;

				arcGISMapComponent.View.Camera = new ArcGISCamera(newGeographicPosition, newGeographicRotation);
			}

			if (UpdateClippingPlanes)
			{
				var z = newGeographicPosition.Z;

				var near = Utils.FrustumHelpers.CalculateNearPlaneDistance(z, cameraComponent.fieldOfView, cameraComponent.aspect);
				var far = Math.Max(near, Utils.FrustumHelpers.CalculateFarPlaneDistance(z, map.MapType, spatialReference));

				cameraComponent.farClipPlane = (float)far;
				cameraComponent.nearClipPlane = (float)Math.Min(500000.0, near);
			}
		}

		private void PushViewportProperties()
		{
			if (!arcGISMapComponent)
			{
				return;
			}

			float newVFov = cameraComponent.fieldOfView;
			uint newViewportSizeX = (uint)cameraComponent.pixelWidth;
			uint newViewportSizeY = (uint)cameraComponent.pixelHeight;

			if (newVFov != vFov || newViewportSizeX != viewportSizeX || newViewportSizeY != viewportSizeY)
			{
				vFov = newVFov;
				viewportSizeX = newViewportSizeX;
				viewportSizeY = newViewportSizeY;

				if (viewportSizeX != 0)
				{
					var vFovRadians = vFov * MathUtils.DegreesToRadians;

					// Comes from aspect_ratio = width / height = tan (hfov / 2) / tan (vfov / 2)
					var hFovRadians = 2.0 * Math.Atan(Math.Tan(vFovRadians / 2.0) * cameraComponent.aspect);

					var hFov = hFovRadians * MathUtils.RadiansToDegrees;

					arcGISMapComponent.View.SetViewportProperties(viewportSizeX, viewportSizeY, (float)hFov, cameraComponent.fieldOfView, 1);
				}
			}
		}

		private void Update()
		{
			if (arcGISMapComponent && arcGISMapComponent.ShouldEditorComponentBeUpdated())
			{
				PushViewportProperties();

				PushPosition();
			}
		}

		public async Task<bool> ZoomToLayer(Esri.GameEngine.Layers.Base.ArcGISLayer layer)
		{
			if (layer == null)
			{
				Debug.LogWarning("Invalid layer passed to zoom to layer");
				return false;
			}

			var spatialReference = arcGISMapComponent.View.SpatialReference;

			if (spatialReference == null)
			{
				Debug.LogWarning("View must have a spatial reference to run zoom to layer");
				return false;
			}

			if (layer.LoadStatus != GameEngine.ArcGISLoadStatus.Loaded)
			{
				if (layer.LoadStatus == GameEngine.ArcGISLoadStatus.NotLoaded)
				{
					layer.Load();
				}
				else if (layer.LoadStatus != GameEngine.ArcGISLoadStatus.FailedToLoad)
				{
					layer.RetryLoad();
				}

				await Task.Run(() =>
				{
					while (layer.LoadStatus == GameEngine.ArcGISLoadStatus.Loading)
					{
					}
				});

				if (layer.LoadStatus == GameEngine.ArcGISLoadStatus.FailedToLoad)
				{
					Debug.LogWarning("Layer passed to zoom to layer must be loaded");
					return false;
				}
			}

			var layerExtent = layer.Extent;

			if (layerExtent == null)
			{
				Debug.LogWarning("The layer passed to zoom to layer does not have a valid extent");
				return false;
			}

			var cameraPosition = layerExtent.Center;
			var largeSide = Math.Max(layerExtent.Width, layerExtent.Height);

			// In global mode we can't see the entire layer if it is on a global scale,
			// so we just need to see the diameter of the planet
			if (arcGISMapComponent.ViewMode == Esri.GameEngine.Map.ArcGISMapType.Global)
			{
				var globeRadius = spatialReference.SpheroidData.MajorSemiAxis;
				largeSide = Math.Min(largeSide, 2 * globeRadius);
			}

			var radAngle = cameraComponent.fieldOfView * MathUtils.DegreesToRadians;
			var radHFOV = Math.Atan(Math.Tan(radAngle / 2));
			var zOffset = 0.5 * largeSide / Math.Tan(radHFOV);

			var newPosition = new ArcGISPoint(cameraPosition.X,
											  cameraPosition.Y,
											  cameraPosition.Z + zOffset,
											  cameraPosition.SpatialReference);
			var newRotation = new ArcGISRotation(0, 0, 0);

			ArcGISLocationComponent.SetPositionAndRotation(gameObject, newPosition, newRotation);

			return true;
		}
	}
}
