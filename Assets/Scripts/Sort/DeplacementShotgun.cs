using UnityEngine;
using System.Collections;

/// <summary>
/// Gère le mouvement, la collision et l'impact d'une pellet de shotgun.
/// Classe externe pour être visible dans les Animation Events.
/// </summary>
public class DeplacementShotgun : MonoBehaviour
{
    private Vector2 direction;
    private float vitesse;
    private float delayVie;
    private string triggerImpact;
    private float fallbackDestroyDelay;

    private float timer = 0f;
    private float spinAngle;
    private bool hasImpacted;
    private bool isInitialized;
    private bool impactFinished;

    private Animator animator;
    private Collider2D projectileCollider;
    private TrailRenderer trailRenderer;
    private bool trailHiddenByOverlayUi;

    [Header("Visual polish")]
    [SerializeField] private float spinSpeed = 540f;
    [SerializeField] private bool autoAddTrail = true;
    [SerializeField] private float trailReenableDelay = 0.08f;
    [SerializeField] private float trailTime = 0.08f;
    [SerializeField] private float trailStartWidth = 0.09f;
    [SerializeField] private float trailEndWidth = 0.01f;
    [SerializeField] private int trailSortingOrderOffset = -5;

    [Header("Hit feedback")]
    [SerializeField] private float hitstopDuration = 0.03f;
    [SerializeField] private float hitstopScale = 0.05f;
    [SerializeField] private float shakeDuration = 0.08f;
    [SerializeField] private float shakeAmplitude = 0.06f;

    private static bool isHitstopActive;
    private static bool isShakeActive;
    private Coroutine delayedTrailEnableCoroutine;

    /// <summary>
    /// Initialise le projectile avec direction, vitesse et durée de vie.
    /// </summary>
    public void Initialiser(Vector2 dir, float vit, float delay, string triggerImpactAnim, float impactFallbackDelay)
    {
        direction = dir.normalized;
        vitesse = vit;
        delayVie = delay;
        triggerImpact = triggerImpactAnim;
        fallbackDestroyDelay = impactFallbackDelay;
        isInitialized = true;

        animator = GetComponent<Animator>();
        projectileCollider = GetComponent<Collider2D>();

        Vector3 startPos = transform.position;
        transform.position = new Vector3(startPos.x, startPos.y, 0f);

        ConfigureTrailRenderer();
    }

    void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        SyncTrailVisibilityWithOverlayUi();

        if (hasImpacted)
        {
            return;
        }

        // Déplacement
        Vector3 frameMove = (Vector3)(direction * vitesse * Time.deltaTime);
        transform.position += frameMove;
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y, 0f);

        // Rotation du projectile selon sa direction avec une rotation visuelle plus marquée.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        spinAngle += spinSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, angle + spinAngle);

        // Gestion de la durée de vie
        timer += Time.deltaTime;
        if (timer >= delayVie)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Déclenche l'animation d'impact et désactive la collision.
    /// </summary>
    public void DemarrerImpact(GameObject target)
    {
        if (hasImpacted)
        {
            return;
        }

        hasImpacted = true;

        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }

        StartCoroutine(PlayHitFeedback(target));

        if (animator != null && !string.IsNullOrEmpty(triggerImpact))
        {
            animator.SetTrigger(triggerImpact);

            // L'event d'animation doit être la voie principale de destruction.
            // Le fallback est seulement un garde-fou si l'event manque.
            StartCoroutine(EmergencyDestroyFallback());
            return;
        }

        Destroy(gameObject);
    }

    private void ConfigureTrailRenderer()
    {
        if (!autoAddTrail)
        {
            trailRenderer = GetComponent<TrailRenderer>();
            return;
        }

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailStartWidth;
        trailRenderer.endWidth = trailEndWidth;
        trailRenderer.minVertexDistance = 0.02f;
        trailRenderer.autodestruct = false;
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;

        if (trailRenderer.material == null)
        {
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        if (spriteRenderer != null)
        {
            trailRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            trailRenderer.sortingOrder = spriteRenderer.sortingOrder + trailSortingOrderOffset;
        }

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(baseColor, 0f),
                new GradientColorKey(new Color(baseColor.r, baseColor.g, baseColor.b), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trailRenderer.colorGradient = gradient;
    }

    private void SyncTrailVisibilityWithOverlayUi()
    {
        if (trailRenderer == null)
        {
            return;
        }

        bool shouldHideTrail = PauseMenuController.IsPauseMenuOpen || GlossaryToggleController.IsGlossaryOpen;
        if (trailHiddenByOverlayUi == shouldHideTrail)
        {
            return;
        }

        trailHiddenByOverlayUi = shouldHideTrail;

        if (shouldHideTrail)
        {
            if (delayedTrailEnableCoroutine != null)
            {
                StopCoroutine(delayedTrailEnableCoroutine);
                delayedTrailEnableCoroutine = null;
            }

            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }
        else
        {
            if (delayedTrailEnableCoroutine != null)
            {
                StopCoroutine(delayedTrailEnableCoroutine);
            }

            delayedTrailEnableCoroutine = StartCoroutine(EnableTrailWithDelay());
        }
    }

    private IEnumerator EnableTrailWithDelay()
    {
        float delay = Mathf.Max(0f, trailReenableDelay);
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        bool menuStillOpen = PauseMenuController.IsPauseMenuOpen || GlossaryToggleController.IsGlossaryOpen;
        if (!menuStillOpen && trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        delayedTrailEnableCoroutine = null;
    }

    private System.Collections.IEnumerator PlayHitFeedback(GameObject target)
    {
        yield return StartCoroutine(DoHitstop());
        StartCoroutine(DoCameraShake());
    }

    private System.Collections.IEnumerator DoHitstop()
    {
        if (isHitstopActive || hitstopDuration <= 0f)
        {
            yield break;
        }

        isHitstopActive = true;

        float originalScale = Time.timeScale;
        float originalFixedDelta = Time.fixedDeltaTime;

        Time.timeScale = Mathf.Clamp(hitstopScale, 0.01f, 1f);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(hitstopDuration);

        Time.timeScale = originalScale;
        Time.fixedDeltaTime = originalFixedDelta;
        isHitstopActive = false;
    }

    private System.Collections.IEnumerator DoCameraShake()
    {
        Camera cam = Camera.main;
        if (cam == null || shakeDuration <= 0f || shakeAmplitude <= 0f || isShakeActive)
        {
            yield break;
        }

        isShakeActive = true;

        Transform camTransform = cam.transform;
        Vector3 previousOffset = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector2 jitter = Random.insideUnitCircle * shakeAmplitude;
            camTransform.position -= previousOffset;
            previousOffset = new Vector3(jitter.x, jitter.y, 0f);
            camTransform.position += previousOffset;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        camTransform.position -= previousOffset;
        isShakeActive = false;
    }

    private System.Collections.IEnumerator EmergencyDestroyFallback()
    {
        float wait = Mathf.Max(1f, fallbackDestroyDelay);
        yield return new WaitForSeconds(wait);

        if (!impactFinished)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Appelée à la fin de l'animation d'impact (via Invoke ou Animation Event).
    /// Accessible depuis les Animation Events car c'est une classe externe.
    /// </summary>
    public void OnImpactAnimationFinished()
    {
        impactFinished = true;
        Destroy(gameObject);
    }
}
