using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sort : MonoBehaviour
{
    public enum LaunchTimingMode
    {
        Immediate,
        WaitForAnimationEvent
    }

    public string nomSort;
    public int damage;
    public float vitesse;

    [Header("UI & Display")]
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public Sprite demonstrationIllustration;

    [Header("Audio")]
    public SpellAudioConfig audioConfig;

    [Header("Launch Timing")]
    [SerializeField] private LaunchTimingMode launchTiming = LaunchTimingMode.Immediate;
    [SerializeField] private string launchAnimationTrigger = "LaunchSpell";
    [SerializeField] private float launchFallbackDelay = 0.2f;

    [Header("Launch VFX")]
    [SerializeField] private bool overrideLaunchColors;
    [SerializeField] private Color launchStartColor = Color.white;
    [SerializeField] private Color launchReleaseColor = Color.white;
    [SerializeField] private GameObject launchStartVfxPrefab;
    [SerializeField] private GameObject launchReleaseVfxPrefab;

    [Header("Impact VFX")]
    [SerializeField] private GameObject impactVfxPrefab;
    [SerializeField] private float impactVfxLifetime = 0.8f;
    [SerializeField] private bool attachImpactVfxToTarget;
    [SerializeField] private Vector3 impactVfxOffset = Vector3.zero;

    protected GameObject cible;
    protected AudioSource activeLoopSource;

    public Animator aniamtor;

    public LaunchTimingMode LaunchTiming => launchTiming;
    public string LaunchAnimationTrigger => launchAnimationTrigger;
    public float LaunchFallbackDelay => launchFallbackDelay;
    public bool OverrideLaunchColors => overrideLaunchColors;
    public Color LaunchStartColor => launchStartColor;
    public Color LaunchReleaseColor => launchReleaseColor;
    public GameObject LaunchStartVfxPrefab => launchStartVfxPrefab;
    public GameObject LaunchReleaseVfxPrefab => launchReleaseVfxPrefab;
    public GameObject ImpactVfxPrefab => impactVfxPrefab;
    public float ImpactVfxLifetime => impactVfxLifetime;
    public bool AttachImpactVfxToTarget => attachImpactVfxToTarget;
    public Vector3 ImpactVfxOffset => impactVfxOffset;

    // Lance le sort sur la cible la plus proche
    public virtual void LancerSort()
    {
        List<GameManager.EnemyEntry> ennemis = GameManager.Instance.list_enemies;

        if (ennemis.Count == 0)
        {
            Debug.Log("[Sort] Aucun ennemi dans la liste → destruction du sort");
            Destroy(gameObject);
            return;
        }

        cible = TrouverCibleProche(ennemis);

        if (cible != null)
        {
            LancerSortCible(cible);
        }
        else
        {
            Debug.LogWarning("[Sort] Aucune cible valide trouvée → destruction du sort");
            Destroy(gameObject);
        }
    }

    // Lance le sort sur une cible spécifique
    public virtual void LancerSortCible(GameObject cibleRef)
    {
        if (cibleRef == null)
        {
            DestroySort(cibleRef);
        }

        cible = cibleRef;

        // Jouer le son de lancement
        if (audioConfig != null)
        {
            audioConfig.Preload();
            audioConfig.PlayLaunchReleaseSFX();
            activeLoopSource = audioConfig.StartActiveLoop();
        }

        // Chaque coroutine suit sa propre cible
        StartCoroutine(DeplacementVersCible(cible));
    }

    // Déplacement vers la cible et disparition si cible détruite
    protected virtual IEnumerator DeplacementVersCible(GameObject target)
    {
        while (target != null && target.GetComponent<Enemy>() != null) // on ne regarde pas si elle bouge, juste si elle existe
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.transform.position,
                vitesse * Time.deltaTime
            );

            Vector2 direction = target.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        // La cible a disparu → on détruit le sort
        DestroySort(target);
    }

    // Trouver la cible la plus proche
    public GameObject TrouverCibleProche(List<GameManager.EnemyEntry> ennemis)
    {
        GameObject cibleProche = null;
        float distanceMin = Mathf.Infinity;

        foreach (GameManager.EnemyEntry entry in ennemis)
        {
            if (entry == null || entry.enemy == null) continue;

            float distance = Vector2.Distance(transform.position, entry.enemy.transform.position);
            if (distance < distanceMin)
            {
                distanceMin = distance;
                cibleProche = entry.enemy;
            }
        }

        return cibleProche;
    }

    protected virtual void OnImpact(GameObject target)
    {
        SpawnImpactVfx(target);

        if (audioConfig != null)
        {
            audioConfig.PlayImpactSFX();
            if (activeLoopSource != null)
            {
                AudioManager.Instance.StopLoop(activeLoopSource);
                activeLoopSource = null;
            }
        }
    }

    protected virtual void SpawnImpactVfx(GameObject target)
    {
        if (impactVfxPrefab == null)
        {
            return;
        }

        Vector3 basePosition = target != null ? target.transform.position : transform.position;
        Vector3 spawnPosition = basePosition + impactVfxOffset;
        Transform parent = attachImpactVfxToTarget && target != null ? target.transform : null;
        Quaternion spawnRotation = impactVfxPrefab.transform.rotation;

        GameObject vfxInstance = Instantiate(impactVfxPrefab, spawnPosition, spawnRotation, parent);
        if (vfxInstance == null)
        {
            return;
        }

        ImpactVfxAutoDestroy autoDestroy = vfxInstance.GetComponent<ImpactVfxAutoDestroy>();
        if (autoDestroy == null)
        {
            autoDestroy = vfxInstance.AddComponent<ImpactVfxAutoDestroy>();
        }

        autoDestroy.Initialize(impactVfxLifetime);
    }

    public virtual void DestroySort(GameObject cible)
    {
        // Jouer l'impact uniquement si ce n'est pas un auto-destroy
        if (cible != null && cible != gameObject)
        {
            OnImpact(cible);
        }

        if (activeLoopSource != null)
        {
            AudioManager.Instance.StopLoop(activeLoopSource);
            activeLoopSource = null;
        }

        Destroy(gameObject);
    }
}