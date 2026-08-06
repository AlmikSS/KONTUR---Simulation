using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CreepyZoneTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Color activeCameraColor = Color.white;

    [Header("Fog Settings (Lighting -> Environment)")]
    [SerializeField] private Color activeFogColor = Color.black;
    [SerializeField] private float activeFogDensity = 0.08f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip creepySound;
    [Range(0f, 1f)] [SerializeField] private float targetVolume = 1f;

    // Сохранённые исходные параметры
    private Color defaultCameraColor;
    private Color defaultFogColor;
    private float defaultFogDensity;

    private Coroutine transitionCoroutine;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            defaultCameraColor = targetCamera.backgroundColor;

        // Включаем туман и запоминаем исходные значения
        RenderSettings.fog = true;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;

        if (audioSource != null && creepySound != null)
        {
            audioSource.clip = creepySound;
            audioSource.loop = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StartTransition(activeCameraColor, activeFogColor, activeFogDensity, targetVolume, playAudio: true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StartTransition(defaultCameraColor, defaultFogColor, defaultFogDensity, 0f, playAudio: false);
    }

    private void StartTransition(Color targetCamCol, Color targetFogCol, float targetDensity, float targetVol, bool playAudio)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionRoutine(targetCamCol, targetFogCol, targetDensity, targetVol, playAudio));
    }

    private IEnumerator TransitionRoutine(Color targetCamCol, Color targetFogCol, float targetDensity, float targetVol, bool playAudio)
    {
        if (playAudio && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.volume = 0f;
            audioSource.Play();
        }

        Color startCamCol = targetCamera != null ? targetCamera.backgroundColor : Color.white;
        Color startFogCol = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float startVol = audioSource != null ? audioSource.volume : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Сглаживание движения (Ease In Out)
            t = Mathf.SmoothStep(0f, 1f, t);

            // Плавное изменение цвета камеры
            if (targetCamera != null)
                targetCamera.backgroundColor = Color.Lerp(startCamCol, targetCamCol, t);

            // Плавное изменение тумана
            RenderSettings.fogColor = Color.Lerp(startFogCol, targetFogCol, t);
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, targetDensity, t);

            // Плавный затухание/нарастание звука
            if (audioSource != null)
                audioSource.volume = Mathf.Lerp(startVol, targetVol, t);

            yield return null;
        }

        // Финальная точность значений
        if (targetCamera != null) targetCamera.backgroundColor = targetCamCol;
        RenderSettings.fogColor = targetFogCol;
        RenderSettings.fogDensity = targetDensity;

        if (audioSource != null)
        {
            audioSource.volume = targetVol;
            if (!playAudio && audioSource.volume <= 0.01f)
            {
                audioSource.Stop();
            }
        }
    }
}