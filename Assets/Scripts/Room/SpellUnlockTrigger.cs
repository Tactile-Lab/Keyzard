using UnityEngine;

public class SpellUnlockTrigger : MonoBehaviour
{
    private Sort spellToUnlock;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool destroyAfterUnlock = true;

    private bool consumed;

    // Permet à la Room d'assigner le sort dynamiquement
    public void SetSpell(Sort spell)
    {
        spellToUnlock = spell;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnlyOnce && consumed)
            return;

        if (!collision.CompareTag("Player"))
            return;

        if (SpellInventoryManager.Instance == null)
        {
            Debug.LogWarning("[SpellUnlockTrigger] Manager introuvable.");
            return;
        }

        if (spellToUnlock == null)
        {
            Debug.LogError("[SpellUnlockTrigger] spellToUnlock est NULL !");
            return;
        }

        bool unlocked = SpellInventoryManager.Instance.UnlockSpell(spellToUnlock);

        if (unlocked)
        {
            Debug.Log("[SpellUnlockTrigger] Sort débloqué: " + spellToUnlock.nomSort);
        }

        consumed = true;

        if (destroyAfterUnlock)
        {
            Destroy(gameObject);
        }
    }
}