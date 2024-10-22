using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Button button;
    [SerializeField] private FeatureLayerQuery featureLayerQuery;
    [SerializeField] private Button menuButton;
    private ARTouchControls touchControls;
    [SerializeField] private TextMeshProUGUI propertiesText;
    [SerializeField] private TMP_InputField inputField;

    private bool menuHidden = false;

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

    private void Start()
    {
        inputField.text = featureLayerQuery.WebLink.Link;
        anim.Play("ShowMenu");
        menuHidden = false;

        inputField.onSubmit.AddListener(delegate (string weblink)
        {
            menuHidden = true;
            anim.Play("HideMenu");
            featureLayerQuery.CreateLink(weblink);
            StartCoroutine(featureLayerQuery.GetFeatures());
        });

        button.onClick.AddListener(delegate
        {
            StartCoroutine(featureLayerQuery.GetFeatures());
        });

        menuButton.onClick.AddListener(delegate
        {
            if (menuHidden)
            {
                anim.Play("ShowMenu");
                menuHidden = false;
            }
            else
            {
                menuHidden = true;
                anim.Play("HideMenu");
            }
        });
    }
}
