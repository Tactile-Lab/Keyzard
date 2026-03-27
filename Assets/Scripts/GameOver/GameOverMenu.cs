using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverMenuController : MonoBehaviour
{
    public static bool IsGameOverMenuOpen { get; private set; }

    [Header("Menu UI")]
    public GameObject panelRoot; // Panel principal du menu
    public RectTransform imageRejouer;
    public RectTransform imageMenu;

    [Header("Input / Animation")] 
    [SerializeField] private float inputCooldown = 0.1f;
    [SerializeField] private float buttonScalePop = 1.15f;
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float punchUpDistance = 20f;
    [SerializeField] private float punchDownDistance = -20f;
    [SerializeField] private float punchDuration = 0.15f;
    [SerializeField] private float attenteBouton = 1.5f;
    [SerializeField] private float playerDeathFadeOutDuration = 0.2f;
    private RectTransform[] optionTargets;
    private UIButtonSpriteSwap[] optionSpriteSwaps;
    private Tween[] buttonScaleTweens;
    private Tween[] buttonPunchTweens;
    private int selectionIndex;
    private float nextInputTime;

    private void Awake()
    {
        optionTargets = new RectTransform[] { imageRejouer, imageMenu };
        optionSpriteSwaps = new UIButtonSpriteSwap[optionTargets.Length];
        buttonScaleTweens = new Tween[optionTargets.Length];
        buttonPunchTweens = new Tween[optionTargets.Length];

        for (int i = 0; i < optionTargets.Length; i++)
        {
            if (optionTargets[i] != null)
                optionSpriteSwaps[i] = optionTargets[i].GetComponent<UIButtonSpriteSwap>();
        }

        if (panelRoot != null)
            panelRoot.SetActive(false);

        IsGameOverMenuOpen = false;
    }

    private void Update()
    {
        if (!IsGameOverMenuOpen || Time.unscaledTime < nextInputTime)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.leftArrowKey.wasPressedThisFrame && selectionIndex == 1)
            ChangeSelection(-1);
        else if (keyboard.rightArrowKey.wasPressedThisFrame && selectionIndex == 0)
            ChangeSelection(1);
        else if (keyboard.spaceKey.wasPressedThisFrame)
            ConfirmSelection();
    }

    public void ShowGameOverMenu()
    {
        AudioManager.Instance?.PlayMusic(GameMusicState.GameOver);

        if (panelRoot != null)
            panelRoot.SetActive(true);

        selectionIndex = 0;
        ResetAllButtons();
        RefreshOptionVisualState();

        SetButtonsVisible(false);

        nextInputTime = Time.unscaledTime + inputCooldown;
        StartCoroutine(ShowButtonsDelayed());
    }

    public void HideGameOverMenu()
    {
        IsGameOverMenuOpen = false;

        for (int i = 0; i < buttonScaleTweens.Length; i++)
        {
            buttonScaleTweens[i]?.Kill();
            buttonPunchTweens[i]?.Kill();
        }

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private IEnumerator ShowButtonsDelayed()
    {
        yield return new WaitForSecondsRealtime(attenteBouton); // ajuste selon ton fade

        SetButtonsVisible(true);
        IsGameOverMenuOpen = true;
    }

    private void SetButtonsVisible(bool visible)
    {
        for (int i = 0; i < optionTargets.Length; i++)
        {
            RectTransform target = optionTargets[i];
            if (target == null) continue;

            CanvasGroup cg = target.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = target.gameObject.AddComponent<CanvasGroup>();
                cg.interactable = false; // on se fout, tu gères au clavier
                cg.blocksRaycasts = false;
            }

            if (visible)
            {
                cg.alpha = 0f; // start invisible
                DOTween.To(
                    () => cg.alpha,
                    x => cg.alpha = x,
                    1f,
                    1f // durée du fade
                ).SetUpdate(true);
            }
            else
            {
                cg.alpha = 0f; // completely invisible
            }
        }
    }

    private void ResetAllButtons()
    {
        for (int i = 0; i < optionSpriteSwaps.Length; i++)
        {
            if (optionSpriteSwaps[i] != null)
            {
                optionSpriteSwaps[i].SetPressed(false);
                optionSpriteSwaps[i].SetSelected(false);
            }
        }
    }

    private void ChangeSelection(int delta)
    {
        selectionIndex = (selectionIndex + delta + optionTargets.Length) % optionTargets.Length;
        nextInputTime = Time.unscaledTime + inputCooldown;
        RefreshOptionVisualState();
        PlayPunchEffect(delta);
    }

    private void PlayPunchEffect(int direction)
    {
        RectTransform target = optionTargets[selectionIndex];
        if (target == null) return;

        buttonPunchTweens[selectionIndex]?.Kill();

        Vector2 originalPos = target.anchoredPosition;
        float punchDistance = direction > 0 ? punchDownDistance : punchUpDistance;

        buttonPunchTweens[selectionIndex] = target
            .DOAnchorPosY(originalPos.y + punchDistance, punchDuration * 0.5f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                buttonPunchTweens[selectionIndex] = target
                    .DOAnchorPosY(originalPos.y, punchDuration * 0.5f)
                    .SetEase(Ease.InCubic)
                    .SetUpdate(true);
            });
    }

    private void ConfirmSelection()
    {
        nextInputTime = Time.unscaledTime + inputCooldown;

        AudioManager.Instance?.FadeOutSFXEvent(SFXEventKey.PlayerDeath, playerDeathFadeOutDuration);

        UIButtonSpriteSwap swap = optionSpriteSwaps[selectionIndex];
        if (swap != null) swap.SetPressed(true);

        switch (selectionIndex)
        {
            case 0: Rejouer(); break;
            case 1: RetourMenu(); break;
        }
    }

    private void RefreshOptionVisualState()
    {
        for (int i = 0; i < optionTargets.Length; i++)
        {
            bool isSelected = (i == selectionIndex);

            UIButtonSpriteSwap swap = optionSpriteSwaps[i];
            if (swap != null) swap.SetSelected(isSelected);

            RectTransform target = optionTargets[i];
            if (target == null) continue;

            buttonScaleTweens[i]?.Kill();
            float targetScale = isSelected ? buttonScalePop : 1f;
            buttonScaleTweens[i] = target
                .DOScale(targetScale, popDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
    }

    private void Rejouer()
    {
        IsGameOverMenuOpen = false;
        SpellInventoryManager.Instance.ResetInventory();
        AudioManager.Instance?.ResetMusicRuntime();
        AudioManager.Instance?.PlaySFXEvent(SFXEventKey.EboulementLaunch);
        TransitionManager.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RetourMenu()
    {
        IsGameOverMenuOpen = false;
        SpellInventoryManager.Instance.ResetInventory();
        AudioManager.Instance?.ResetMusicRuntime();
        TransitionManager.Instance.LoadScene(0);
    }
}