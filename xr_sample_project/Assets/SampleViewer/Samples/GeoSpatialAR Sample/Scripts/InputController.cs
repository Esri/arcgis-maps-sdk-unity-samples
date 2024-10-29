using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using Esri.GameEngine.MapView;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    private Camera arcGISCamera;
    private FeatureLayerQuery featureLayerQuery;
    private GameObject lastSelectedFeature;
    private bool menuVisible;
    private ARTouchControls touchControls;

    [Header("Animations")]
    [SerializeField] private Animator anim;
    [SerializeField] private Animator infoAnim;

    [Header("Materials")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material outlineMaterial;

    [Header("Mini Map")]
    private Button expandButton;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private RenderTexture miniMapTexture;

    [Header("UI Components")]
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button hideInfoButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button menuButton;
    [SerializeField] private TextMeshProUGUI propertiesText;
    [SerializeField] private Button searchButton;
    [SerializeField] private Sprite upSprite;

    private void Awake()
    {
        touchControls = new ARTouchControls();
        arcGISCamera = FindFirstObjectByType<ArcGISCameraComponent>().GetComponent<Camera>();
        featureLayerQuery = FindFirstObjectByType<FeatureLayerQuery>();
        expandButton = miniMap.GetComponent<Button>();
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
                SetAdditionalMaterial(highlightMaterial, outlineMaterial, hit.collider);

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
        collider.GetComponent<Renderer>().materials.CopyTo(materialsArray, 0);
        collider.GetComponent<Renderer>().materials.CopyTo(materialsArray, 1);
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

    private void Start()
    {
        inputField.text = featureLayerQuery.WebLink.Link;
        menuButton.image.sprite = upSprite;
        menuButton.GetComponentInChildren<TextMeshProUGUI>().text = "Touch to Hide Menu";
        anim.Play("ShowMenu");
        menuVisible = true;
        exitButton.gameObject.SetActive(false);

        inputField.onSubmit.AddListener(delegate (string weblink)
        {
            menuButton.image.sprite = downSprite;
            menuButton.GetComponentInChildren<TextMeshProUGUI>().text = "Touch to Show Menu";
            anim.Play("HideMenu");
            menuVisible = false;
            DestroyFeatures();
            featureLayerQuery.CreateLink(weblink);
            StartCoroutine(featureLayerQuery.GetFeatures());
        });

        clearButton.onClick.AddListener(delegate { DestroyFeatures(); });

        searchButton.onClick.AddListener(delegate
        {
            menuButton.image.sprite = downSprite;
            menuButton.GetComponentInChildren<TextMeshProUGUI>().text = "Touch to Show Menu";
            anim.Play("HideMenu");
            menuVisible = false;
            DestroyFeatures();
            StartCoroutine(featureLayerQuery.GetFeatures());
        });

        menuButton.onClick.AddListener(delegate
        {
            if (menuVisible)
            {
                menuButton.image.sprite = downSprite;
                menuButton.GetComponentInChildren<TextMeshProUGUI>().text = "Touch to Show Menu";
                anim.Play("HideMenu");
                menuVisible = false;
            }
            else
            {
                menuButton.image.sprite = upSprite;
                menuButton.GetComponentInChildren<TextMeshProUGUI>().text = "Touch to Hide Menu";
                anim.Play("ShowMenu");
                menuVisible = true;
            }
        });

        expandButton.onClick.AddListener(delegate
        {
            miniMap.SetActive(false);
            arcGISCamera.targetTexture = null;
            exitButton.gameObject.SetActive(true);
        });

        exitButton.onClick.AddListener(delegate
        {
            miniMap.SetActive(true);
            arcGISCamera.targetTexture = miniMapTexture;
            exitButton.gameObject.SetActive(false);
        });

        infoButton.onClick.AddListener(delegate
        {
            infoAnim.Play("ShowInstructions");
        });

        hideInfoButton.onClick.AddListener(delegate
        {
            infoAnim.Play("HideInstructions");
        });
    }
}