using UnityEngine;

public class StaffTipLaunchVFX : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private SpriteRenderer launchSpriteRenderer;
    [SerializeField] private Animator launchAnimator;

    [Header("Animation Triggers")]
    [SerializeField] private string startTrigger = "LaunchStart";
    [SerializeField] private string releaseTrigger = "LaunchRelease";

    [Header("Optional Prefab Fallback")]
    [SerializeField] private GameObject defaultStartVfxPrefab;
    [SerializeField] private GameObject defaultReleaseVfxPrefab;
    [SerializeField] private float spawnedVfxLifetime = 1.2f;
    [SerializeField] private bool autoDestroySpawnedVfxAtAnimationEnd = true;

    public void PlayLaunchStart(Sort sortData)
    {
        ApplyTint(sortData, isStart: true);
        TriggerAnimation(startTrigger);
        SpawnLaunchPrefab(sortData != null ? sortData.LaunchStartVfxPrefab : null, defaultStartVfxPrefab);
    }

    public void PlayLaunchRelease(Sort sortData)
    {
        ApplyTint(sortData, isStart: false);
        TriggerAnimation(releaseTrigger);

        GameObject releasePrefab = sortData != null ? sortData.LaunchReleaseVfxPrefab : null;
        GameObject startPrefab = sortData != null ? sortData.LaunchStartVfxPrefab : null;
        GameObject fallbackPrefab = defaultReleaseVfxPrefab != null ? defaultReleaseVfxPrefab : defaultStartVfxPrefab;

        // In immediate mode, release is often the only hook called.
        // If no release prefab is configured, reuse start prefab to avoid silent no-VFX casts.
        SpawnLaunchPrefab(releasePrefab != null ? releasePrefab : startPrefab, fallbackPrefab);
    }

    private void ApplyTint(Sort sortData, bool isStart)
    {
        if (launchSpriteRenderer == null)
        {
            return;
        }

        if (sortData != null && sortData.OverrideLaunchColors)
        {
            launchSpriteRenderer.color = isStart ? sortData.LaunchStartColor : sortData.LaunchReleaseColor;
            return;
        }

        launchSpriteRenderer.color = Color.white;
    }

    private void TriggerAnimation(string trigger)
    {
        if (launchAnimator == null || string.IsNullOrEmpty(trigger))
        {
            return;
        }

        launchAnimator.SetTrigger(trigger);
    }

    private void SpawnLaunchPrefab(GameObject sortSpecificPrefab, GameObject defaultPrefab)
    {
        GameObject prefabToSpawn = sortSpecificPrefab != null ? sortSpecificPrefab : defaultPrefab;
        if (prefabToSpawn == null)
        {
            return;
        }

        GameObject instance = Instantiate(prefabToSpawn, transform.position, Quaternion.identity, transform);

        if (autoDestroySpawnedVfxAtAnimationEnd)
        {
            LaunchVfxAutoDestroy autoDestroy = instance.GetComponent<LaunchVfxAutoDestroy>();
            if (autoDestroy == null)
            {
                autoDestroy = instance.AddComponent<LaunchVfxAutoDestroy>();
            }

            autoDestroy.Initialize(spawnedVfxLifetime);
            return;
        }

        if (spawnedVfxLifetime > 0f)
        {
            Destroy(instance, spawnedVfxLifetime);
        }
    }
}

public class LaunchVfxAutoDestroy : MonoBehaviour
{
    private float fallbackLifetime;

    public void Initialize(float lifetime)
    {
        fallbackLifetime = lifetime;
        StartCoroutine(DestroyRoutine());
    }

    private System.Collections.IEnumerator DestroyRoutine()
    {
        Animator animator = GetComponent<Animator>();

        // Let Animator enter its default state before querying state info.
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
