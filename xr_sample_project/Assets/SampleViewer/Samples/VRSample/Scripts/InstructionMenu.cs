// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using UnityEngine;

public class InstructionMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup instructions;
    [SerializeField] private CanvasGroup menu;

    public void ExitMenus()
    {
        instructions.gameObject.SetActive(false);
        menu.gameObject.SetActive(true);
        gameObject.SetActive(false);
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
}