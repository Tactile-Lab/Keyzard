using UnityEngine;

/// <summary>
/// Gère le mouvement, la collision et l'impact d'une pellet de shotgun.
/// Classe externe pour être visible dans les Animation Events.
/// </summary>
public class DeplacementShotgun : MonoBehaviour
{
    private Vector2 direction;
    private float vitesse;
    private float delayVie;
    private int initialDamage;
    private string triggerImpact;
    private float fallbackDestroyDelay;

    private float timer = 0f;
    private float travelledDistance;
    private float spinAngle;
    private bool hasImpacted;
    private bool isInitialized;
    private bool impactFinished;

    private Animator animator;
    private Collider2D projectileCollider;
    private ShotGunFeu shotgunSort;

    [Header("Damage falloff")]
    [SerializeField] private float damageFalloffDistance = 3.5f;
    [SerializeField] private float minDamageRatio = 0.55f;

    [Header("Visual polish")]
    [SerializeField] private float spinSpeed = 540f;
    [SerializeField] private bool autoAddTrail = true;
    [SerializeField] private float trailTime = 0.08f;
    [SerializeField] private float trailStartWidth = 0.09f;
    [SerializeField] private float trailEndWidth = 0.01f;

    [Header("Hit feedback")]
    [SerializeField] private float hitstopDuration = 0.03f;
    [SerializeField] private float hitstopScale = 0.05f;
    [SerializeField] private float shakeDuration = 0.08f;
    [SerializeField] private float shakeAmplitude = 0.06f;
    [SerializeField] private float blinkDuration = 0.06f;

    private static bool isHitstopActive;

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
        shotgunSort = GetComponent<ShotGunFeu>();
        initialDamage = shotgunSort != null ? shotgunSort.damage : 1;

        ConfigureTrailRenderer();
    }

    void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        if (hasImpacted)
        {
            return;
        }

        // Déplacement
        Vector3 frameMove = (Vector3)(direction * vitesse * Time.deltaTime);
        transform.position += frameMove;
        travelledDistance += frameMove.magnitude;

        ApplyDistanceDamageFalloff();

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

    private void ApplyDistanceDamageFalloff()
    {
        if (shotgunSort == null)
        {
            return;
        }

        float safeDistance = Mathf.Max(0.01f, damageFalloffDistance);
        float t = Mathf.Clamp01(travelledDistance / safeDistance);
        float ratio = Mathf.Lerp(1f, Mathf.Clamp(minDamageRatio, 0.05f, 1f), t);

        shotgunSort.damage = Mathf.Max(1, Mathf.RoundToInt(initialDamage * ratio));
    }

    private void ConfigureTrailRenderer()
    {
        if (!autoAddTrail)
        {
            return;
        }

        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }

        trail.time = trailTime;
        trail.startWidth = trailStartWidth;
        trail.endWidth = trailEndWidth;
        trail.minVertexDistance = 0.02f;
        trail.autodestruct = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        if (trail.material == null)
        {
            trail.material = new Material(Shader.Find("Sprites/Default"));
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(baseColor, 0f),
                new GradientColorKey(new Color(baseColor.r, baseColor.g, baseColor.b), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = gradient;
    }

    private System.Collections.IEnumerator PlayHitFeedback(GameObject target)
    {
        yield return StartCoroutine(DoHitstop());
        StartCoroutine(DoCameraShake());

        if (target != null)
        {
            StartCoroutine(BlinkTarget(target));
        }
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
        if (cam == null || shakeDuration <= 0f || shakeAmplitude <= 0f)
        {
            yield break;
        }

        Transform camTransform = cam.transform;
        Vector3 startPos = camTransform.position;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector2 jitter = Random.insideUnitCircle * shakeAmplitude;
            camTransform.position = startPos + new Vector3(jitter.x, jitter.y, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        camTransform.position = startPos;
    }

    private System.Collections.IEnumerator BlinkTarget(GameObject target)
    {
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>();
        if (renderers == null || renderers.Length == 0)
        {
            yield break;
        }

        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] = renderers[i].color;
                renderers[i].color = Color.white;
            }
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, blinkDuration));

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = originalColors[i];
            }
        }
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
