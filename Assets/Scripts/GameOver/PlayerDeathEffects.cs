using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathEffects : MonoBehaviour
{
    [Header("UI")]
    public Canvas canvas;           // Canvas classique
    public Image noirImage;         // Image noire dans le Canvas
    public TMP_Text gameOverText;   // GameOver UI dans le même Canvas
    public float fadeDuration = 1f; // durée du fade

    [Header("References")]
    public Animator animator;       // Animator du joueur
    public GameObject staff;        // Staff à désactiver
    public Camera mainCamera;       // Caméra pour zoom instantané
    public Transform player;        // Transform du joueur

    [Header("Camera Zoom")]
    public float zoomAmount = 2f;   // Zoom final (progressif)

    private void OnEnable()
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Died += OnPlayerDied;
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
        Time.timeScale = 0f;

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

        // S'assurer que le joueur est devant
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = 2;

        // Lancer la coroutine complète
        StartCoroutine(DeathFullSequence());
    }

    private IEnumerator DeathFullSequence()
    {
        float t = 0f;

        // Valeurs initiales du zoom
        float startSize = mainCamera.orthographicSize;
        float targetSize = startSize / zoomAmount;

        // Setup noir
        Color noirColor = noirImage.color;
        noirColor.a = 0f;
        noirImage.color = noirColor;
        noirImage.gameObject.SetActive(true);

        // 🔥 ZOOM + FADE NOIR EN MÊME TEMPS
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / fadeDuration;

            // easing stylé (optionnel)
            float eased = progress * progress;

            // Zoom progressif
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, eased);

            // Fade noir
            noirColor.a = Mathf.Lerp(0f, 1f, eased);
            noirImage.color = noirColor;

            yield return null;
        }

        // Assurer valeurs finales
        mainCamera.orthographicSize = targetSize;
        noirColor.a = 1f;
        noirImage.color = noirColor;

        // Petite pause dramatique
        yield return new WaitForSecondsRealtime(0.3f);

        // 🔥 FADE DU GAME OVER APRES TOUT
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