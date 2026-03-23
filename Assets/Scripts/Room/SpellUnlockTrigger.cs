using Unity.VisualScripting;
using UnityEngine;

public class SpellUnlockTrigger : MonoBehaviour
{
    private Sort spellToUnlock;
    [SerializeField] private SpellUnlockVisual visual;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool destroyAfterUnlock = true;

    private bool consumed = false;

    public void SetSpell(Sort spell)
    {
        spellToUnlock = spell;
        if (visual != null && spell != null)
        {
            visual.SetSprite(spell.icon); // assigne le sprite au visuel
        }
        gameObject.SetActive(true); // active le trigger seulement après assignation
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnlyOnce && consumed) return;
        if (!collision.CompareTag("Player")) return;

        if (spellToUnlock == null)
        {
            Debug.LogError("[SpellUnlockTrigger] Aucun sort assigné !");
            return;
        }

        if (SpellInventoryManager.Instance == null)
        {
            Debug.LogError("[SpellUnlockTrigger] SpellInventoryManager introuvable !");
            return;
        }

        bool unlocked = SpellInventoryManager.Instance.UnlockSpell(spellToUnlock);
        if (unlocked)
        {
            Debug.Log($"Sort débloqué : {spellToUnlock.nomSort}");
        }

        if (visual != null)
        {
            visual.gameObject.SetActive(false); 
        }

        consumed = true;

        if (destroyAfterUnlock)
            Destroy(gameObject);
    }
}