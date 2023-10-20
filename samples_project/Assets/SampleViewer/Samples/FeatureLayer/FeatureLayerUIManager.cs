using FeatureLayerData;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FeatureLayerUIManager : MonoBehaviour
{
    [SerializeField] private Animator dropDownAnim;
    [SerializeField] private Toggle dropDownButton;
    private FeatureLayer featureLayer;
    [SerializeField] private Toggle getAllToggle;
    [SerializeField] private Button hideButton;
    [SerializeField] private Animator infoAnim;
    [SerializeField] private Toggle infoButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_InputField maxInputField;
    [SerializeField] private TMP_InputField minInputField;
    [SerializeField] private GameObject outfieldsList;
    [SerializeField] private Button requestButton;
    [SerializeField] private Animator resetAnim;
    [SerializeField] private Button resetButton;
    [SerializeField] private TextMeshProUGUI textToDisplay;
    [SerializeField] private TextMeshProUGUI titleText;
    
    public enum TextToDisplay
    {
        Information,
        LinkError,
        IndexOutOfBoundsError,
        CoordinatesError
    }

    public TextToDisplay DisplayText;

    private void Start()
    {
        InvokeRepeating("ErrorCheck", 1.0f, 0.5f);
        featureLayer = GetComponent<FeatureLayer>();
        inputField.text = featureLayer.WebLink.Link;
        maxInputField.text = featureLayer.LastValue.ToString();
        minInputField.text = featureLayer.StartValue.ToString();
        getAllToggle.isOn = featureLayer.GetAllFeatures;

        dropDownButton.onValueChanged.AddListener(delegate(bool value)
        {
            outfieldsList.SetActive(value);
            if (!value)
            {
                dropDownAnim.Play("ReverseDropDownArrow");
            }
            else
            {
                dropDownAnim.Play("DropDownArrow");
            }
        });

        inputField.onSubmit.AddListener(delegate(string weblink)
        {
            featureLayer.NewLink = true;
            featureLayer.CreateLink(weblink);
            StartCoroutine(featureLayer.GetFeatures());
        });

        requestButton.onClick.AddListener(delegate
        {
            if (featureLayer.FeatureItems.Count != 0)
            {
                foreach (var item in featureLayer.FeatureItems)
                {
                    Destroy(item);
                }

                featureLayer.FeatureItems.Clear();
                featureLayer.Features.Clear();
            }

            StartCoroutine(featureLayer.GetFeatures());
        });

        resetButton.onClick.AddListener(delegate
        {
            resetAnim.Play("ResetRotation");
            foreach (var toggle in featureLayer.ListItems)
            {
                featureLayer.GetAllOutfields = false;
                toggle.GetComponent<ScrollViewItem>().Data.enabled = false;
                featureLayer.OutfieldsToGet.Clear();
            }
        });

        getAllToggle.onValueChanged.AddListener(delegate(bool value)
        {
            featureLayer.GetAllFeatures = value;
        });

        maxInputField.onSubmit.AddListener(delegate(string value)
        {
            if (Convert.ToInt32(value) > 0 && Convert.ToInt32(value) > featureLayer.StartValue)
            {
                featureLayer.LastValue = Convert.ToInt32(value);
            }
            else
            {
                featureLayer.LastValue = 10;
                maxInputField.text = featureLayer.LastValue.ToString();
            }
        });

        minInputField.onSubmit.AddListener(delegate(string value)
        {
            if (Convert.ToInt32(value) > 0 && Convert.ToInt32(value) < featureLayer.LastValue)
            {
                featureLayer.StartValue = Convert.ToInt32(value);
            }
            else
            {
                featureLayer.StartValue = 0;
                minInputField.text = featureLayer.StartValue.ToString();
            }
        });
    }

    private void ErrorCheck()
    {
        if (DisplayText == TextToDisplay.Information)
        {
            titleText.text = "Information";
            textToDisplay.text = "Please use the input field above to modify or insert a link. \n" +
                                 "\n" +
                                 "Point Layer data sets are currently the only data set supported, we apologize for the inconvenience.";
        }
        else if (DisplayText == TextToDisplay.LinkError)
        {
            titleText.text = "Warning";
            textToDisplay.text = "There was an error processing your request. \n" +
                                 "\n" +
                                 "Please double check your link and try again.";
            infoAnim.Play("NotificationAnim");
        }
        else if (DisplayText == TextToDisplay.IndexOutOfBoundsError)
        {
            titleText.text = "Warning";
            textToDisplay.text = "Index out of bounds. \n" +
                                 "\n" +
                                 "Please lower your max value or select 'Get All Features'.";
            infoAnim.Play("NotificationAnim");
        }
        else if (DisplayText == TextToDisplay.CoordinatesError)
        {
            titleText.text = "Warning";
            textToDisplay.text = "This data type is currently unsupported. \n" +
                                 "\n" +
                                 "We currently only support Point Layer types, we apologize for the inconvenience.";
            infoAnim.Play("NotificationAnim");
        }

        if (featureLayer.jFeatures != null)
        {
            if (featureLayer.jFeatures.Length >= featureLayer.LastValue || featureLayer.GetAllFeatures)
            {
                if (DisplayText != TextToDisplay.LinkError && DisplayText != TextToDisplay.CoordinatesError)
                {
                    DisplayText = TextToDisplay.Information;
                }
            }
        }
    }
}