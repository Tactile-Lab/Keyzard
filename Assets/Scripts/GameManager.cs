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
        HashSet<string> usedCodes = new();

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            EnemyEntry entry = new EnemyEntry
            {
                enemy = enemy,
                code = GenerateRandomCode(usedCodes)
            };

            list_enemies.Add(entry);
        }
    }

    private string GenerateRandomCode(HashSet<string> usedCodes)
    {
        string code;

        do
        {
            char first = (char)Random.Range('A', 'Z' + 1);
            char second = (char)Random.Range('A', 'Z' + 1);

            code = first.ToString() + second.ToString();

        } while (usedCodes.Contains(code));

        usedCodes.Add(code);

        return code;
    }
}