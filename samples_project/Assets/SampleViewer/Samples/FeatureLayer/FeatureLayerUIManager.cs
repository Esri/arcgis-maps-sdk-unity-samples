using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeatureLayerUIManager : MonoBehaviour
{
    [SerializeField] private Toggle dropDownButton;
    private FeatureLayer featureLayer;
    [SerializeField] private Toggle getAllToggle;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_InputField maxInputField;
    [SerializeField] private TMP_InputField minInputField;
    [SerializeField] private GameObject outfieldsList;
    [SerializeField] private Button requestButton;
    [SerializeField] private Button resetButton;

    // Start is called before the first frame update
    private void Start()
    {
        featureLayer = GetComponent<FeatureLayer>();
        inputField.text = featureLayer.WebLink.Link;
        maxInputField.text = featureLayer.LastValue.ToString();
        minInputField.text = featureLayer.StartValue.ToString();
        getAllToggle.isOn = featureLayer.GetAllFeatures;
        
        dropDownButton.onValueChanged.AddListener(delegate(bool value)
        {
            outfieldsList.SetActive(value);
        });
        
        inputField.onSubmit.AddListener(delegate(string weblink)
        {
            featureLayer.NewLink = true;
            featureLayer.CreateLink(weblink);
            StartCoroutine(featureLayer.GetFeatures()); 
        });

        requestButton.onClick.AddListener(delegate
        {
            var items = GameObject.FindGameObjectsWithTag("FeatureItem");
            if (items.Length != 0)
            {
                foreach (var item in items)
                {
                    Destroy(item);
                }
                featureLayer.Features.Clear();
            }
            StartCoroutine(featureLayer.GetFeatures()); 
        });

        resetButton.onClick.AddListener(delegate
        {
            foreach (var toggle in featureLayer.ListItems)
            {
                featureLayer.GetAllOutfields = false;
                toggle.GetComponent<ScrollViewItem>().data.enabled = false;
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
}
