using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup menu;
    [SerializeField] private CanvasGroup instructions;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OpenInstructions()
    {
        menu.gameObject.SetActive(false);
        instructions.gameObject.SetActive(true);
    }

    public void ReturnToMenu()
    {
        instructions.gameObject.SetActive(false);
        menu.gameObject.SetActive(true);
    }

    public void ExitMenus()
    {
        instructions.gameObject.SetActive(false);
        menu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
