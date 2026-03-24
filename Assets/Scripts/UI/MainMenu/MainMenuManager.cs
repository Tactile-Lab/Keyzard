using System;
using System.Collections.Generic;
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

        [Tooltip("Composant visuel optionnel a activer pour le contour (Outline UI, script custom, etc.).")]
        public Behaviour outlineBehaviour;

        [Tooltip("Objet optionnel a activer pour afficher un contour sprite/UI.")]
        public GameObject outlineObject;
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
    [SerializeField] private bool logSelectionWarnings = true;

    private readonly MenuOption[] options = new MenuOption[2];
    private Vector3[] baseScales = new Vector3[2];
    private readonly HashSet<int> warnedOptionIndices = new HashSet<int>();
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
        RefreshOutlineState();
    }

    private void ConfirmSelection()
    {
        nextInputTime = Time.unscaledTime + Mathf.Max(0f, inputCooldown);

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

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RefreshSelectionImmediate()
    {
        RefreshOutlineState();

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

    private void RefreshOutlineState()
    {
        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = i == selectedIndex;
            MenuOption option = options[i];

            if (option != null && option.outlineBehaviour != null)
            {
                if (CanToggleOutlineBehaviour(option.outlineBehaviour))
                {
                    option.outlineBehaviour.enabled = isSelected;
                }
                else if (logSelectionWarnings && warnedOptionIndices.Add(i))
                {
                    Debug.LogWarning(
                        $"[MainMenuManager] outlineBehaviour sur l'option {i} n'est pas un composant d'outline/shadow. " +
                        "Assigne un component Unity UI Outline (ou utilise outlineObject)."
                    );
                }
            }

            if (option != null && option.outlineObject != null)
            {
                option.outlineObject.SetActive(isSelected);
            }
        }
    }

    private static bool CanToggleOutlineBehaviour(Behaviour behaviour)
    {
        string typeName = behaviour.GetType().Name;
        return typeName.Contains("Outline") || typeName.Contains("Shadow");
    }
}