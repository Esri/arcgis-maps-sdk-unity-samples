// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    [Header("---------Dyanmic Variables---------")]
    [SerializeField] private Color fadeColor;

    [Min(0)][SerializeField] private float fadeDuration = 2;

    private Renderer rendererComponent;

    // Make Object into a Singleton
    public static FadeScreen Instance { get; private set; }

    // Nest coroutine so user can normally call Fade function outside of this script
    public void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    public void FadeIn()
    {
        // Fade from 100% oppacity to 0%
        Fade(1, 0);
    }

    public void FadeOut()
    {
        // Fade from 0% oppacity to 100%
        Fade(0, 1);
    }

    // Helper function to retrieve private variable
    public float GetFadeDuration()
    {
        return fadeDuration;
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        float timer = 0;
        Color newColor;

        // While timer has not concluded, change material alpha using lerp
        while (timer <= fadeDuration)
        {
            newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);
            if (rendererComponent)
            {
                rendererComponent.material.SetColor("_UnlitColor", newColor);
            }
            timer += Time.fixedDeltaTime;
            yield return null;
        }

        // To Confrim that the alpha finishes at the correct amount
        Color lastColor = fadeColor;
        lastColor.a = alphaOut;
        if (rendererComponent)
        {
            rendererComponent.material.SetColor("_UnlitColor", lastColor);
        }
    }

    private void Start()
    {
        rendererComponent = GetComponent<Renderer>();
        FadeOut();
    }
}