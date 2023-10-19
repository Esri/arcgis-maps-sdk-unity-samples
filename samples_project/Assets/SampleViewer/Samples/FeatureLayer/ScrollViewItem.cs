using Esri.ArcGISMapsSDK.Components;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FeatureLayerData
{
    public struct ScrollItemData
    {
        public string name;
        public bool enabled;
    }

    public class ScrollViewItem : MonoBehaviour, IPointerClickHandler
    {
        private FeatureLayer featureLayer;
        public ScrollItemData Data;
        private void Start()
        {
            featureLayer = FindObjectOfType<ArcGISMapComponent>().GetComponentInChildren<FeatureLayer>();
            Data.name = GetComponentInChildren<TextMeshProUGUI>().text;
            InvokeRepeating("CheckDataValues", 0.1f, 0.5f);
        }

        private void CheckDataValues()
        {
            if (featureLayer.GetAllOutfields && Data.name == "Get All Outfields")
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
                if (Data.name == "Get All Outfields" && !featureLayer.GetAllOutfields)
                {
                    featureLayer.GetAllOutfields = true;
                    featureLayer.OutfieldsToGet.Clear();
                }
                else
                {
                    featureLayer.GetAllOutfields = false;
                    featureLayer.OutfieldsToGet.Remove("Get All Outfields");
                }

                featureLayer.OutfieldsToGet.Add(Data.name);
                Data.enabled = true;
                return;
            }

            if (Data.name == "Get All Outfields" && featureLayer.GetAllOutfields)
            {
                featureLayer.GetAllOutfields = false;
            }

            featureLayer.OutfieldsToGet.Remove(Data.name);
            Data.enabled = false;
        }
    }
}