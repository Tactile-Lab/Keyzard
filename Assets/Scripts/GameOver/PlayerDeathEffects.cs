using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathEffects : MonoBehaviour
{
    [Header("UI")]
    public Canvas canvas;
    public Image noirImage;         // fade noir
    public TMP_Text gameOverText;
    public float fadeDuration = 1f;

    [Header("References")]
    public Animator animator;
    public GameObject staff;
    public Camera mainCamera;
    public Transform player;

    [Header("Camera Zoom")]
    public float zoomAmount = 2f;      // zoom final
    public float zoomDuration = 0.5f;  // durée du zoom

    [Header("Shake (optionnel)")]
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.3f;

    [Header("Slow Motion")]
    public float slowMotionFactor = 0f; // 0 = freeze complet

    [Header("Delays")]
    public float postFadeDelay = 0.3f;  // pause avant Game Over
    

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
        // Freeze gameplay
        Time.timeScale = slowMotionFactor;

        // Désactiver le staff
        if (staff != null)
            staff.SetActive(false);

        // Jouer animation de mort
        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetTrigger("Die");
        }

        // Activer canvas
        canvas.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);

        // Joueur devant
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 2;

        // Lancer la cinématique
        

        StartCoroutine(DeathCinematic());
    }

    private IEnumerator DeathCinematic()
{
    float t = 0f;
    Time.timeScale = slowMotionFactor;

    // Zoom et position initiale
    float startSize = mainCamera.orthographicSize;
    float targetSize = startSize / zoomAmount;

    Vector3 startPos = mainCamera.transform.position;
    Vector3 targetPos = new Vector3(player.position.x, player.position.y, startPos.z);

    // Setup noir
    Color noirColor = noirImage.color;
    noirColor.a = 0f;
    noirImage.color = noirColor;
    noirImage.gameObject.SetActive(true);

    // Agrandir le spotlight avant fade
    noirImage.rectTransform.localScale = Vector3.one * 5f; // taille initiale grande

    // 🔥 Zoom + recentrage + fade noir + rétrécissement spotlight
    while (t < fadeDuration)
    {
        Time.timeScale = slowMotionFactor;
        t += Time.unscaledDeltaTime;
        float progress = t / fadeDuration;

        // easing pour fluidité
        float eased = progress * progress;

        // Zoom progressif
        mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, eased);

        // Recentrage progressif sur le joueur
        mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, eased);

        // Fade noir
        noirColor.a = Mathf.Lerp(0f, 1f, eased);
        noirImage.color = noirColor;

        // Spotlight rétrécissant vers scale = 1
        noirImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(5f, 1f, eased);

        yield return null;
    }

    // Assurer valeurs finales
    mainCamera.orthographicSize = targetSize;
    mainCamera.transform.position = targetPos;
    noirColor.a = 1f;
    noirImage.color = noirColor;
    noirImage.rectTransform.localScale = Vector3.one; // scale finale = 1

    // Pause dramatique
    yield return new WaitForSecondsRealtime(postFadeDelay);

    // 🔥 Game Over fade
    if (gameOverText != null)
    {
        Color textColor = gameOverText.color;
        textColor.a = 0f;
        gameOverText.color = textColor;
        gameOverText.gameObject.SetActive(true);

        float t2 = 0f;
        while (t2 < fadeDuration)
        {
            t2 += Time.unscaledDeltaTime;
            textColor.a = Mathf.Lerp(0f, 1f, t2 / fadeDuration);
            gameOverText.color = textColor;
            yield return null;
        }
    }
}
}