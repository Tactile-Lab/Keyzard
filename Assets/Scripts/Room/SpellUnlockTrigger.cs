using UnityEngine;
using System.Collections;
using TMPro;

public class SpellUnlockTrigger : MonoBehaviour
{
    private Sort spellToUnlock;

    [SerializeField] private SpellUnlockVisual visual;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private bool destroyAfterUnlock = true;
    [SerializeField] private float pickupDelay = 1f; // temps au-dessus de la tête
    [SerializeField] private float animBeforeEnd = 0.1f;

    [SerializeField] private GameObject banière;
    [SerializeField] private TMP_Text textSortUnlock;

    private bool consumed = false;

    public void Awake()
    {
        if (banière != null )
        {
            banière.SetActive(false);
        }
    }

    public void SetSpell(Sort spell)
    {
        spellToUnlock = spell;

        if (visual != null && spell != null)
        {
            visual.SetSprite(spell.icon);
            visual.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);

        if(textSortUnlock != null)
        {
            textSortUnlock.text = spell.nomSort.ToUpper();
        }
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

        GetComponent<Collider2D>().enabled = false;

        consumed = true;

        bool unlocked = SpellInventoryManager.Instance.UnlockSpell(spellToUnlock);
        if (unlocked)
            Debug.Log($"Sort débloqué : {spellToUnlock.nomSort}");
        
        banière.SetActive(true);

        // 🔹 Ensuite lancer la coroutine existante
        StartCoroutine(PickupAboveHead(collision.transform));
    }

    private IEnumerator PickupAboveHead(Transform player)
    {
        Vector3 offset = new Vector3(0, 1f, 0);

        Animator playerAnimator = player.GetComponent<Animator>();
        bool animTriggered = false;

        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup < startTime + pickupDelay)
        {
            visual.transform.position = player.position + offset;

            if (!animTriggered && Time.realtimeSinceStartup >= startTime + pickupDelay - animBeforeEnd)
            {
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("acquireSpell");
                }

                animTriggered = true;
            }

            yield return null;
        }

        // cacher le visuel
        if (visual != null)
            visual.gameObject.SetActive(false);
        if (banière != null)
            banière.SetActive(false);

        if (destroyAfterUnlock)
            Destroy(gameObject);
    }
}