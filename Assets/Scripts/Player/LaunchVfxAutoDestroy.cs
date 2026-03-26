using UnityEngine;

public class LaunchVfxAutoDestroy : MonoBehaviour
{
    private float fallbackLifetime;
    private bool preferAnimationEvent;
    private float eventSafetyTimeout;
    private bool animationFinished;

    public void Initialize(float lifetime, bool useAnimationEvent, float safetyTimeout)
    {
        fallbackLifetime = lifetime;
        preferAnimationEvent = useAnimationEvent;
        eventSafetyTimeout = Mathf.Max(0f, safetyTimeout);
        animationFinished = false;
        StartCoroutine(DestroyRoutine());
    }

    // Optional: call this via Animation Event on the last frame for deterministic cleanup.
    public void OnLaunchVfxAnimationFinished()
    {
        animationFinished = true;
    }

    private System.Collections.IEnumerator DestroyRoutine()
    {
        Animator animator = GetComponent<Animator>();

        // Let Animator enter its default state before querying state info.
        yield return null;

        if (preferAnimationEvent)
        {
            float elapsed = 0f;
            while (!animationFinished)
            {
                if (eventSafetyTimeout > 0f && elapsed >= eventSafetyTimeout)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
            yield break;
        }

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            float effectiveSpeed = Mathf.Abs(animator.speed * state.speed * state.speedMultiplier);

            if (!state.loop && state.length > 0f && effectiveSpeed > 0.0001f)
            {
                float wait = state.length / effectiveSpeed;
                yield return new WaitForSeconds(wait);
                Destroy(gameObject);
                yield break;
            }
        }

        if (fallbackLifetime > 0f)
        {
            yield return new WaitForSeconds(fallbackLifetime);
            Destroy(gameObject);
            yield break;
        }

        Destroy(gameObject);
    }
}
