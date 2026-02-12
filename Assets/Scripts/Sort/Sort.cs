using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sort : MonoBehaviour
{
    public string nomSort;
    public int damage;
    public float vitesse;

    protected GameObject cible;

    public Animator aniamtor;

    // Méthode qui cherche la cible la plus proche et lance le sort dessus
    public virtual void LancerSort()
    {
        // Récupère la liste d'EnemyEntry depuis le GameManager
        List<GameManager.EnemyEntry> ennemis = GameManager.Instance.list_enemies;

        // Cherche la cible la plus proche
        GameObject cible = TrouverCibleProche(ennemis);
        if (cible != null)
        {
            LancerSortCible(cible);
        }
    }

    // Méthode qui fait suivre le sort vers une cible spécifique
    public virtual void LancerSortCible(GameObject cible)
    {
        if (cible == null) return;
        this.cible = cible;
        StartCoroutine(DeplacementVersCible(cible));
    }

    // Coroutine qui déplace le sort vers la cible en continu
    protected virtual IEnumerator DeplacementVersCible(GameObject cible)
    {
        while (cible != null)
        {
            // Déplacement
            transform.position = Vector2.MoveTowards(
                transform.position,
                cible.transform.position,
                vitesse * Time.deltaTime
            );

            // Rotation instantanée vers la cible
            Vector2 direction = cible.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }
    }


    // Méthode pour trouver la cible la plus proche parmi les EnemyEntry
    public GameObject TrouverCibleProche(List<GameManager.EnemyEntry> ennemis)
    {
        GameObject cibleProche = null;
        float distanceMin = Mathf.Infinity;

        foreach (GameManager.EnemyEntry entry in ennemis)
        {
            if (entry == null || entry.enemy == null) continue; // sécurité

            float distance = Vector2.Distance(transform.position, entry.enemy.transform.position);
            if (distance < distanceMin)
            {
                distanceMin = distance;
                cibleProche = entry.enemy; // on ne garde que le GameObject
            }
        }

        return cibleProche;
    }

    public virtual void DestroySort(GameObject cible)
    {
        Destroy(gameObject);
    }

    
}
