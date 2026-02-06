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
        // If an instance already exists and it's not this one, destroy it
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional: keep this object between scenes
        DontDestroyOnLoad(gameObject);
    }
void Start()
{
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

    list_enemies.Clear();

    HashSet<string> usedCodes = new HashSet<string>();

    foreach (GameObject enemy in enemies)
    {
        EnemyEntry entry = new EnemyEntry();
        entry.enemy = enemy;
        entry.code = GenerateRandomCode(usedCodes);

        list_enemies.Add(entry);
    }
}


    string GenerateRandomCode(HashSet<string> usedCodes)
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