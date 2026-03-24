using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathEffects : MonoBehaviour
{
    [Header("UI")]
    public Canvas canvas;           // Ton Canvas classique
    public Image noirImage;         // Image noire dans le Canvas
    public TMP_Text gameOverText;       // GameOver UI dans le même Canvas
    public float fadeDuration = 1f;

    [Header("References")]
    public Animator animator;       // Animator du joueur
    public GameObject staff;        // Staff à désactiver
    public Camera mainCamera;       // Caméra pour zoom instantané
    public Transform player;        // Transform du joueur

    [Header("Camera Zoom")]
    public float zoomAmount = 2f;   // Zoom instantané

    private void OnEnable()
    {
        // Récupérer le PlayerHealth (ex : attaché au même GameObject)
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Died += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        // Se désabonner pour éviter les erreurs si l’objet est détruit
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Died -= OnPlayerDied;
        }
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

        // Zoom caméra instantané sur le joueur
        if (mainCamera != null && player != null)
        {
            mainCamera.transform.position = new Vector3(player.position.x, player.position.y, mainCamera.transform.position.z);
            mainCamera.orthographicSize /= zoomAmount;
        }

        canvas.gameObject.SetActive(true);
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 2;

        // Lancer fade UI
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Fade Image noire
        if (noirImage != null)
        {
            Color noirColor = noirImage.color;
            noirColor.a = 0f;
            noirImage.color = noirColor;
            noirImage.gameObject.SetActive(true);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                noirColor.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
                noirImage.color = noirColor;
                yield return null;
            }
        }

        // Fade GameOver Text
        if (gameOverText != null)
        {
            Color textColor = gameOverText.color;
            textColor.a = 0f;
            gameOverText.color = textColor;
            gameOverText.gameObject.SetActive(true);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                textColor.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
                gameOverText.color = textColor;
                yield return null;
            }
        }
    }
}