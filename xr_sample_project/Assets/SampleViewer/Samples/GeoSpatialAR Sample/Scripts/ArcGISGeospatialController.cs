using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Google.XR.ARCoreExtensions;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ArcGISGeospatialController : MonoBehaviour
{
    private AREarthManager EarthManager;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private Button locationButton;
    private bool _waitingForLocationService;
    private ArcGISMapComponent mapComponent;

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

        locationButton.onClick.AddListener(delegate
        {
            var location = EarthManager.CameraGeospatialPose;
            SetOrigin(new ArcGISPoint(location.Longitude, location.Latitude, 0, ArcGISSpatialReference.WGS84()));
            locationText.text = "Origin: " + mapComponent.OriginPosition.X + 
            ", " + mapComponent.OriginPosition.Y + 
            ", " + mapComponent.OriginPosition.Z;
        });
    }

    void Update()
    {
        var earthTrackingState = EarthManager.EarthTrackingState;

        if (earthTrackingState == TrackingState.Tracking)
        {
            var cameraGeospatialPose = EarthManager.CameraGeospatialPose;
        }
    }


    private void SetOrigin(ArcGISPoint OriginPoint)
    {
        mapComponent.OriginPosition = OriginPoint;
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
