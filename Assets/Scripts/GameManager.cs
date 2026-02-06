using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<Enemy> ennemies;

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

}
