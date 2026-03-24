using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sort : MonoBehaviour
{
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

    protected GameObject cible;
    protected AudioSource activeLoopSource;

    public Animator aniamtor;

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

    private IEnumerator LancerSortDelayed()
    {
        yield return null; // attend 1 frame

        List<GameManager.EnemyEntry> ennemis = GameManager.Instance.list_enemies;

        cible = TrouverCibleProche(ennemis);

        if (cible != null)
        {
            LancerSortCible(cible);
        }
        else
        {
            Debug.LogWarning("[Sort] Toujours aucune cible après délai !");
        }
    }

    // Lance le sort sur une cible spécifique
    public virtual void LancerSortCible(GameObject cibleRef)
    {
        if (cibleRef == null)
        {
            DestroySort(gameObject);
        }

        cible = cibleRef;

        // Jouer le son de lancement
        if (audioConfig != null)
        {
            audioConfig.Preload();
            audioConfig.PlayLaunchSFX();
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
        DestroySort(gameObject);
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