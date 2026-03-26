using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public static bool IsPauseMenuOpen { get; private set; }

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private RectTransform[] optionTargets = new RectTransform[3];
    [SerializeField] private RectTransform visualIndicatorObject;

    [Header("Input")]
    [SerializeField] private float inputCooldown = 0.02f;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.32f;
    [SerializeField] private float optionStagger = 0.06f;
    [SerializeField] private float closedLeftOffsetX = -1600f;
    [SerializeField] private float closedRightOffsetX = 1600f;
    [SerializeField] private Ease openEase = Ease.OutCubic;
    [SerializeField] private Ease closeEase = Ease.InCubic;

    [Header("Button Effects")]
    [SerializeField] private float buttonScalePop = 1.15f;
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float punchUpDistance = 30f;
    [SerializeField] private float punchDownDistance = -30f;
    [SerializeField] private float punchDuration = 0.15f;

    [Header("Backdrop")]
    [SerializeField] private GameObject backdropTarget;
    [SerializeField] private float backdropTargetAlpha = 0.45f;
    [SerializeField] private float backdropFadeDuration = 0.2f;
    [SerializeField] private bool manageBackdropActiveState = false;

    [Header("Audio")]
    [SerializeField] private bool muffleMusicWhenOpen = true;
    [SerializeField] private float musicMuffleTransition = 0.25f;

    private readonly UIButtonSpriteSwap[] optionSpriteSwaps = new UIButtonSpriteSwap[3];
    private readonly Vector2[] openAnchoredPositions = new Vector2[3];
    private readonly Tween[] optionTweens = new Tween[3];
    private readonly Tween[] buttonScaleTweens = new Tween[3];
    private readonly Tween[] buttonPunchTweens = new Tween[3];

    private CanvasGroup backdropGroup;
    private Tween backdropTween;
    private Tween visualIndicatorTween;

    private Vector2 visualIndicatorOpenPos;

    private bool isOpen;
    private bool isTransitioning;
    private bool isConfigured;
    private int selectedIndex;
    private float nextInputTime;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (pauseRoot == null && optionTargets != null && optionTargets.Length > 0 && optionTargets[0] != null)
        {
            pauseRoot = optionTargets[0].gameObject;
        }

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth introuvable dans la scène !");
        }

        CacheBackdropGroup();
        CacheOptions();
        CacheVisualIndicator();

        if (pauseRoot != null)
        {
            pauseRoot.SetActive(false);
        }

        SetBackdropState(false, false);
        IsPauseMenuOpen = false;
        isOpen = false;
        isTransitioning = false;
        isConfigured = optionTargets != null && optionTargets.Length == 3;
    }

    private void Update()
    {
        if (!isConfigured)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            TryTogglePause();
            return;
        }

        if (!isOpen || isTransitioning || Time.unscaledTime < nextInputTime)
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

    public void TryTogglePause()
    {
        if (isTransitioning || Time.unscaledTime < nextInputTime)
        {
            return;
        }

        // Bloquer le menu si une transition est en cours
        if (TransitionManager.IsTransitioning)
        {
            return;
        }

        if (GameOverMenuController.IsGameOverMenuOpen)
        {
            return;
        }

        if(EndController.IsEndMenuOpen)
        {
            return;
        }

        if (!isOpen && GlossaryToggleController.IsGlossaryOpen)
        {
            return;
        }


        nextInputTime = Time.unscaledTime + Mathf.Max(0f, inputCooldown);

        if (isOpen)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        if (isOpen || isTransitioning)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        isOpen = true;
        IsPauseMenuOpen = true;
        isTransitioning = true;
        selectedIndex = 0;

        if (pauseRoot != null)
        {
            pauseRoot.SetActive(true);
        }

        ApplyPauseState(true);
        SetBackdropState(true, true);
        PlaceOptionsOnLeft();
        RefreshOptionVisualState();
        PlayOptionsOpenAnimation();
    }

    public void ClosePauseMenu()
    {
        if (!isOpen || isTransitioning)
        {
            return;
        }

        isOpen = false;
        IsPauseMenuOpen = false;
        isTransitioning = true;

        ApplyPauseState(false);
        SetBackdropState(false, true);
        PlayOptionsCloseAnimation();
    }

    private void ChangeSelection(int delta)
    {
        int optionCount = optionTargets.Length;
        selectedIndex = (selectedIndex + delta + optionCount) % optionCount;
        nextInputTime = Time.unscaledTime + Mathf.Max(0f, inputCooldown);
        RefreshOptionVisualState();
        PlayPunchEffect(delta);
    }

    private void PlayPunchEffect(int direction)
    {
        RectTransform target = optionTargets[selectedIndex];
        if (target == null)
        {
            return;
        }

        buttonPunchTweens[selectedIndex]?.Kill();

        Vector2 centerPos = openAnchoredPositions[selectedIndex];
        float punchDistance = direction > 0 ? punchDownDistance : punchUpDistance;

        buttonPunchTweens[selectedIndex] = target
            .DOAnchorPosY(centerPos.y + punchDistance, punchDuration * 0.5f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                buttonPunchTweens[selectedIndex] = target
                    .DOAnchorPosY(centerPos.y, punchDuration * 0.5f)
                    .SetEase(Ease.InCubic)
                    .SetUpdate(true);
            });
    }

    private void ConfirmSelection()
    {
        nextInputTime = Time.unscaledTime + Mathf.Max(0f, inputCooldown);

        UIButtonSpriteSwap selectedSwap = optionSpriteSwaps[selectedIndex];
        if (selectedSwap != null)
        {
            selectedSwap.SetPressed(true);
        }

        switch (selectedIndex)
        {
            case 0:
                ClosePauseMenu();
                break;
            case 1:
                RestartCurrentScene();
                break;
            case 2:
                LoadMainMenu();
                break;
        }
    }

    private void RestartCurrentScene()
    {
        ApplyPauseState(false);
        SpellInventoryManager.Instance.ResetInventory();
        TransitionManager.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadMainMenu()
    {
        ApplyPauseState(false);
        SpellInventoryManager.Instance.ResetInventory();
        TransitionManager.Instance.LoadScene(0);
    }

    private void ApplyPauseState(bool paused)
    {
        if (paused)
        {
            Time.timeScale = 0f;
        }
        else if (!TransitionManager.IsTransitioning)
        {
            Time.timeScale = 1f;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGameplayAudioPaused(paused);
            if (muffleMusicWhenOpen)
            {
                float duration = paused ? musicMuffleTransition : 0f;
                AudioManager.Instance.SetMusicMuffled(paused, duration);
            }
            else if (!paused)
            {
                AudioManager.Instance.SetMusicMuffled(false, 0f);
            }
        }
    }

    private void RefreshOptionVisualState()
    {
        for (int i = 0; i < optionSpriteSwaps.Length; i++)
        {
            UIButtonSpriteSwap spriteSwap = optionSpriteSwaps[i];
            if (spriteSwap != null)
            {
                spriteSwap.SetSelected(i == selectedIndex);
            }

            RectTransform target = optionTargets[i];
            if (target == null)
            {
                continue;
            }

            bool isSelected = i == selectedIndex;
            float targetScale = isSelected ? buttonScalePop : 1f;

            buttonScaleTweens[i]?.Kill();
            buttonScaleTweens[i] = target
                .DOScale(targetScale, popDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
    }

    private void CacheOptions()
    {
        if (optionTargets == null || optionTargets.Length != 3)
        {
            Debug.LogWarning("[PauseMenuController] Assigne exactement 3 options dans optionTargets.");
            return;
        }

        for (int i = 0; i < optionTargets.Length; i++)
        {
            RectTransform target = optionTargets[i];
            if (target == null)
            {
                Debug.LogWarning($"[PauseMenuController] optionTargets[{i}] n'est pas assigne.");
                continue;
            }

            openAnchoredPositions[i] = target.anchoredPosition;
            optionSpriteSwaps[i] = target.GetComponent<UIButtonSpriteSwap>();

            if (optionSpriteSwaps[i] == null)
            {
                Debug.LogWarning($"[PauseMenuController] UIButtonSpriteSwap manquant sur {target.name}.");
            }
        }
    }

    private void CacheVisualIndicator()
    {
        if (visualIndicatorObject != null)
        {
            visualIndicatorOpenPos = visualIndicatorObject.anchoredPosition;
        }
    }

    private void PlaceOptionsOnLeft()
    {
        for (int i = 0; i < optionTargets.Length; i++)
        {
            RectTransform target = optionTargets[i];
            if (target == null)
            {
                continue;
            }

            Vector2 pos = openAnchoredPositions[i];
            pos.x += closedLeftOffsetX;
            target.anchoredPosition = pos;
            target.localScale = Vector3.one;
        }
    }

    private void PlayOptionsOpenAnimation()
    {
        KillOptionTweens();

        int lastAnimatedIndex = -1;
        for (int i = 0; i < optionTargets.Length; i++)
        {
            RectTransform target = optionTargets[i];
            if (target == null)
            {
                continue;
            }

            lastAnimatedIndex = i;
            int capturedIndex = i;
            optionTweens[i] = target
                .DOAnchorPos(openAnchoredPositions[i], slideDuration)
                .SetDelay(optionStagger * i)
                .SetEase(openEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (capturedIndex == lastAnimatedIndex)
                    {
                        isTransitioning = false;
                    }
                });
        }

        if (lastAnimatedIndex < 0)
        {
            isTransitioning = false;
        }

        PlayVisualIndicatorOpenAnimation(optionStagger * 2f);
    }

    private void PlayVisualIndicatorOpenAnimation(float delay)
    {
        if (visualIndicatorObject == null)
        {
            return;
        }

        Vector2 visualPos = visualIndicatorOpenPos;
        visualPos.x += closedLeftOffsetX;
        visualIndicatorObject.anchoredPosition = visualPos;

        visualIndicatorTween?.Kill();
        visualIndicatorTween = visualIndicatorObject
            .DOAnchorPos(visualIndicatorOpenPos, slideDuration)
            .SetDelay(delay)
            .SetEase(openEase)
            .SetUpdate(true);
    }

    private void PlayOptionsCloseAnimation()
    {
        KillOptionTweens();

        int lastAnimatedIndex = -1;
        for (int i = 0; i < optionTargets.Length; i++)
        {
            RectTransform target = optionTargets[i];
            if (target == null)
            {
                continue;
            }

            lastAnimatedIndex = i;
            int capturedIndex = i;
            Vector2 closePos = openAnchoredPositions[i];
            closePos.x += closedRightOffsetX;

            optionTweens[i] = target
                .DOAnchorPos(closePos, slideDuration)
                .SetDelay(optionStagger * i)
                .SetEase(closeEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (capturedIndex == lastAnimatedIndex)
                    {
                        if (pauseRoot != null)
                        {
                            pauseRoot.SetActive(false);
                        }
                        isTransitioning = false;
                    }
                });
        }

        if (lastAnimatedIndex < 0)
        {
            if (pauseRoot != null)
            {
                pauseRoot.SetActive(false);
            }
            isTransitioning = false;
        }

        PlayVisualIndicatorCloseAnimation(0f);
    }

    private void PlayVisualIndicatorCloseAnimation(float delay)
    {
        if (visualIndicatorObject == null)
        {
            return;
        }

        Vector2 closePos = visualIndicatorOpenPos;
        closePos.x += closedRightOffsetX;

        visualIndicatorTween?.Kill();
        visualIndicatorTween = visualIndicatorObject
            .DOAnchorPos(closePos, slideDuration)
            .SetDelay(delay)
            .SetEase(closeEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                PlaceOptionsOnLeft();
            });
    }

    private void KillOptionTweens()
    {
        for (int i = 0; i < optionTweens.Length; i++)
        {
            optionTweens[i]?.Kill();
            optionTweens[i] = null;
        }
    }

    private void CacheBackdropGroup()
    {
        backdropGroup = null;
        if (backdropTarget == null)
        {
            return;
        }

        backdropGroup = backdropTarget.GetComponent<CanvasGroup>();
        if (backdropGroup == null)
        {
            backdropGroup = backdropTarget.AddComponent<CanvasGroup>();
        }
    }

    private void SetBackdropState(bool show, bool animated)
    {
        if (backdropGroup == null)
        {
            return;
        }

        float target = show ? Mathf.Clamp01(backdropTargetAlpha) : 0f;

        if (show && manageBackdropActiveState && !backdropGroup.gameObject.activeSelf)
        {
            backdropGroup.gameObject.SetActive(true);
        }

        backdropTween?.Kill();

        if (!animated)
        {
            backdropGroup.alpha = target;
            if (!show && manageBackdropActiveState)
            {
                backdropGroup.gameObject.SetActive(false);
            }
            return;
        }

        backdropTween = backdropGroup
            .DOFade(target, Mathf.Max(0f, backdropFadeDuration))
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (!show && manageBackdropActiveState)
                {
                    backdropGroup.gameObject.SetActive(false);
                }
            });
    }

    private void OnDisable()
    {
        KillOptionTweens();
        KillButtonScaleTweens();
        KillButtonPunchTweens();
        backdropTween?.Kill();
        visualIndicatorTween?.Kill();
        isTransitioning = false;
        isOpen = false;
        IsPauseMenuOpen = false;

        ApplyPauseState(false);
        SetBackdropState(false, false);
    }

    private void KillButtonScaleTweens()
    {
        for (int i = 0; i < buttonScaleTweens.Length; i++)
        {
            buttonScaleTweens[i]?.Kill();
            buttonScaleTweens[i] = null;
        }
    }

    private void KillButtonPunchTweens()
    {
        for (int i = 0; i < buttonPunchTweens.Length; i++)
        {
            buttonPunchTweens[i]?.Kill();
            buttonPunchTweens[i] = null;
        }
    }
}