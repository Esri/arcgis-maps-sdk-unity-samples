using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private ARTouchControls touchControls;
    [SerializeField] private TextMeshProUGUI propertiesText;
    private void Awake()
    {
        touchControls = new ARTouchControls();
    }

    private void OnEnable()
    {
        touchControls.Enable();
        touchControls.TouchControls.Touched.started += ctx => OnTouchStarted(ctx);
    }

    private void OnDisable()
    {
        touchControls.Disable();
        touchControls.TouchControls.Touched.started -= ctx => OnTouchStarted(ctx);
    }
    
    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(touchControls.TouchControls.TouchPosition.ReadValue<Vector2>());
        
        if (Physics.Raycast(ray, out hit))
        {
            try
            {
                var data = hit.collider.GetComponent<FeatureData>();
                propertiesText.text = "Properties: \n";
                foreach (var property in data.Properties)
                {
                    propertiesText.text += property + "\n";
                }
            }
            catch (UnityException ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }
}
