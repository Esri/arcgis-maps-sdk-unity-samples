// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Google.XR.ARCoreExtensions;
using System.Collections;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SampleViewer.Samples.GeoSpatialAR.Scripts
{
    public class ArcGISGeospatialController : MonoBehaviour
    {
        private GeospatialPose location;
        public GeospatialPose cameraGeospatialPose;

        [SerializeField] private AREarthManager EarthManager;
        private bool _waitingForLocationService;
        private ArcGISMapComponent mapComponent;

        private bool _isReturning = false;
        public double _headingAccuracyThreshold = 25;
        public double yawAccuracy = 1000;

        public XROrigin XROrigin;
        [SerializeField] private GameObject precisionMarker;
        [SerializeField] private Image VPSStatus;
        [SerializeField] private GameObject welcomeScreen;
        [SerializeField] private Slider progressBar;

        private void Awake()
        {
            mapComponent = GetComponent<ArcGISMapComponent>();
        }

        private void OnEnable()
        {
            StartCoroutine(nameof(StartLocationService));
        }

        private void OnDisable()
        {
            Input.location.Stop();
        }

        void Start()
        {
            InvokeRepeating(nameof(SetOriginLocation), 0.0f, 2.0f);
            StartCoroutine(AvailabilityCheck());           
            Invoke(nameof(SetLocation), 1.0f);
        }

        void Update()
        {
            var earthTrackingState = EarthManager.EarthTrackingState;

            if (earthTrackingState == TrackingState.Tracking)
            {
                cameraGeospatialPose = EarthManager.CameraGeospatialPose;
                SetCamera(new ArcGISPoint(cameraGeospatialPose.Longitude, cameraGeospatialPose.Latitude, 5000,
                    ArcGISSpatialReference.WGS84()));

                SetInitialRotation();
            }
        }

        public void SetInitialRotation()
        {
            if (cameraGeospatialPose.OrientationYawAccuracy < _headingAccuracyThreshold && Math.Round(cameraGeospatialPose.OrientationYawAccuracy, 1) < yawAccuracy)
            {
                Vector3 originRotation = XROrigin.transform.rotation.eulerAngles;
                originRotation.y = cameraGeospatialPose.EunRotation.eulerAngles.y -
                                   Camera.main.transform.localEulerAngles.y;
                XROrigin.transform.rotation = Quaternion.Euler(originRotation);

                yawAccuracy = Math.Round(cameraGeospatialPose.OrientationYawAccuracy, 1);
            }

            progressBar.value = 1.0f;
            welcomeScreen.SetActive(false);
        }

        private void SetLocation()
        {
            location = EarthManager.CameraGeospatialPose;
            SetOrigin(new ArcGISPoint(location.Longitude, location.Latitude, 0, ArcGISSpatialReference.WGS84()));
            Debug.Log("Origin Updated to Lat: " + location.Latitude + " Long: " + location.Longitude);
        }

        private void SetOriginLocation()
        {
            var earthTrackingState = EarthManager.EarthTrackingState;
            progressBar.value += 0.1f;

            if (earthTrackingState == TrackingState.Tracking)
            {
                var cameraGeospatialPose = EarthManager.CameraGeospatialPose;
                XROrigin.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(cameraGeospatialPose.Longitude,
                    cameraGeospatialPose.Latitude, 0, ArcGISSpatialReference.WGS84());
                Camera.main.transform.position = Vector3.zero;

                CancelInvoke(nameof(SetOriginLocation));
                Debug.LogWarning("Cancelled");
            }
        }

        private void SetOrigin(ArcGISPoint OriginPoint)
        {
            mapComponent.OriginPosition = OriginPoint;
        }

        private void SetCamera(ArcGISPoint OriginPoint)
        {
            mapComponent.GetComponentInChildren<ArcGISCameraComponent>().gameObject.GetComponent<ArcGISLocationComponent>()
                .Position = OriginPoint;
        }

        private IEnumerator AvailabilityCheck()
        {
            if (ARSession.state == ARSessionState.None)
            {
                yield return ARSession.CheckAvailability();
            }

            // Waiting for ARSessionState.CheckingAvailability.
            yield return null;

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }

            // Waiting for ARSessionState.Installing.
            yield return null;
#if UNITY_ANDROID

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("Requesting camera permission.");
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitForSeconds(3.0f);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                // User has denied the request.
                Debug.LogWarning(
                    "Failed to get the camera permission. VPS availability check isn't available.");
                yield break;
            }
#endif

            while (_waitingForLocationService)
            {
                yield return null;
            }

            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarning(
                    "Location services aren't running. VPS availability check is not available.");
                yield break;
            }

            // Update event is executed before coroutines so it checks the latest error states.
            if (_isReturning)
            {
                yield break;
            }

            var location = Input.location.lastData;

            //var latitude = 34.05921;
            //var longitude = -117.19581;
            progressBar.value = 0.5f;
            var vpsAvailabilityPromise =
                AREarthManager.CheckVpsAvailabilityAsync(location.latitude, location.longitude); ;
            yield return vpsAvailabilityPromise;

            Debug.LogFormat("VPS Availability at ({0}, {1}): {2}",
                location.latitude, location.longitude, vpsAvailabilityPromise.Result);

            precisionMarker.SetActive(vpsAvailabilityPromise.Result != VpsAvailability.Available);
            VPSStatus.color = vpsAvailabilityPromise.Result != VpsAvailability.Available ? Color.red : Color.green;
        }

        private IEnumerator StartLocationService()
        {
            _waitingForLocationService = true;
    #if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                Debug.Log("Requesting the fine location permission.");
                Permission.RequestUserPermission(Permission.FineLocation);
                // Delay Proceeding to confirm that Location Services are enabled before proceeding
                yield return new WaitForSeconds(3.0f);
            }
    #endif

            if (!Input.location.isEnabledByUser)
            {
                Debug.Log("Location service is disabled by the user.");
                _waitingForLocationService = false;
                yield break;
            }

            Debug.Log("Starting location service.");
            Input.location.Start();

            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                yield return null;
            }

            _waitingForLocationService = false;
            if (Input.location.status != LocationServiceStatus.Running)
            {
                Debug.LogWarningFormat(
                    "Location service ended with {0} status.", Input.location.status);
                Input.location.Stop();
            }
        }
       
    }
}