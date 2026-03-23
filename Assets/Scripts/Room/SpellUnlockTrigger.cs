using UnityEngine;
using System.Collections;

public class SpellUnlockTrigger : MonoBehaviour
{
    private Sort spellToUnlock;

    [SerializeField] private SpellUnlockVisual visual;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool destroyAfterUnlock = true;
    [SerializeField] private float pickupDelay = 3f; // temps au-dessus de la tête
    [SerializeField] private float acquireAnimDuration = 0.8f; // durée de l'anim AcquireSpell

    private bool consumed = false;

    public void SetSpell(Sort spell)
    {
        spellToUnlock = spell;

        if (visual != null && spell != null)
        {
            visual.SetSprite(spell.icon);
            visual.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnlyOnce && consumed) return;
        if (!collision.CompareTag("Player")) return;

        if (spellToUnlock == null || SpellInventoryManager.Instance == null)
        {
            Debug.LogError("[SpellUnlockTrigger] Sort ou SpellInventoryManager manquant !");
            return;
        }

        consumed = true;

        // 🔹 Lancer l'anim avant que le spell aille au-dessus de la tête
        Animator playerAnimator = collision.GetComponent<Animator>();
        if (playerAnimator != null)
           playerAnimator.SetTrigger("acquireSpell");

        bool unlocked = SpellInventoryManager.Instance.UnlockSpell(spellToUnlock);
        if (unlocked)
            Debug.Log($"Sort débloqué : {spellToUnlock.nomSort}");

        // 🔹 Ensuite lancer la coroutine existante
        StartCoroutine(PickupAboveHead(collision.transform));
    }

    private IEnumerator PickupAboveHead(Transform player)
    {
        // Attendre la fin de l'animation si nécessaire
        yield return new WaitForSeconds(acquireAnimDuration);

        Vector3 offset = new Vector3(0, 1f, 0); // juste au-dessus de la tête
        visual.transform.position = player.position + offset; // position immédiate

        float timer = 0f;
        while (timer < pickupDelay)
        {
            timer += Time.deltaTime;

            // garder le visuel au-dessus de la tête même si le joueur bouge
            visual.transform.position = player.position + offset;

            yield return null;
        }

        // Retirer le visuel
        if (visual != null)
            visual.gameObject.SetActive(false);

        // Détruire le trigger si nécessaire
        if (destroyAfterUnlock)
            Destroy(gameObject);
    }
}