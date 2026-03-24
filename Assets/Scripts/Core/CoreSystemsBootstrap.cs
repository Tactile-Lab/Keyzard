using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreSystemsBootstrap : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private GameManager gameManagerPrefab;
    [SerializeField] private SpellInventoryManager spellInventoryManagerPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;

    [Header("Gameplay Bootstrap")]
    [SerializeField] private GameObject typingManagerPrefab;
    [SerializeField] private GameObject glossaryPrefab;

    private bool sceneHookRegistered;

    private void Awake()
    {
        EnsureManagers();
        RegisterSceneHook();

        // Cas ou on demarre directement dans la scene de gameplay.
        EnsureGameplayTypingManager(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (sceneHookRegistered)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            sceneHookRegistered = false;
        }
    }

    private void EnsureManagers()
    {
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }

        if (SpellInventoryManager.Instance == null && spellInventoryManagerPrefab != null)
        {
            Instantiate(spellInventoryManagerPrefab);
        }

        if (AudioManager.Instance == null && audioManagerPrefab != null)
        {
            Instantiate(audioManagerPrefab);
        }
    }

    private void RegisterSceneHook()
    {
        if (sceneHookRegistered)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneHookRegistered = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureManagers();
        EnsureGameplayTypingManager(scene);
    }

    private void EnsureGameplayTypingManager(Scene scene)
    {
        if (typingManagerPrefab == null)
        {
            return;
        }

        // Spawn only in playable scenes that contain the player controller.
        if (FindFirstObjectByType<PlayerControler>() == null)
        {
            return;
        }

        if (FindFirstObjectByType<TypingSortManager>() != null)
        {
            return;
        }

        Instantiate(typingManagerPrefab);

        // Also spawn glossary if available and not already present.
        if (glossaryPrefab != null && FindFirstObjectByType<GlossaryToggleController>() == null)
        {
            Instantiate(glossaryPrefab);
        }
    }
}