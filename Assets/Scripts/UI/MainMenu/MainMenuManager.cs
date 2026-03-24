using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static readonly Color SelectedOutlineColor = new Color32(0xFF, 0xB4, 0x00, 0xFF);

    [Serializable]
    private class MenuOption
    {
        [Tooltip("Racine visuelle de l'option (image, texte, groupe).")]
        public RectTransform target;

        [Tooltip("Cree automatiquement un Outline UI sur cette option (si composant Graphic present).")]
        public bool autoOutline = true;

        [Tooltip("Couleur du contour quand l'option est selectionnee.")]
        public Color outlineColor = new Color32(0xFF, 0xB4, 0x00, 0xFF);

        [Tooltip("Epaisseur du contour UI.")]
        public Vector2 outlineDistance = new Vector2(6f, 6f);
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
    private Vector3[] baseScales = new Vector3[2];
    private Vector2[] baseAnchoredPositions = new Vector2[2];
    private Outline[] runtimeOutlines = new Outline[2];
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
                baseAnchoredPositions[i] = options[i].target.anchoredPosition;
                SetupOutlineForOption(i);
            }
            else
            {
                baseScales[i] = Vector3.one;
                baseAnchoredPositions[i] = Vector2.zero;
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
            ApplyScaleWithCenteredVisual(i, targetMultiplier);
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

            // Conserve le centre visuel, meme si le pivot du RectTransform est decale.
            float currentMultiplier = Mathf.Abs(baseScales[i].x) > 0.0001f
                ? options[i].target.localScale.x / baseScales[i].x
                : 1f;
            ApplyCenteredAnchoredPosition(i, currentMultiplier);
        }
    }

    private void RefreshOutlineState()
    {
        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = i == selectedIndex;

            if (runtimeOutlines[i] != null)
            {
                runtimeOutlines[i].effectColor = SelectedOutlineColor;
                runtimeOutlines[i].effectDistance = options[i].outlineDistance;
                runtimeOutlines[i].useGraphicAlpha = false;
                runtimeOutlines[i].enabled = isSelected;
            }
        }
    }

    private void SetupOutlineForOption(int index)
    {
        MenuOption option = options[index];
        if (option == null || option.target == null || !option.autoOutline)
        {
            return;
        }

        Graphic graphic = option.target.GetComponent<Graphic>();
        if (graphic == null)
        {
            Debug.LogWarning($"[MainMenuManager] Aucun Graphic sur '{option.target.name}', contour auto ignore.");
            return;
        }

        Outline outline = option.target.GetComponent<Outline>();
        if (outline == null)
        {
            outline = option.target.gameObject.AddComponent<Outline>();
        }

        // Force outline settings to ensure color renders correctly.
        outline.useGraphicAlpha = false;
        outline.effectColor = SelectedOutlineColor;
        outline.effectDistance = option.outlineDistance;
        outline.enabled = false;
        runtimeOutlines[index] = outline;
    }

    private void ApplyScaleWithCenteredVisual(int index, float scaleMultiplier)
    {
        if (options[index] == null || options[index].target == null)
        {
            return;
        }

        options[index].target.localScale = baseScales[index] * scaleMultiplier;
        ApplyCenteredAnchoredPosition(index, scaleMultiplier);
    }

    private void ApplyCenteredAnchoredPosition(int index, float scaleMultiplier)
    {
        RectTransform target = options[index].target;
        Vector2 pivot = target.pivot;
        Vector2 size = target.rect.size;

        // Compense le pivot pour garder le centre visuel fixe pendant le scale.
        Vector2 centerOffset = new Vector2((0.5f - pivot.x) * size.x, (0.5f - pivot.y) * size.y);
        Vector2 compensation = centerOffset * (scaleMultiplier - 1f);
        target.anchoredPosition = baseAnchoredPositions[index] - compensation;
    }
}