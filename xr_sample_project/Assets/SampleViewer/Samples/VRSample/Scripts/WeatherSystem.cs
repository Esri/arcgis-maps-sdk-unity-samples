// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//

using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
public struct CloudSettings
{
    [SerializeField] public float cloudSize;
    [SerializeField] public float cloudDensity;
    [SerializeField] public float cloudAlpha;
    [SerializeField] public Vector2 cloudSpeed;
    [SerializeField] public Color cloudColor;
    [SerializeField] public Color zeninthColor;
    [SerializeField] public Color horizonColor;

    public CloudSettings(float cloudSize, float cloudDensity, float cloudAlpha, Vector2 cloudSpeed, Color cloudColor, Color zeninthColor, Color horizonColor)
    {
        this.cloudSize = cloudSize;
        this.cloudDensity = cloudDensity;
        this.cloudAlpha = cloudAlpha;
        this.cloudSpeed = cloudSpeed;
        this.cloudColor = cloudColor;
        this.zeninthColor = zeninthColor;
        this.horizonColor = horizonColor;
    }
}

public class WeatherSystem : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private Material cloudMaterial;

    [SerializeField] private XROrigin player;
    [SerializeField] private float weatherHeight;

    [SerializeField] private CloudSettings[] cloudSettings;
    [SerializeField] private LightmapData[] lightmaps;

    [Header("Rainy Settings")]
    [SerializeField] private ParticleSystem rainSystem;

    [SerializeField] private AudioSource rainAudio;

    [Header("Snowy Settings")]
    [SerializeField] private ParticleSystem snowSystem;

    [SerializeField] private AudioSource snowAudio;

    [Header("Thunder Settings")]
    [SerializeField] private ParticleSystem thunderRainParticles;
    [SerializeField] private float lightingStartHeight;

    [SerializeField] private VisualEffect lightningEffect;
    [SerializeField] private AudioSource[] thunderAudio;
    private Coroutine thunderStormRoutine;

    private void ModifyClouds(int index)
    {
        if (index < 0 || index >= cloudSettings.Length)
        {
            return;
        }
       
        cloudMaterial.SetFloat("_Cloud_size", cloudSettings[index].cloudSize);
        cloudMaterial.SetFloat("_Cloud_density", cloudSettings[index].cloudDensity);
        cloudMaterial.SetFloat("_Cloud_alpha", cloudSettings[index].cloudAlpha);
        cloudMaterial.SetVector("_Cloud_speed", cloudSettings[index].cloudSpeed);
        cloudMaterial.SetColor("_CloudColor", cloudSettings[index].cloudColor);
        cloudMaterial.SetColor("_ZenithColor", cloudSettings[index].zeninthColor);
        cloudMaterial.SetColor("_HorizonColor", cloudSettings[index].horizonColor);
    }

    public void SetToCloudy()
    {
        rainSystem.Stop();
        snowSystem.Stop();
        snowAudio.Stop();

        EndStorm();

        if (rainAudio.isPlaying) rainAudio.Stop();

        ModifyClouds(1);
    }

    public void SetToRainy()
    {
        rainSystem.Play();
        snowSystem.Stop();
        snowAudio.Stop();

        EndStorm();

        if (!rainAudio.isPlaying) rainAudio.Play();

        ModifyClouds(2);
    }

    public void SetToSnowy()
    {
        rainSystem.Stop();
        snowSystem.Play();
        snowAudio.Play();

        EndStorm();

        if (rainAudio.isPlaying) rainAudio.Stop();

        ModifyClouds(3);
    }

    public void SetToSunny()
    {
        rainSystem.Stop();
        snowSystem.Stop();
        snowAudio.Stop();

        EndStorm();

        if (rainAudio.isPlaying) rainAudio.Stop();

        ModifyClouds(0);
    }

    public void SetToThunder()
    {
        rainSystem.Stop();
        snowSystem.Stop();
        snowAudio.Stop();

        BeginStorm();

        ModifyClouds(4);
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
        cloudMaterial = RenderSettings.skybox;
    }

    private IEnumerator ThunderStorm()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 10f));

            Vector3 placement = player.transform.position + (player.Camera.transform.forward * 4000f) + new Vector3(0, 50f, 0);

            lightningEffect.transform.position = new Vector3(placement.x, player.transform.position.y + lightingStartHeight, placement.z);
            lightningEffect.transform.LookAt(player.Camera.transform);

            cloudMaterial.SetColor("_CloudColor", new Color32(245, 245, 245, 255));

            lightningEffect.Play();

            yield return new WaitForSeconds(0.5f);

            cloudMaterial.SetColor("_CloudColor", cloudSettings[4].cloudColor);

            yield return new WaitForSeconds(0.5f);

            thunderAudio[UnityEngine.Random.Range(0, thunderAudio.Length - 1)].Play();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = player.transform.position + new Vector3(0f, weatherHeight, 0f);
    }
}