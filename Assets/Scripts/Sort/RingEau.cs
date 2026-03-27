using UnityEngine;

public class RingEau : Sort
{

    public float forceKnockback = 5f;
    private bool launchAudioPlayed;

    public override void LancerSort()
    {
        // RingEau est un sort de zone autour du joueur: pas de cible a suivre.
        // On pilote donc l'audio localement au lieu d'utiliser le flux cible de Sort.
        if (!launchAudioPlayed && audioConfig != null)
        {
            launchAudioPlayed = true;
            audioConfig.Preload();
            audioConfig.PlayLaunchReleaseSFX();
            activeLoopSource = audioConfig.StartActiveLoop();
        }
    }

    public override void LancerSortCible(GameObject cibleRef)
    {
        // Meme si un ennemi est selectionne, RingEau reste un sort de zone.
        LancerSort();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;

                enemy.ApplyKnockback(direction, forceKnockback);

                if (audioConfig != null)
                {
                    audioConfig.PlayImpactSFX();
                }
            }
        }
    }



    public void OnAnimationFinished()
    {
        if (activeLoopSource != null)
        {
            AudioManager.Instance.StopLoop(activeLoopSource);
            activeLoopSource = null;
        }

        Destroy(gameObject);
    }

    public override void DestroySort(GameObject cible)
    {
        // RingEau ne doit pas etre detruit par les callbacks d'impact des ennemis.
        // La fin de vie est pilotee par l'animation (OnAnimationFinished).
        return;
    }

    private void OnDisable()
    {
        if (activeLoopSource != null)
        {
            AudioManager.Instance.StopLoop(activeLoopSource);
            activeLoopSource = null;
        }
    }

    
}
