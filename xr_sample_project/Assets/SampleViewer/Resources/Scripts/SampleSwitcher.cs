using Esri.HPFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class SampleSwitcher : MonoBehaviour
{
    [SerializeField] private bool isSampleViewer;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button vrSceneButton;
    [SerializeField] private Button xrTableTopSceneButton;


    public void ChangeScene(string NextScene)
    {
        var currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync(NextScene);
    }

    private void Start()
    {
        if (!isSampleViewer)
        {
            homeButton.onClick.AddListener(delegate
            {
                ChangeScene("SampleViewer");
            });

            return;
        }

        vrSceneButton.onClick.AddListener(delegate
        {
            ChangeScene("VR-Sample");
        });

        xrTableTopSceneButton.onClick.AddListener(delegate
        {
            ChangeScene("XRTableTop");
        });
    }
}
