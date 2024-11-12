// COPYRIGHT 1995-2024 ESRI
// TRADE SECRETS: ESRI PROPRIETARY AND CONFIDENTIAL
// Unpublished material - all rights reserved under the
// Copyright Laws of the United States and applicable international
// laws, treaties, and conventions.
//
// For additional information, contact:
// Attn: Contracts and Legal Department
// Environmental Systems Research Institute, Inc.
// 380 New York Street
// Redlands, California 92373
// USA
//
// email: legal@esri.com
using Esri.ArcGISMapsSDK.Components;
using Esri.ArcGISMapsSDK.Utils;
using TMPro;
using UnityEngine;

namespace Esri.ArcGISMapsSDK.Samples.Components
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ArcGISAttributionDisplay : MonoBehaviour
    {
        private TextMeshProUGUI attributionText;

        private ArcGISMapComponent mapComponent;

        private void OnEnable()
        {
            attributionText = GetComponent<TextMeshProUGUI>();

            if (attributionText == null)
            {
                return;
            }

            mapComponent = FindFirstObjectByType<ArcGISMapComponent>();

            if (!mapComponent || !mapComponent.View)
            {
                Debug.LogError("Unable to find a parent ArcGISMapComponent.");

                enabled = false;
                return;
            }

            SetAttributionText(mapComponent.View.AttributionText);

            mapComponent.View.AttributionChanged += () =>
            {
                SetAttributionText(mapComponent.View.AttributionText);
            };
        }

        private void OnDisable()
        {
            if (mapComponent && mapComponent.View)
            {
                mapComponent.View.AttributionChanged = null;
            }

            SetAttributionText(string.Empty);
        }

        private void SetAttributionText(string text)
        {
            Debug.Log(text);

            if (attributionText == null)
            {
                return;
            }

            ArcGISMainThreadScheduler.Instance().Schedule(() =>
            {
                attributionText.text = text;
            });
        }
    }
}