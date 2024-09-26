using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class HitTestQuery : MonoBehaviour
{
    private string weblink = "https://services.arcgis.com/P3ePLMYs2RVChkJx/ArcGIS/rest/services/Buildings_Boston_USA/FeatureServer/0/query?f=geojson&where=1=1&outfields=*";
    [SerializeField] private List<string> outfields = new List<string>();
    [SerializeField] private TMP_Dropdown scrollView;
    [SerializeField] private JToken[] jFeatures;
    
    public IEnumerator GetFeatures()
    {
        // To learn more about the Feature Layer rest API and all the things that are possible checkout
        // https://developers.arcgis.com/rest/services-reference/enterprise/query-feature-service-layer-.htm

        UnityWebRequest Request = UnityWebRequest.Get(weblink);
        yield return Request.SendWebRequest();

        if (Request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(Request.error);
        }
        else
        {
            PopulateOutfieldsDropdown(Request.downloadHandler.text);
            CreateGameObjectsFromResponse(Request.downloadHandler.text);
        }
    }

    private void CreateGameObjectsFromResponse(string response)
    {
        // Deserialize the JSON response from the query.
        var jObject = JObject.Parse(response);
        jFeatures = jObject.SelectToken("features").ToArray();
    }

    private void PopulateOutfieldsDropdown(string response)
    {
        var jObject = JObject.Parse(response);
        var jFeatures = jObject.SelectToken("features").ToArray();
        var properties = jFeatures[0].SelectToken("properties");
        //Populate Outfields drop down

        foreach (var outfield in properties)
        {
            var removeQuote = outfield.ToString().Split('"');
            var outfieldName = removeQuote[1].Split(":");
            outfields.Add(outfieldName[0]);
        }
        
        scrollView.AddOptions(outfields);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetFeatures());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
