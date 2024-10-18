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

public class ArcGISGeospatialController : MonoBehaviour
{
    public GeospatialPose location;
    public GeospatialPose cameraGeospatialPose;
    
    private AREarthManager EarthManager;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private Button locationButton;
    private bool _waitingForLocationService;
    private ArcGISMapComponent mapComponent;
    
    private const double _headingAccuracyThreshold = 25;
    private double yawAccuracy = 1000;

    public XROrigin XROrigin;

    private void Awake()
    {
        mapComponent = GetComponent<ArcGISMapComponent>();
    }

    private void OnEnable()
    {
        StartCoroutine("StartLocationService");
        StartCoroutine("AvailabilityCheck");
    }

    private void OnDisable()
    {
        Input.location.Stop();
    }

    void Start()
    {
        EarthManager = new AREarthManager();
        InvokeRepeating(nameof(SetOriginLocation), 0.0f, 2.0f);

        locationButton.onClick.AddListener(delegate
        {
            location = EarthManager.CameraGeospatialPose;
            SetOrigin(new ArcGISPoint(location.Longitude, location.Latitude, 0, ArcGISSpatialReference.WGS84()));
            locationText.text = "Origin: " + location.Longitude + ", " + location.Latitude + ", " + location.Altitude;
        });
    }

    void Update()
    {
        var earthTrackingState = EarthManager.EarthTrackingState;
        
        if (earthTrackingState == TrackingState.Tracking)
        {
            cameraGeospatialPose = EarthManager.CameraGeospatialPose;
            SetCamera(new ArcGISPoint(cameraGeospatialPose.Longitude, cameraGeospatialPose.Latitude, 100, ArcGISSpatialReference.WGS84()));
            
            if (cameraGeospatialPose.OrientationYawAccuracy < _headingAccuracyThreshold && Math.Round(cameraGeospatialPose.OrientationYawAccuracy, 1) < yawAccuracy)
            {
                Vector3 originRotation = XROrigin.transform.rotation.eulerAngles;
                originRotation.y = cameraGeospatialPose.EunRotation.eulerAngles.y - Camera.main.transform.localEulerAngles.y;
                XROrigin.transform.rotation = Quaternion.Euler(originRotation);

                yawAccuracy = Math.Round(cameraGeospatialPose.OrientationYawAccuracy, 1);
            }
        }
    }

    private void SetOriginLocation()
    {
        var earthTrackingState = EarthManager.EarthTrackingState;
        
        if (earthTrackingState == TrackingState.Tracking)
        {
            var cameraGeospatialPose = EarthManager.CameraGeospatialPose;
            XROrigin.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(cameraGeospatialPose.Longitude, cameraGeospatialPose.Latitude, ArcGISSpatialReference.WGS84());
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
        mapComponent.GetComponentInChildren<ArcGISCameraComponent>().gameObject.GetComponent<ArcGISLocationComponent>().Position = OriginPoint;
    }
    
    private IEnumerator AvailabilityCheck()
    {
        if (ARSession.state == ARSessionState.None)
        {
            yield return ARSession.CheckAvailability();
        }

        yield return null;

        if (ARSession.state == ARSessionState.NeedsInstall)
        {
            yield return ARSession.Install();
        }

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

        var location = Input.location.lastData;
        var vpsAvailabilityPromise =
            AREarthManager.CheckVpsAvailabilityAsync(location.latitude, location.longitude); ;
        yield return vpsAvailabilityPromise;

        Debug.LogFormat("VPS Availability at ({0}, {1}): {2}",
            location.latitude, location.longitude, vpsAvailabilityPromise.Result);
    }

    private IEnumerator StartLocationService()
    {
        _waitingForLocationService = true;
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.Log("Requesting the fine location permission.");
            Permission.RequestUserPermission(Permission.FineLocation);
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
