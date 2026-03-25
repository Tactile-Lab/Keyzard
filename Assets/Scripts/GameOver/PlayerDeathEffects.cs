using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathEffects : MonoBehaviour
{
    [Header("UI")]
    public Canvas canvas;
    public Image noirImage;
    public TMP_Text gameOverText;
    public float fadeDuration = 1f;

    [Header("References")]
    public Animator animator;
    public GameObject staff;
    public Camera mainCamera;
    public Transform player;

    [Header("Camera Zoom")]
    public float zoomAmount = 2f;
    public float zoomDuration = 0.5f;

    [Header("Shake (optionnel)")]
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.3f;

    [Header("Slow Motion")]
    public float slowMotionFactor = 0f;

    [Header("Delays")]
    public float postFadeDelay = 0.3f;

    [Header("Debug")]
    public bool allowEarlyInput = false;

    public static bool CanInteract = false;

    private void OnEnable()
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Died += OnPlayerDied;

        canvas.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Died -= OnPlayerDied;
    }

    public void OnPlayerDied()
    {
        CanInteract = false; // ❌ bloque les inputs

        Time.timeScale = slowMotionFactor;

        if (staff != null)
            staff.SetActive(false);

        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetTrigger("Die");
        }

        canvas.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 2;

        StartCoroutine(DeathCinematic());
    }

    private IEnumerator DeathCinematic()
    {
        float t = 0f;

        float startSize = mainCamera.orthographicSize;
        float targetSize = startSize / zoomAmount;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = new Vector3(player.position.x, player.position.y, startPos.z);

        Color noirColor = noirImage.color;
        noirColor.a = 0f;
        noirImage.color = noirColor;
        noirImage.gameObject.SetActive(true);

        noirImage.rectTransform.localScale = Vector3.one * 5f;

        // 🔥 Zoom + fade noir
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / fadeDuration;
            float eased = progress * progress;

            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, eased);
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, eased);

            noirColor.a = Mathf.Lerp(0f, 1f, eased);
            noirImage.color = noirColor;

            noirImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(5f, 1f, eased);

            yield return null;
        }

        // valeurs finales
        mainCamera.orthographicSize = targetSize;
        mainCamera.transform.position = targetPos;
        noirColor.a = 1f;
        noirImage.color = noirColor;
        noirImage.rectTransform.localScale = Vector3.one;

        // pause dramatique
        yield return new WaitForSecondsRealtime(postFadeDelay);

        // 🔥 Game Over fade
        if (gameOverText != null)
        {
            Color textColor = gameOverText.color;
            textColor.a = 0f;
            gameOverText.color = textColor;
            gameOverText.gameObject.SetActive(true);

            // ✅ autorisation anticipée (DEBUG ONLY)
            if (allowEarlyInput)
            {
                CanInteract = true;
            }

            float t2 = 0f;
            while (t2 < fadeDuration)
            {
                t2 += Time.unscaledDeltaTime;
                textColor.a = Mathf.Lerp(0f, 1f, t2 / fadeDuration);
                gameOverText.color = textColor;
                yield return null;
            }
        }

        // ✅ autorisation normale (fin complète)
        CanInteract = true;
    }
}