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
        SpawnLaunchPrefab(sortData != null ? sortData.LaunchReleaseVfxPrefab : null, defaultReleaseVfxPrefab);
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
        if (spawnedVfxLifetime > 0f)
        {
            Destroy(instance, spawnedVfxLifetime);
        }
    }
}
