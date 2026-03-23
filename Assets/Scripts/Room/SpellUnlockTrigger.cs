using UnityEngine;
using System.Collections;

public class SpellUnlockTrigger : MonoBehaviour
{
    private Sort spellToUnlock;

    [SerializeField] private SpellUnlockVisual visual;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool destroyAfterUnlock = true;
    [SerializeField] private float pickupDelay = 5f; // temps au-dessus de la tête

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

        // Commence la coroutine de pickup au-dessus de la tête
        StartCoroutine(PickupAboveHead(collision.transform));
        consumed = true;
    }

    private IEnumerator PickupAboveHead(Transform player)
    {
        float timer = 0f;
        Vector3 offset = new Vector3(0, 1.2f, 0); // au-dessus de la tête
        Vector3 startPos = visual.transform.position;

        while (timer < pickupDelay)
        {
            timer += Time.deltaTime;

            // Faire flotter le sort au-dessus de la tête du joueur
            visual.transform.position = Vector3.Lerp(startPos, player.position + offset, timer / pickupDelay);

            yield return null;
        }

        // Débloquer le sort
        bool unlocked = SpellInventoryManager.Instance.UnlockSpell(spellToUnlock);
        if (unlocked)
            Debug.Log($"Sort débloqué : {spellToUnlock.nomSort}");

        // Retirer le visuel
        if (visual != null)
            visual.gameObject.SetActive(false);

        // Détruire le trigger si nécessaire
        if (destroyAfterUnlock)
            Destroy(gameObject);
    }
}