using System;
using System.Collections;
using System.Collections.Generic;
using Esri.ArcGISMapsSDK.Components;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct Data
{
    public string name;
    public bool enabled;
}

public class ScrollViewItem : MonoBehaviour, IPointerClickHandler
{
    private FeatureLayer featureLayer;
    public Data Data;

    private void Start()
    {
        featureLayer = FindObjectOfType<ArcGISMapComponent>().GetComponentInChildren<FeatureLayer>();
        Data.name = GetComponentInChildren<TextMeshProUGUI>().text;
    }

    private void Update()
    {
        if (featureLayer.GetAllOutfields && Data.name == "Get All Features")
        {
            Data.enabled = true;
        }

        GetComponentInChildren<Toggle>().isOn = Data.enabled;
        featureLayer.SelectItems();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!featureLayer.OutfieldsToGet.Contains(Data.name))
        {
            if (Data.name == "Get All Features" && !featureLayer.GetAllOutfields)
            {
                featureLayer.GetAllOutfields = true;
                featureLayer.OutfieldsToGet.Clear();
            }
            else
            {
                featureLayer.GetAllOutfields = false;
                featureLayer.OutfieldsToGet.Remove("Get All Features");
            }

            featureLayer.OutfieldsToGet.Add(Data.name);
            Data.enabled = true;
        }
        else
        {
            if (Data.name == "Get All Features" && featureLayer.GetAllOutfields)
            {
                featureLayer.GetAllOutfields = false;
            }

            featureLayer.OutfieldsToGet.Remove(Data.name);
            Data.enabled = false;
        }
    }
}