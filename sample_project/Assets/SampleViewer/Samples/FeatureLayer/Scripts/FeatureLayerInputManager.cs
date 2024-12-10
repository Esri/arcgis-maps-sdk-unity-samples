using System.Collections;
using System.Collections.Generic;
using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FeatureLayerInputManager : MonoBehaviour
{
    [SerializeField] private ArcGISMapComponent mapcomponent;
    private bool isLeftShiftPressed;
    [SerializeField] private GameObject propertiesCanvas;
    
    private InputActions inputActions;
    private TouchControls touchControls;
    
    private void Awake()
    {
        propertiesCanvas.SetActive(false);
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        inputActions = new InputActions();
#else
        touchControls = new TouchControls();
#endif
    }

    private void OnEnable()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        inputActions.Enable();
        inputActions.DrawingControls.LeftClick.started += OnLeftClickStart;
        inputActions.DrawingControls.LeftShift.performed += ctx => OnLeftShift(true);
        inputActions.DrawingControls.LeftShift.canceled += ctx => OnLeftShift(false);
#else
        touchControls.Enable();
        touchControls.Touch.TouchPress.started += OnTouchInputStarted;
#endif
    }

    private void OnDisable()
    {
#if !UNITY_IOS && !UNITY_ANDROID && !UNITY_VISIONOS
        inputActions.Disable();
        inputActions.DrawingControls.LeftClick.started -= OnLeftClickStart;
        inputActions.DrawingControls.LeftShift.performed -= ctx => OnLeftShift(true);
        inputActions.DrawingControls.LeftShift.canceled -= ctx => OnLeftShift(false);
#else
        touchControls.Disable();
        touchControls.Touch.TouchPress.started -= OnTouchInputStarted;
#endif
    }
    
    private void OnLeftShift(bool isPressed)
    {
        isLeftShiftPressed = isPressed;
    }

    private void OnLeftClickStart(InputAction.CallbackContext obj)
    {
        if (!isLeftShiftPressed)
        {
            return;
        }
        
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit))
        {
            var featureData = hit.collider.gameObject.GetComponent<FeatureData>();

            if (!featureData)
            {
                return;
            }

            if (!propertiesCanvas.activeInHierarchy)
            {
                propertiesCanvas.SetActive(true);
            }

            propertiesCanvas.GetComponent<ArcGISLocationComponent>().Position = new ArcGISPoint(featureData.Coordinates[0], featureData.Coordinates[1], 100, mapcomponent.OriginPosition.SpatialReference);
            propertiesCanvas.GetComponentInChildren<TextMeshProUGUI>().text = "Properties: \n";
            
            foreach (var property in featureData.Properties)
            {
                propertiesCanvas.GetComponentInChildren<TextMeshProUGUI>().text += property + '\n';
            }
        }
    }

    private void OnTouchInputStarted(InputAction.CallbackContext ctx)
    {
                
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(touchControls.Touch.TouchPosition.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out hit))
        {
            var featureData = hit.collider.gameObject.GetComponent<FeatureData>();

            if (!featureData)
            {
                return;
            }

            var output = "";
            foreach (var property in featureData.Properties)
            {
                output += property + '\n';
                Debug.Log(output);
            }
        }
    }
}
