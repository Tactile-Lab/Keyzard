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
    private string triggerImpact;
    private float fallbackDestroyDelay;

    private float timer = 0f;
    private bool hasImpacted;
    private bool isInitialized;
    private bool impactFinished;

    private Animator animator;
    private Collider2D projectileCollider;

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
        transform.position += (Vector3)(direction * vitesse * Time.deltaTime);

        // Rotation du projectile selon sa direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

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
    public void DemarrerImpact()
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
