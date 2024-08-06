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
    [SerializeField] public Vector3 cloudHeight;

    [ColorUsage(true, true)]
    [SerializeField] public Color color;

    [ColorUsage(true, true)]
    [SerializeField] public Color color1;

    public CloudSettings(float size, float density, float alpha, Vector2 speed, Vector3 height, Color color, Color color1)
    {
        this.cloudSize = size;
        this.cloudDensity = density;
        this.cloudAlpha = alpha;
        this.cloudSpeed = speed;
        this.cloudHeight = height;
        this.color = color;
        this.color1 = color1;
    }
}

public class WeatherSystem : MonoBehaviour
{
    [ColorUsage(true, true)]
    private Material cloudMaterial;

    [Header("General Settings")]
    [SerializeField] private Renderer cloudRenderer;

    [SerializeField] private XROrigin player;
    [SerializeField] private float weatherHeight;
    [SerializeField] private float cloudHeight;

    [SerializeField] private CloudSettings[] cloudSettings;

    [Header("Rainy Settings")]
    [SerializeField] private ParticleSystem rainSystem;

    [SerializeField] private AudioSource rainAudio;

    [Header("Snowy Settings")]
    [SerializeField] private ParticleSystem snowSystem;

    [SerializeField] private AudioSource snowAudio;

    [Header("Thunder Settings")]
    [SerializeField] private ParticleSystem thunderRainParticles;

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
        cloudMaterial.SetVector("_Cloud_height", cloudSettings[index].cloudHeight);
        cloudMaterial.SetColor("_Color", cloudSettings[index].color);
        cloudMaterial.SetColor("_Color_1", cloudSettings[index].color1);
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
        cloudMaterial = cloudRenderer.material;
    }

    private IEnumerator ThunderStorm()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 10f));

            lightningEffect.transform.position = player.transform.position + (player.Camera.transform.forward * 4000f) + new Vector3(0, 50f, 0);
            lightningEffect.transform.LookAt(player.Camera.transform);

            cloudMaterial.SetColor("_Color", new Color32(245, 245, 245, 255));

            lightningEffect.Play();

            yield return new WaitForSeconds(0.5f);

            cloudMaterial.SetColor("_Color", cloudSettings[4].color);

            yield return new WaitForSeconds(0.5f);

            thunderAudio[UnityEngine.Random.Range(0, thunderAudio.Length - 1)].Play();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = player.transform.position + new Vector3(0f, weatherHeight, 0f);
        cloudRenderer.transform.position = transform.position + new Vector3(0f, cloudHeight, 0f);
    }
}