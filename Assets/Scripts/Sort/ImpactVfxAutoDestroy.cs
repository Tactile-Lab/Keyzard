using UnityEngine;
using System.Collections;

public class ImpactVfxAutoDestroy : MonoBehaviour
{
    private float fallbackLifetime;

    public void Initialize(float lifetime)
    {
        fallbackLifetime = lifetime;
        StartCoroutine(DestroyRoutine());
    }

    // Call this from an Animation Event on the last frame of the impact clip.
    public void OnImpactVfxAnimationFinished()
    {
        Destroy(gameObject);
    }

    private IEnumerator DestroyRoutine()
    {
        Animator animator = GetComponent<Animator>();
        yield return null;

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
