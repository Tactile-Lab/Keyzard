using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class BlackFadeEffect : MonoBehaviour
{
    [Header("UI")]
    public Image noirImage;
    public float fadeDuration = 1f;

    [Header("Scale")]
    public Vector3 minScale = Vector3.one * 1f;
    public Vector3 maxScale = Vector3.one * 6f;

    private Canvas canvas;

    public static BlackFadeEffect Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        if (noirImage != null)
            noirImage.gameObject.SetActive(false);

        SetCanvasModeByScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void SetCanvasModeByScene(int sceneIndex)
    {
        bool overlay = sceneIndex == 0;
        canvas.renderMode = overlay ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = overlay ? null : Camera.main;
    }

    private IEnumerator ReassignCameraCoroutine()
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            yield break;

        while (Camera.main == null)
            yield return null;

        canvas.worldCamera = Camera.main;
    }

    // FADE OUT (avant load) : scale max→min, alpha ne change pas
    public IEnumerator PlayExitEffectCoroutine(Action onComplete)
    {
        yield return ReassignCameraCoroutine();

        if (noirImage == null) { onComplete?.Invoke(); yield break; }

        noirImage.gameObject.SetActive(true);
        noirImage.rectTransform.localScale = maxScale;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / fadeDuration;

            // Scale animation uniquement
            noirImage.rectTransform.localScale = Vector3.Lerp(maxScale, minScale, progress);

            yield return null;
        }

        noirImage.rectTransform.localScale = minScale;

        onComplete?.Invoke();
    }

    // FADE IN (après load) : scale min→max, alpha ne change pas
    public void PlayEntranceEffect(Action onComplete = null)
    {
        StartCoroutine(PlayEntranceCoroutine(onComplete));
    }

    private IEnumerator PlayEntranceCoroutine(Action onComplete)
    {
        yield return ReassignCameraCoroutine();

        if (noirImage == null) { onComplete?.Invoke(); yield break; }

        noirImage.gameObject.SetActive(true);
        noirImage.rectTransform.localScale = minScale;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / fadeDuration;

            // Scale animation uniquement
            noirImage.rectTransform.localScale = Vector3.Lerp(minScale, maxScale, progress);

            yield return null;
        }

        noirImage.rectTransform.localScale = maxScale;
        noirImage.gameObject.SetActive(false);

        onComplete?.Invoke();
    }
}