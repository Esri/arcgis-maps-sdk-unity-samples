// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.VFX;

public class WeatherSystem : MonoBehaviour
{
    private Material cloudMaterial;

    [Header("General Settings")]
    [SerializeField] private Renderer cloudRenderer;

    [SerializeField] private VisualEffect lightningEffect;
    [SerializeField] private XROrigin player;
    [SerializeField] private Vector3 playerOffset;
    [SerializeField] private AudioSource rainAudio;

    [Header("Rainy Settings")]
    [SerializeField] private ParticleSystem rainSystem;
    [SerializeField] private AudioSource snowAudio;

    [Header("Snowy Settings")]
    [SerializeField] private ParticleSystem snowSystem;
    [SerializeField] private AudioSource[] thunderAudio;

    [Header("Thunder Settings")]
    [SerializeField] private ParticleSystem thunderRainParticles;
    private Coroutine thunderStormRoutine;
    public void SetToCloudy()
    {
        rainSystem.Stop();
        snowSystem.Stop();
        snowAudio.Stop();

        EndStorm();

        if (rainAudio.isPlaying) rainAudio.Stop();

        cloudMaterial.SetFloat("_Cloud_size", 1000);
        cloudMaterial.SetFloat("_Cloud_density", 1f);
        cloudMaterial.SetFloat("_Cloud_alpha", 1);
        cloudMaterial.SetVector("_Cloud_speed", new Vector2(0.0003f, 0));
        cloudMaterial.SetVector("_Cloud_height", new Vector3(0, 100, 0));
        cloudMaterial.SetColor("_Color", new Color32(255, 255, 255, 255));
        cloudMaterial.SetColor("_Color_1", new Color32(204, 231, 255, 255));
    }

    public void SetToRainy()
    {
        rainSystem.Play();
        snowSystem.Stop();
        snowAudio.Stop();

        EndStorm();

        if (!rainAudio.isPlaying) rainAudio.Play();

        cloudMaterial.SetFloat("_Cloud_size", 5000);
        cloudMaterial.SetFloat("_Cloud_density", 1f);
        cloudMaterial.SetFloat("_Cloud_alpha", 10);
        cloudMaterial.SetVector("_Cloud_speed", new Vector2(0.00003f, 0));
        cloudMaterial.SetVector("_Cloud_height", new Vector3(0, 100, 0));
        cloudMaterial.SetColor("_Color", new Color32(65, 65, 65, 255));
        cloudMaterial.SetColor("_Color_1", new Color32(161, 161, 161, 255));
    }

    public void SetToSnowy()
    {
        rainSystem.Stop();
        snowSystem.Play();
        snowAudio.Play();

        EndStorm();

        if (rainAudio.isPlaying) rainAudio.Stop();

        cloudMaterial.SetFloat("_Cloud_size", 5000);
        cloudMaterial.SetFloat("_Cloud_density", 1f);
        cloudMaterial.SetFloat("_Cloud_alpha", 20);
        cloudMaterial.SetVector("_Cloud_speed", new Vector2(0.000003f, 0));
        cloudMaterial.SetVector("_Cloud_height", new Vector3(0, 100, 0));
        cloudMaterial.SetColor("_Color", new Color32(224, 224, 224, 255));
        cloudMaterial.SetColor("_Color_1", new Color32(190, 190, 190, 255));
    }

    public void SetToSunny()
    {
        rainSystem.Stop();
        snowSystem.Stop();
        snowAudio.Stop();

        EndStorm();

        if (rainAudio.isPlaying) rainAudio.Stop();

        cloudMaterial.SetFloat("_Cloud_size", 20000);
        cloudMaterial.SetFloat("_Cloud_density", 0.02f);
        cloudMaterial.SetFloat("_Cloud_alpha", 1);
        cloudMaterial.SetVector("_Cloud_speed", new Vector2(0.0003f, 0));
        cloudMaterial.SetVector("_Cloud_height", new Vector3(0, 100, 0));
        cloudMaterial.SetColor("_Color", new Color32(255, 255, 255, 255));
        cloudMaterial.SetColor("_Color_1", new Color32(0, 124, 233, 255));
    }

    public void SetToThunder()
    {
        rainSystem.Stop();
        snowSystem.Stop();
        snowAudio.Stop();

        BeginStorm();

        cloudMaterial.SetFloat("_Cloud_size", 9000);
        cloudMaterial.SetFloat("_Cloud_density", 1f);
        cloudMaterial.SetFloat("_Cloud_alpha", 50);
        cloudMaterial.SetVector("_Cloud_speed", new Vector2(0.0001f, 0));
        cloudMaterial.SetVector("_Cloud_height", new Vector3(0, 100, 0));
        cloudMaterial.SetColor("_Color", new Color32(22, 22, 22, 255));
        cloudMaterial.SetColor("_Color_1", new Color32(12, 12, 12, 255));
    }

    private void BeginStorm()
    {
        thunderRainParticles.Play();
        if (!rainAudio.isPlaying) rainAudio.Play();
        if (thunderStormRoutine == null)
        {
            thunderStormRoutine = StartCoroutine(ThunderStorm());
        }
    }

    private void EndStorm()
    {
        thunderRainParticles.Stop();
        if (thunderStormRoutine != null)
        {
            StopCoroutine(thunderStormRoutine);
            thunderStormRoutine = null;
        }
        lightningEffect.Stop();
    }

    // Start is called before the first frame update
    private void Start()
    {
        cloudMaterial = cloudRenderer.material;
    }
    private IEnumerator ThunderStorm()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));

            lightningEffect.transform.position = player.transform.position + (player.Camera.transform.forward * 4000f) + new Vector3(0, 50f, 0);
            lightningEffect.transform.LookAt(player.Camera.transform);

            cloudMaterial.SetColor("_Color", new Color32(137, 137, 137, 255));
            cloudMaterial.SetColor("_Color_1", new Color32(171, 171, 171, 255));

            lightningEffect.Play();

            yield return new WaitForSeconds(0.5f);

            cloudMaterial.SetColor("_Color", new Color32(22, 22, 22, 255));
            cloudMaterial.SetColor("_Color_1", new Color32(12, 12, 12, 255));

            yield return new WaitForSeconds(0.5f);

            thunderAudio[Random.Range(0, thunderAudio.Length - 1)].Play();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = player.transform.position + playerOffset;
    }
}