using System;
using System.Collections.Generic;
using UnityEngine;


public class SpellInventoryManager : MonoBehaviour
{
    public static SpellInventoryManager Instance { get; private set; }

    [Header("Catalog")]
    [SerializeField] private List<Sort> allSpells = new List<Sort>();

    [Header("Starting Inventory")]
    [SerializeField] private List<Sort> startingSpells = new List<Sort>();
    [SerializeField] private bool initializeOnAwake = true;

    [Header("Debug")]
    [SerializeField] private bool debugUnlockAllSpellsOnStart = false;

    private readonly List<Sort> unlockedSpells = new List<Sort>();
    private readonly HashSet<string> unlockedIds = new HashSet<string>();
    private bool initialized;
    private bool lastDebugUnlockAllApplied = false;

    public event Action InventoryChanged;

    public IReadOnlyList<Sort> UnlockedSpells => unlockedSpells;

    public IReadOnlyList<Sort> GetSpellCatalog()
    {
        if (allSpells != null && allSpells.Count > 0)
        {
            return allSpells;
        }

        return startingSpells;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        lastDebugUnlockAllApplied = debugUnlockAllSpellsOnStart;

        if (initializeOnAwake)
        {
            InitializeStartingInventory();
        }
    }

    public void InitializeStartingInventory()
    {
        unlockedSpells.Clear();
        unlockedIds.Clear();

        List<Sort> source = debugUnlockAllSpellsOnStart ? allSpells : startingSpells;
        for (int i = 0; i < source.Count; i++)
        {
            UnlockSpell(source[i], false);
        }

        initialized = true;
        InventoryChanged?.Invoke();
    }

    private void UnlockAllSpells()
    {
        if (allSpells == null || allSpells.Count == 0)
        {
            InitializeStartingInventory();
            return;
        }

        unlockedSpells.Clear();
        unlockedIds.Clear();

        for (int i = 0; i < allSpells.Count; i++)
        {
            UnlockSpell(allSpells[i], false);
        }

        initialized = true;
        InventoryChanged?.Invoke();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (!initialized) return;

        // Détecte le changement de debugUnlockAllSpellsOnStart
        if (debugUnlockAllSpellsOnStart != lastDebugUnlockAllApplied)
        {
            lastDebugUnlockAllApplied = debugUnlockAllSpellsOnStart;

            if (debugUnlockAllSpellsOnStart)
            {
                // Passer de false à true : déverrouiller TOUS les spells
                UnlockAllSpells();
            }
            else
            {
                // Passer de true à false : réinitialiser avec l'inventaire normal
                InitializeStartingInventory();
            }
        }
    }

    public IReadOnlyList<Sort> GetUnlockedSorts()
    {
        if (!initialized && initializeOnAwake)
        {
            InitializeStartingInventory();
        }

        return unlockedSpells;
    }

    public bool UnlockSpell(Sort spellPrefab)
    {
        return UnlockSpell(spellPrefab, true);
    }

    public bool UnlockSpellByName(string sortName)
    {
        if (string.IsNullOrWhiteSpace(sortName))
        {
            return false;
        }

        Sort spell = FindSpellByName(sortName);
        if (spell == null)
        {
            return false;
        }

        return UnlockSpell(spell, true);
    }

    public bool IsUnlocked(string sortName)
    {
        if (string.IsNullOrWhiteSpace(sortName))
        {
            return false;
        }

        return unlockedIds.Contains(NormalizeKey(sortName));
    }

    private bool UnlockSpell(Sort spellPrefab, bool notify)
    {
        if (spellPrefab == null)
        {
            return false;
        }

        string key = GetSortKey(spellPrefab);
        if (!unlockedIds.Add(key))
        {
            return false;
        }

        unlockedSpells.Add(spellPrefab);

        if (notify)
        {
            InventoryChanged?.Invoke();
        }

        return true;
    }

    private Sort FindSpellByName(string sortName)
    {
        string key = NormalizeKey(sortName);

        for (int i = 0; i < allSpells.Count; i++)
        {
            Sort spell = allSpells[i];
            if (spell == null) continue;

            if (GetSortKey(spell) == key)
            {
                return spell;
            }
        }

        for (int i = 0; i < startingSpells.Count; i++)
        {
            Sort spell = startingSpells[i];
            if (spell == null) continue;

            if (GetSortKey(spell) == key)
            {
                return spell;
            }
        }

        return null;
    }

    private static string GetSortKey(Sort spell)
    {
        if (!string.IsNullOrWhiteSpace(spell.nomSort))
        {
            return NormalizeKey(spell.nomSort);
        }

        return NormalizeKey(spell.name);
    }

    private static string NormalizeKey(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

   public Sort GetRandomLockedSpell()
{
    IReadOnlyList<Sort> catalog = GetSpellCatalog();

    List<Sort> lockedSpells = new List<Sort>();

    for (int i = 0; i < catalog.Count; i++)
    {
        Sort spell = catalog[i];
        if (spell == null) continue;

        string key = GetSortKey(spell);

        if (!unlockedIds.Contains(key))
        {
            lockedSpells.Add(spell);
        }
    }

    if (lockedSpells.Count == 0)
    {
        return null;
    }

    int randomIndex = UnityEngine.Random.Range(0, lockedSpells.Count);
    return lockedSpells[randomIndex];
}
}