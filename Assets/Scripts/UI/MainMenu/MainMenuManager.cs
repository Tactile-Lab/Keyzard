using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Serializable]
    private class MenuOption
    {
        [Tooltip("Racine visuelle de l'option (image, texte, groupe).")]
        public RectTransform target;
    }

    [Header("Options")]
    [SerializeField] private MenuOption nouvellePartie;
    [SerializeField] private MenuOption quitter;

    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "RoomCrawling New";

    [Header("Selection Visual")]
    [SerializeField] private float selectedScale = 1.08f;
    [SerializeField] private float unselectedScale = 1f;
    [SerializeField] private float scaleLerpSpeed = 14f;

    [Header("Input")]
    [SerializeField] private float inputCooldown = 0.12f;

    private readonly MenuOption[] options = new MenuOption[2];
    private readonly UIButtonSpriteSwap[] optionSpriteSwaps = new UIButtonSpriteSwap[2];
    private Vector3[] baseScales = new Vector3[2];
    private int selectedIndex;
    private float nextInputTime;
    private bool isLoading;

    private void Awake()
    {
        options[0] = nouvellePartie;
        options[1] = quitter;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null && options[i].target != null)
            {
                baseScales[i] = options[i].target.localScale;
                optionSpriteSwaps[i] = options[i].target.GetComponent<UIButtonSpriteSwap>();

                if (optionSpriteSwaps[i] == null)
                {
                    Debug.LogWarning($"[MainMenuManager] UIButtonSpriteSwap manquant sur l'option {i} ({options[i].target.name}).");
                }
            }
            else
            {
                baseScales[i] = Vector3.one;
            }
        }

        selectedIndex = 0;
        Time.timeScale = 1f;
        RefreshSelectionImmediate();
    }

    private void Start()
    {
        AudioManager.Instance?.ResetMusicRuntime();
        AudioManager.Instance?.PlayMusic(GameMusicState.MainMenu);
    }

    private void Update()
    {
        UpdateSelectionScale();

        if (isLoading)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || Time.unscaledTime < nextInputTime)
        {
            return;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            ChangeSelection(-1);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            ChangeSelection(1);
        }
        else if (keyboard.spaceKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    private void ChangeSelection(int delta)
    {
        int optionCount = options.Length;
        selectedIndex = (selectedIndex + delta + optionCount) % optionCount;
        nextInputTime = Time.unscaledTime + Mathf.Max(0f, inputCooldown);
        RefreshVisualState();
    }

    private void ConfirmSelection()
    {
        nextInputTime = Time.unscaledTime + Mathf.Max(0f, inputCooldown);

        if (selectedIndex >= 0 && selectedIndex < options.Length)
        {
            UIButtonSpriteSwap selectedSpriteSwap = optionSpriteSwaps[selectedIndex];
            if (selectedSpriteSwap != null)
            {
                selectedSpriteSwap.SetPressed(true);
            }
        }

        if (selectedIndex == 0)
        {
            StartNewGame();
            return;
        }

        QuitGame();
    }

    private void StartNewGame()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.list_enemies.Clear();
        }

        if (SpellInventoryManager.Instance != null)
        {
            SpellInventoryManager.Instance.InitializeStartingInventory();
        }

        AudioManager.Instance?.ResetMusicRuntime();
        AudioManager.Instance?.PlaySFXEvent(SFXEventKey.EboulementLaunch);

        Time.timeScale = 1f;
        //SceneManager.LoadScene(gameplaySceneName);
        TransitionManager.Instance.LoadScene(gameplaySceneName);
    }

    private void QuitGame()
    {
        if (BlackFadeEffect.Instance != null)
        {
            // Bloque tout pendant le fade
            Time.timeScale = 0f;

            BlackFadeEffect.Instance.PlayFadeOut(() =>
            {
                // Une fois le fade terminé, on quitte le jeu
    #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
    #else
                Application.Quit();
    #endif
            });
        }
        else
        {
            // Si pas de BlackFadeEffect, quitte directement
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
    }

    private void RefreshSelectionImmediate()
    {
        RefreshVisualState();

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null || options[i].target == null)
            {
                continue;
            }

            float targetMultiplier = (i == selectedIndex) ? selectedScale : unselectedScale;
            options[i].target.localScale = baseScales[i] * targetMultiplier;
        }
    }

    private void UpdateSelectionScale()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == null || options[i].target == null)
            {
                continue;
            }

            float targetMultiplier = (i == selectedIndex) ? selectedScale : unselectedScale;
            Vector3 targetScale = baseScales[i] * targetMultiplier;
            options[i].target.localScale = Vector3.Lerp(
                options[i].target.localScale,
                targetScale,
                1f - Mathf.Exp(-scaleLerpSpeed * Time.unscaledDeltaTime)
            );
        }
    }

    private void RefreshVisualState()
    {
        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = i == selectedIndex;
            UIButtonSpriteSwap spriteSwap = optionSpriteSwaps[i];

            if (spriteSwap != null)
            {
                spriteSwap.SetSelected(isSelected);
            }
        }
    }
}