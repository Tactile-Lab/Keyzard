using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class BlackFadeEffect : MonoBehaviour
{
    [Header("UI")]
    public Image noirImage;
    public float fadeDuration = 1f;

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

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if (noirImage != null)
        {
            noirImage.gameObject.SetActive(false);
            Color c = noirImage.color;
            c.a = 0f;
            noirImage.color = c;
        }
    }

    // FADE OUT : alpha 0 → 1
    public void PlayFadeOut(Action onComplete = null)
    {
        if (noirImage == null)
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
    }

    // FADE IN : alpha 1 → 0
    public void PlayFadeIn(Action onComplete = null)
    {
        if (noirImage == null)
        {
            onComplete?.Invoke();
            return;
        }
        StartCoroutine(FadeCoroutine(1f, 0f, onComplete));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, Action onComplete)
    {
        noirImage.gameObject.SetActive(true);
        Color c = noirImage.color;
        float startTime = Time.realtimeSinceStartup;

        while (true)
        {
            float elapsed = Time.realtimeSinceStartup - startTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = Mathf.Lerp(startAlpha, endAlpha, progress);
            noirImage.color = c;

            if (progress >= 1f)
                break;

            yield return null;
        }

        c.a = endAlpha;
        noirImage.color = c;

        if (endAlpha == 0f)
            noirImage.gameObject.SetActive(false);

        onComplete?.Invoke();
        Debug.Log($"Fade {(startAlpha < endAlpha ? "Out" : "In")} finished in {Time.realtimeSinceStartup - startTime:F2}s");
    }
}