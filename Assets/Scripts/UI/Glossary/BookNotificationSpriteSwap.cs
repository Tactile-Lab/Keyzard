using UnityEngine;
using UnityEngine.UI;

public class BookNotificationSpriteSwap : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Image targetImage;

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightedSprite;

    [Header("First Time Glow")]
    [SerializeField] private bool glowOnFirstLaunch = true;

    private SpellInventoryManager spellInventory;
    private bool isBound;
    private bool firstGlowConsumedThisSession;
    private bool lastGlossaryOpen;
    private bool hasSeenClosedState;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        firstGlowConsumedThisSession = false;
        lastGlossaryOpen = GlossaryToggleController.IsGlossaryOpen;
        hasSeenClosedState = !lastGlossaryOpen;
        TryBindManager();
        RefreshVisual();
    }

    private void OnDisable()
    {
        UnbindManager();
    }

    private void Update()
    {
        bool glossaryOpen = GlossaryToggleController.IsGlossaryOpen;
        if (!glossaryOpen)
        {
            hasSeenClosedState = true;
        }

        bool openedThisFrame = glossaryOpen && !lastGlossaryOpen;
        if (glowOnFirstLaunch && !firstGlowConsumedThisSession && hasSeenClosedState && openedThisFrame)
        {
            firstGlowConsumedThisSession = true;
            RefreshVisual();
        }

        lastGlossaryOpen = glossaryOpen;

        // Bootstrap managers can appear slightly after this view in some scene load orders.
        if (!isBound)
        {
            TryBindManager();
            RefreshVisual();
        }
    }

    private void TryBindManager()
    {
        if (isBound)
        {
            return;
        }

        spellInventory = SpellInventoryManager.Instance;
        if (spellInventory == null)
        {
            return;
        }

        spellInventory.BookNotificationChanged += OnBookNotificationChanged;
        isBound = true;
    }

    private void UnbindManager()
    {
        if (!isBound || spellInventory == null)
        {
            return;
        }

        spellInventory.BookNotificationChanged -= OnBookNotificationChanged;
        isBound = false;
        spellInventory = null;
    }

    private void OnBookNotificationChanged(bool hasUnseenSpell)
    {
        ApplySprite(hasUnseenSpell);
    }

    private void RefreshVisual()
    {
        bool forceFirstGlow = glowOnFirstLaunch && !firstGlowConsumedThisSession;
        bool hasUnseen = forceFirstGlow || (spellInventory != null && spellInventory.HasUnseenUnlockedSpell);
        ApplySprite(hasUnseen);
    }

    private void ApplySprite(bool highlighted)
    {
        if (targetImage == null)
        {
            return;
        }

        Sprite nextSprite = highlighted ? highlightedSprite : normalSprite;
        if (nextSprite != null)
        {
            targetImage.sprite = nextSprite;
        }
    }
}
