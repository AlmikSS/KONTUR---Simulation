using System.Collections;
using _KONTUR___Simulation._Scripts.GamePlay.UI.Laboratory;
using _KONTUR___Simulation._Scripts.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class OneTimeCreepyTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float duration = 3.0f;

    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Color targetCameraColor = Color.white;

    [Header("Fog Settings (Lighting -> Environment)")]
    [SerializeField] private Color targetFogColor = Color.black;
    [SerializeField] private float targetFogDensity = 0.08f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip creepySound;
    [Range(0f, 1f)] [SerializeField] private float targetVolume = 1f;

    [Header("Final Event")]
    [Tooltip("Можно привязать действия через инспектор или написать код в OnEffectCompleted()")]
    [SerializeField] private UnityEvent onEffectComplete;
    
    [SerializeField] private DayConfig _config;
    [SerializeField] private int _nextLevel;
    
    private Collider triggerCollider;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Включаем туман, если он был выключен
        RenderSettings.fog = true;

        if (audioSource != null && creepySound != null)
        {
            audioSource.clip = creepySound;
            audioSource.loop = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Отключаем триггер, чтобы повторный вход был невозможен
        triggerCollider.enabled = false;

        StartCoroutine(IrreversibleEffectRoutine());
    }

    private IEnumerator IrreversibleEffectRoutine()
    {
        if (audioSource != null && creepySound != null)
        {
            audioSource.volume = 0f;
            audioSource.Play();
        }

        Color startCamCol = targetCamera != null ? targetCamera.backgroundColor : Color.black;
        Color startFogCol = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float startVol = audioSource != null ? audioSource.volume : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            if (targetCamera != null)
                targetCamera.backgroundColor = Color.Lerp(startCamCol, targetCameraColor, t);

            RenderSettings.fogColor = Color.Lerp(startFogCol, targetFogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, targetFogDensity, t);

            if (audioSource != null)
                audioSource.volume = Mathf.Lerp(startVol, targetVolume, t);

            yield return null;
        }

        // Фиксация итоговых значений
        if (targetCamera != null) targetCamera.backgroundColor = targetCameraColor;
        RenderSettings.fogColor = targetFogColor;
        RenderSettings.fogDensity = targetFogDensity;
        if (audioSource != null) audioSource.volume = targetVolume;

        // Вызов финального события
        OnEffectCompleted();
    }

    /// <summary>
    /// Этот метод вызывается ровно в момент, когда эффект полностью завершился.
    /// Напиши сюда свою логику или используй UnityEvent из инспектора.
    /// </summary>
    protected virtual void OnEffectCompleted()
    {
        // Вызов событий, подтянутых через инспектор Unity
        onEffectComplete?.Invoke();

        // --- ВАША ЛОГИКА НИЖЕ ---
        Debug.Log("Эффект завершён! Происходит событие...");
        
        SceneService.Instance.State.CurrentLevel = _nextLevel;
        SceneService.Instance.State.DayConfig = _config;
        SceneService.Instance.LoadScene("Laboratory");
    }
}