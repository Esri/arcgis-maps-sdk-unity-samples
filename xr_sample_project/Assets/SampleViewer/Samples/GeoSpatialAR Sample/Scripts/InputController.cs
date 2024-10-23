using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Button bottomHandle;
    [SerializeField] private Button clearButton;
    [SerializeField] private FeatureLayerQuery featureLayerQuery;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material outlineMaterial;
    private GameObject lastSelectedFeature;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI propertiesText;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button topHandle;
    private ARTouchControls touchControls;

    private void Awake()
    {
        touchControls = new ARTouchControls();
    }

    private void DestroyFeatures()
    {
        if (featureLayerQuery.FeatureItems.Count == 0)
        {
            return;
        }
        
        foreach (var feature in featureLayerQuery.FeatureItems)
        {
            Destroy(feature);
        }
    }
    
    private void OnEnable()
    {
        touchControls.Enable();
        touchControls.TouchControls.Touched.started += OnTouchStarted;
    }

    private void OnDisable()
    {
        touchControls.Disable();
        touchControls.TouchControls.Touched.started -= OnTouchStarted;
    }
    
    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(touchControls.TouchControls.TouchPosition.ReadValue<Vector2>());
        
        if (Physics.Raycast(ray, out hit))
        {
            try
            {
                if (lastSelectedFeature)
                {
                    ClearAdditionalMaterial(lastSelectedFeature);   
                }
                
                lastSelectedFeature = hit.collider.gameObject;
                var data = lastSelectedFeature.GetComponent<FeatureData>();
                SetAdditionalMaterial(highlightMaterial,outlineMaterial ,hit.collider);
                
                if (!propertiesText.gameObject.activeInHierarchy)
                {
                    propertiesText.gameObject.SetActive(true);
                }
                
                propertiesText.text = "Properties: \n";
                
                foreach (var property in data.Properties)
                {
                    propertiesText.text += "- " + property + "\n";
                }
            }
            catch (UnityException ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }

    private void SetAdditionalMaterial(Material highlight, Material outLine, Collider collider)
    {
        Material[] materialsArray = new Material[collider.GetComponent<Renderer>().materials.Length + 2];
        collider.GetComponent<Renderer>().materials.CopyTo(materialsArray,0);
        collider.GetComponent<Renderer>().materials.CopyTo(materialsArray,1);
        materialsArray[materialsArray.Length - 1] = highlight;
        materialsArray[materialsArray.Length - 2] = outLine;
        collider.GetComponent<Renderer>().materials = materialsArray;
    }

    private void ClearAdditionalMaterial(GameObject feature)
    {
        Material[] materialsArray = new Material[feature.GetComponent<Renderer>().materials.Length - 2];
        
        for (int i = 0; i < feature.GetComponent<Renderer>().materials.Length - 2; i++)
        {
            materialsArray[i] = feature.GetComponent<Renderer>().materials[i];
        }
        
        feature.GetComponent<Renderer>().materials = materialsArray;
    }

    private void PullDown()
    {
        anim.Play("ShowMenu");
    }

    private void SwipeUp()
    {
        anim.Play("HideMenu");
    }
    
    private void Start()
    {
        inputField.text = featureLayerQuery.WebLink.Link;
        anim.Play("ShowMenu");
        propertiesText.gameObject.SetActive(false);

        inputField.onSubmit.AddListener(delegate (string weblink)
        {
            anim.Play("HideMenu");
            DestroyFeatures();
            featureLayerQuery.CreateLink(weblink);
            StartCoroutine(featureLayerQuery.GetFeatures());
        });

        clearButton.onClick.AddListener(delegate
        {
            DestroyFeatures();
        });
        
        searchButton.onClick.AddListener(delegate
        {
            anim.Play("HideMenu");
            DestroyFeatures();
            StartCoroutine(featureLayerQuery.GetFeatures());
        });

        topHandle.onClick.AddListener(delegate
        {
            PullDown();
        });
        
        bottomHandle.onClick.AddListener(delegate
        {
            SwipeUp();
        });
    }
}
