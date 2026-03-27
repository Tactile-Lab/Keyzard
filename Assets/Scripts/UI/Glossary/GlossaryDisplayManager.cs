using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GlossaryDisplayManager : MonoBehaviour
{
    [Header("Slots Source")]
    [SerializeField] private Transform slotsContainer;

    [Header("Display Elements")]
    [SerializeField] private TextMeshProUGUI sortNameText;
    [SerializeField] private TextMeshProUGUI sortDescriptionText;
    [SerializeField] private Image sortIconImage;
    [SerializeField] private Image sortDemonstrationImage;

    [Header("Navigation")]
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private int columnCount = 4;
    [SerializeField] private bool wrapHorizontal = true;
    [SerializeField] private SFXEventKey glossaryNavigationSfx = SFXEventKey.UIMenuMove;

    [Header("Slot Binding")]
    [SerializeField] private string slotNamePrefix = "Slot_";
    [SerializeField] private string[] selectionObjectNames = { "Selection", "selection", "Object Selection", "object selection" };
    [SerializeField] private string[] iconObjectNames = { "icon sort", "Icon Sort", "icon_sort" };

    [Header("Locked Display")]
    [SerializeField] private string lockedNameLabel = "???";
     private string lockedDescriptionLabel = "Ce sort n'est pas encore débloqué.";
    [SerializeField] private float lockedIconAlpha = 0.25f;

    private readonly List<GameObject> slots = new List<GameObject>();
    private int currentSelectedIndex = 0;
    private IReadOnlyList<Sort> spellCatalog;
    private int lastKnownChildCount = -1;
    private SpellInventoryManager boundInventory;

    private void OnEnable()
    {
        if (slotsContainer == null)
        {
            slotsContainer = transform;
        }

        BindInventory();
        BuildFromInventory();

        if (spellCatalog != null && spellCatalog.Count > 0)
        {
            SelectSlot(currentSelectedIndex, true);
        }
        else
        {
            ClearDisplay();
        }
    }

    private void OnDisable()
    {
        if (boundInventory != null)
        {
            boundInventory.InventoryChanged -= OnInventoryChanged;
            boundInventory = null;
        }
    }

    private void Start()
    {
        BuildFromInventory();
        SelectSlot(0, true);
    }

    private void BindInventory()
    {
        SpellInventoryManager current = SpellInventoryManager.Instance;
        if (ReferenceEquals(boundInventory, current))
        {
            return;
        }

        if (boundInventory != null)
        {
            boundInventory.InventoryChanged -= OnInventoryChanged;
        }

        boundInventory = current;

        if (boundInventory != null)
        {
            boundInventory.InventoryChanged -= OnInventoryChanged;
            boundInventory.InventoryChanged += OnInventoryChanged;
        }
    }

    private void OnInventoryChanged()
    {
        BuildFromInventory();
        SelectSlot(currentSelectedIndex, true);
    }

    private void CacheSlots()
    {
        slots.Clear();

        Transform source = slotsContainer != null ? slotsContainer : transform;

        foreach (Transform child in source)
        {
            if (!child.name.StartsWith(slotNamePrefix, StringComparison.OrdinalIgnoreCase)) continue;
            slots.Add(child.gameObject);
        }

        lastKnownChildCount = source.childCount;

        slots.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        if (gridLayout == null)
        {
            gridLayout = GetComponent<GridLayoutGroup>();
        }

        if (gridLayout != null)
        {
            columnCount = gridLayout.constraintCount;
        }
    }

    private void BuildFromInventory()
    {
        CacheSlots();

        SpellInventoryManager inventory = boundInventory != null ? boundInventory : SpellInventoryManager.Instance;
        spellCatalog = inventory != null ? inventory.GetSpellCatalog() : null;

        for (int i = 0; i < slots.Count; i++)
        {
            Sort sort = (spellCatalog != null && i < spellCatalog.Count) ? spellCatalog[i] : null;
            SetSlotVisual(slots[i], sort);
            SetSlotSelection(slots[i], false);
        }

        int max = Mathf.Max(0, slots.Count - 1);
        currentSelectedIndex = Mathf.Clamp(currentSelectedIndex, 0, max);
    }

    private void Update()
    {
        SpellInventoryManager previousInventory = boundInventory;
        BindInventory();
        if (!ReferenceEquals(previousInventory, boundInventory))
        {
            BuildFromInventory();
            SelectSlot(currentSelectedIndex, true);
        }

        Transform source = slotsContainer != null ? slotsContainer : transform;
        if (source.childCount != lastKnownChildCount)
        {
            BuildFromInventory();
            SelectSlot(currentSelectedIndex, true);
        }

        HandleArrowNavigation();
    }

    private void HandleArrowNavigation()
    {
        if (!GlossaryToggleController.IsGlossaryOpen)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Flèche droite
        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            int next = currentSelectedIndex + 1;
            if (wrapHorizontal && slots.Count > 0)
            {
                next = (next + slots.Count) % slots.Count;
            }
            SelectSlot(next);
        }
        // Flèche gauche
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            int next = currentSelectedIndex - 1;
            if (wrapHorizontal && slots.Count > 0)
            {
                next = (next + slots.Count) % slots.Count;
            }
            SelectSlot(next);
        }
        // Flèche bas
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            SelectSlot(currentSelectedIndex + columnCount);
        }
        // Flèche haut
        else if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            SelectSlot(currentSelectedIndex - columnCount);
        }
    }

    private void SelectSlot(int index, bool force = false)
    {
        if (slots.Count == 0)
        {
            ClearDisplay();
            return;
        }

        // Clamper l'index entre 0 et le nombre total de slots
        int maxIndex = Mathf.Max(0, slots.Count - 1);
        index = Mathf.Clamp(index, 0, maxIndex);

        if (!force && index == currentSelectedIndex)
            return;

        bool shouldPlayNavigationSfx = !force && index != currentSelectedIndex;

        // Désactiver la selection du slot précédent
        if (currentSelectedIndex >= 0 && currentSelectedIndex < slots.Count)
        {
            SetSlotSelection(slots[currentSelectedIndex], false);
        }

        // Activer la selection du nouveau slot
        currentSelectedIndex = index;
        SetSlotSelection(slots[currentSelectedIndex], true);

        if (shouldPlayNavigationSfx)
        {
            AudioManager.Instance?.PlaySFXEvent(glossaryNavigationSfx);
        }

        // Mettre à jour l'affichage
        UpdateDisplay();
    }

    private void SetSlotSelection(GameObject slot, bool isSelected)
    {
        if (slot == null)
            return;

        GlossarySlotRefs refs = slot.GetComponent<GlossarySlotRefs>();
        if (refs != null && refs.SelectionObject != null)
        {
            refs.SelectionObject.SetActive(isSelected);
            return;
        }

        for (int i = 0; i < selectionObjectNames.Length; i++)
        {
            Transform selectionObj = FindChildRecursiveByName(slot.transform, selectionObjectNames[i]);
            if (selectionObj != null)
            {
                selectionObj.gameObject.SetActive(isSelected);
                return;
            }
        }
    }

    private void SetSlotVisual(GameObject slot, Sort sort)
    {
        if (slot == null) return;

        Image iconTarget = FindIconImage(slot);
        if (iconTarget == null)
        {
            return;
        }

        bool hasContent = sort != null && sort.icon != null;
        bool isUnlocked = sort != null && IsSortUnlocked(sort);

        if (hasContent && isUnlocked)
        {
            iconTarget.enabled = true;
            iconTarget.sprite = sort.icon;
            SetImageAlpha(iconTarget, 1f);
        }
        else
        {
            iconTarget.enabled = false;
            iconTarget.sprite = null;
            SetImageAlpha(iconTarget, lockedIconAlpha);
        }
    }

    private Image FindIconImage(GameObject slot)
    {
        GlossarySlotRefs refs = slot.GetComponent<GlossarySlotRefs>();
        if (refs != null && refs.IconSortImage != null)
        {
            return refs.IconSortImage;
        }

        for (int i = 0; i < iconObjectNames.Length; i++)
        {
            Transform iconObject = FindChildRecursiveByName(slot.transform, iconObjectNames[i]);
            if (iconObject != null)
            {
                Image image = iconObject.GetComponent<Image>();
                if (image != null) return image;
            }
        }

        return null;
    }

    private static Transform FindChildRecursiveByName(Transform root, string targetName)
    {
        if (root == null || string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (string.Equals(child.name, targetName, StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }

            Transform nested = FindChildRecursiveByName(child, targetName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null) return;
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void UpdateDisplay()
    {
        if (currentSelectedIndex < 0 || currentSelectedIndex >= slots.Count)
            return;

        Sort sort = (spellCatalog != null && currentSelectedIndex < spellCatalog.Count)
            ? spellCatalog[currentSelectedIndex]
            : null;

        if (sort == null)
        {
            ClearDisplay();
            return;
        }

        // Vérifier que le sort est déverrouillé
        if (!IsSortUnlocked(sort))
        {
            DisplayLockedSpell();
            return;
        }

        // Afficher les infos du sort
        if (sortNameText != null)
            sortNameText.text = GetDisplaySpellName(sort);

        if (sortDescriptionText != null)
            sortDescriptionText.text = sort.description;

        if (sortIconImage != null)
        {
            sortIconImage.enabled = sort.icon != null;
            sortIconImage.sprite = sort.icon;
        }

        if (sortDemonstrationImage != null)
        {
            sortDemonstrationImage.enabled = sort.demonstrationIllustration != null;
            sortDemonstrationImage.sprite = sort.demonstrationIllustration;
        }
    }

    private bool IsSortUnlocked(Sort sort)
    {
        SpellInventoryManager inventory = boundInventory != null ? boundInventory : SpellInventoryManager.Instance;
        if (inventory == null || sort == null)
        {
            return false;
        }

        string key = !string.IsNullOrWhiteSpace(sort.nomSort) ? sort.nomSort : sort.name;
        bool unlockedByKey = !string.IsNullOrWhiteSpace(key) && inventory.IsUnlocked(key);
        if (unlockedByKey)
        {
            return true;
        }

        // Fallback: certains prefabs peuvent avoir un nom cle differente mais etre bien presents en unlocked list.
        IReadOnlyList<Sort> unlocked = inventory.GetUnlockedSorts();
        for (int i = 0; i < unlocked.Count; i++)
        {
            if (ReferenceEquals(unlocked[i], sort))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetDisplaySpellName(Sort sort)
    {
        if (sort == null)
        {
            return string.Empty;
        }

        string raw = !string.IsNullOrWhiteSpace(sort.nomSort) ? sort.nomSort : sort.name;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        string normalized = raw.Replace('_', ' ').Trim();
        return normalized.ToUpperInvariant();
    }

    private void DisplayLockedSpell()
    {
        if (sortNameText != null)
            sortNameText.text = lockedNameLabel;

        if (sortDescriptionText != null)
            sortDescriptionText.text = lockedDescriptionLabel;

        if (sortIconImage != null)
        {
            sortIconImage.enabled = false;
            sortIconImage.sprite = null;
        }

        if (sortDemonstrationImage != null)
        {
            sortDemonstrationImage.enabled = false;
            sortDemonstrationImage.sprite = null;
        }
    }

    private void ClearDisplay()
    {
        if (sortNameText != null)
            sortNameText.text = "";

        if (sortDescriptionText != null)
            sortDescriptionText.text = "";

        if (sortIconImage != null)
        {
            sortIconImage.enabled = false;
            sortIconImage.sprite = null;
        }

        if (sortDemonstrationImage != null)
        {
            sortDemonstrationImage.enabled = false;
            sortDemonstrationImage.sprite = null;
        }
    }

}

