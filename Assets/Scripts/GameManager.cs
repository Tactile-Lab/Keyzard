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

        // Initialisation des ennemis dès Awake
        InitEnemies();
    }

    private void InitEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        list_enemies.Clear();
        HashSet<string> usedCodes = new();

        foreach (GameObject enemy in enemies)
        {
            EnemyEntry entry = new()
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
