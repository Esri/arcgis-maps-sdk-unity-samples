using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FeatureLayerUIManager : MonoBehaviour
{
    private FeatureLayer featureLayer;
    public TMP_InputField InputField;
    // Start is called before the first frame update
    void Start()
    {
        featureLayer = GetComponent<FeatureLayer>();
        InputField.text = featureLayer.webLink.Link;
        InputField.onValueChanged.AddListener(delegate(string weblink)
        {
            featureLayer.CreateLink(weblink);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
