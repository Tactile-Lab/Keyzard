using UnityEngine;

public class SpellUnlockTrigger : MonoBehaviour
{
    [SerializeField] private Sort spellToUnlock;
    [SerializeField] private bool useRandomSpell = true;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool destroyAfterUnlock = true;

    private bool consumed;

    private void OnTriggerEnter2D(Collider2D collision)
{
    if (triggerOnlyOnce && consumed) return;

    if (!collision.CompareTag("Player")) return;

    if (SpellInventoryManager.Instance == null)
    {
        Debug.LogWarning("SpellInventoryManager introuvable.");
        return;
    }

    Sort spell = null;

    if (useRandomSpell)
    {
        spell = SpellInventoryManager.Instance.GetRandomLockedSpell();

        if (spell == null)
        {
            Debug.Log("Tous les sorts sont déjà débloqués.");
            return;
        }
    }
    else
    {
        spell = spellToUnlock;

        if (spell == null)
        {
            Debug.LogWarning("Aucun sort assigné.");
            return;
        }
    }

    bool unlocked = SpellInventoryManager.Instance.UnlockSpell(spell);

    if (unlocked)
    {
        Debug.Log("Sort débloqué : " + spell.nomSort);
    }

    consumed = true;

    if (destroyAfterUnlock)
    {
        Destroy(gameObject);
    }
}
}
