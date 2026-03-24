using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject enemy;
        public string code;
    }

    public static GameManager Instance;

    public List<EnemyEntry> list_enemies = new List<EnemyEntry>();

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // plus d'initialisation automatique
    }

    /// <summary>
    /// Enregistre une liste d'ennemis dans le GameManager quand la room devient active
    /// </summary>
    public void RegisterEnemies(List<GameObject> enemies)
    {
        // Crée un HashSet avec les 2 premières lettres de tous les sorts
        HashSet<string> spellPrefixes = new HashSet<string>();
        SpellInventoryManager inventory = SpellInventoryManager.Instance;
        if (inventory != null)
        {
            IReadOnlyList<Sort> catalog = inventory.GetSpellCatalog();
            for (int i = 0; i < catalog.Count; i++)
            {
                Sort sort = catalog[i];
                if (sort == null) continue;

                if (!string.IsNullOrEmpty(sort.nomSort) && sort.nomSort.Length >= 2)
                {
                    spellPrefixes.Add(sort.nomSort.Substring(0, 2).ToUpper());
                }
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] SpellInventoryManager introuvable, aucun prefixe de sort reserve.");
        }

        HashSet<string> usedCodes = new HashSet<string>();

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            EnemyEntry entry = new EnemyEntry
            {
                enemy = enemy,
                code = GenerateRandomCode(usedCodes, spellPrefixes)
            };

            list_enemies.Add(entry);
        }
    }

    private string GenerateRandomCode(HashSet<string> usedCodes, HashSet<string> forbiddenPrefixes)
    {
        string code;
        do
        {
            char first = (char)Random.Range('A', 'Z' + 1);
            char second = (char)Random.Range('A', 'Z' + 1);

            code = (first.ToString() + second.ToString()).ToUpper();

            // Vérifie que le code n'existe pas déjà et n'est pas un préfixe de sort
        } while (usedCodes.Contains(code) || forbiddenPrefixes.Contains(code));

        usedCodes.Add(code);
        return code;
    }
}