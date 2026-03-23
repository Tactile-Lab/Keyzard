using UnityEngine;

public class SpellUnlockTrigger : MonoBehaviour
{
    [SerializeField] private Sort spellToUnlock;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool destroyAfterUnlock = true;

    private bool consumed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnlyOnce && consumed)
        {
            return;
        }

        if (!collision.CompareTag("Player"))
        {
            return;
        }

        if (SpellInventoryManager.Instance == null)
        {
            Debug.LogWarning("[SpellUnlockTrigger] SpellInventoryManager introuvable dans la scene.");
            return;
        }

        if (spellToUnlock == null)
        {
            Debug.LogWarning("[SpellUnlockTrigger] Aucun sort assigne au trigger de debloquage.");
            return;
        }

        bool unlocked = SpellInventoryManager.Instance.UnlockSpell(spellToUnlock);
        if (unlocked)
        {
            Debug.Log("[SpellUnlockTrigger] Sort debloque: " + spellToUnlock.nomSort);
        }

        consumed = true;

        if (destroyAfterUnlock)
        {
            Destroy(gameObject);
        }
    }
}
