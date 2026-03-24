using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class GlossaryToggleController : MonoBehaviour
{
    public static bool IsGlossaryOpen { get; private set; }

    [Header("Glossary UI")]
    [SerializeField] private GameObject glossaryRoot;
    [SerializeField] private RectTransform glossaryPanel;
    [SerializeField] private string glossarySearchByTag = "Glossary";
    [SerializeField] private string glossarySearchByName = "";

    [Header("Input")]
    [SerializeField] private Key toggleKey = Key.LeftCtrl;
    [SerializeField] private bool acceptRightCtrl = true;
    [SerializeField] private float toggleCooldown = 0.2f;

    [Header("Audio")]
    [SerializeField] private bool muffleMusicWhenOpen = true;
    [SerializeField] private float musicMuffleTransition = 0.25f;

    [Header("Animation")]
    [SerializeField] private float openX = 0f;
    [SerializeField] private float closedLeftX = -1600f;
    [SerializeField] private float closedRightX = 1600f;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;
    [SerializeField] private bool enablePop = true;
    [SerializeField] private float popScaleFrom = 0.96f;
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private Ease popEase = Ease.OutBack;

    [Header("Backdrop")]
    [SerializeField] private GameObject backdropTarget;
    [SerializeField] private float backdropTargetAlpha = 0.45f;
    [SerializeField] private float backdropFadeDuration = 0.2f;
    [SerializeField] private bool manageBackdropActiveState = false;

    private bool isOpen;
    private bool isTransitioning;
    private float nextToggleAllowedTime;
    private Tween moveTween;
    private Tween popTween;
    private Tween backdropTween;
    private Vector3 basePanelScale = Vector3.one;
    private CanvasGroup backdropGroup;
    private bool missingReferencesWarningLogged;
    private float lastResolveAttemptTime = -999f;
    private const float ResolveRetryInterval = 1f;

    private void Awake()
    {
        ResolveGlossaryReferences();

        if (glossaryPanel != null)
        {
            basePanelScale = glossaryPanel.localScale;
        }

        CacheBackdropGroup();

        if (glossaryRoot != null)
        {
            isOpen = glossaryRoot.activeSelf;
            if (isOpen)
            {
                SetPanelPosition(openX);
                SetBackdropState(true, false);
                ApplyOpenState(true, false);
            }
            else
            {
                SetPanelPosition(closedLeftX);
                SetBackdropState(false, false);
                ApplyOpenState(false, false);
            }
        }
        else
        {
            SetBackdropState(false, false);
            ApplyOpenState(false, false);
        }
    }

    private void Update()
    {
        ResolveGlossaryReferencesDynamic();
        if (!HasUsableGlossary())
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool leftPressed = IsKeyPressedThisFrame(keyboard, toggleKey);
        bool rightPressed = acceptRightCtrl && keyboard.rightCtrlKey.wasPressedThisFrame;

        if (leftPressed || rightPressed)
        {
            ToggleGlossary();
        }
    }

    public void ToggleGlossary()
    {
        if (!HasUsableGlossary())
        {
            return;
        }

        if (isTransitioning || Time.unscaledTime < nextToggleAllowedTime)
        {
            return;
        }

        nextToggleAllowedTime = Time.unscaledTime + Mathf.Max(0f, toggleCooldown);
        SetGlossaryOpen(!isOpen);
    }

    public void SetGlossaryOpen(bool open)
    {
        if (!HasUsableGlossary())
        {
            return;
        }

        if (isOpen == open)
        {
            return;
        }

        isOpen = open;
        PlayTransition(open);
    }

    private void PlayTransition(bool opening)
    {
        moveTween?.Kill();
        popTween?.Kill();
        backdropTween?.Kill();
        isTransitioning = true;

        if (opening)
        {
            if (glossaryRoot != null)
            {
                glossaryRoot.SetActive(true);
            }

            // Garantit que chaque ouverture démarre visuellement de la gauche.
            SetPanelPosition(closedLeftX);
            SetBackdropState(true, true);
            ApplyOpenState(true, true);
            AnimatePanel(openX, openEase, true, false);
            return;
        }

        // Closing should immediately unpause gameplay; panel can keep animating in unscaled time.
        ApplyOpenState(false, true);
        SetBackdropState(false, true);
        AnimatePanel(closedRightX, closeEase, false, true);
    }

    private void AnimatePanel(float targetX, Ease ease, bool opening, bool disableAtEnd)
    {
        if (glossaryPanel == null)
        {
            if (disableAtEnd && glossaryRoot != null)
            {
                glossaryRoot.SetActive(false);
            }

            ApplyOpenState(opening, true);
            return;
        }

        Vector2 target = glossaryPanel.anchoredPosition;
        target.x = targetX;

        moveTween = glossaryPanel
            .DOAnchorPos(target, slideDuration)
            .SetEase(ease)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (disableAtEnd && glossaryRoot != null)
                {
                    glossaryRoot.SetActive(false);
                    // Réarme la prochaine ouverture depuis la gauche.
                    SetPanelPosition(closedLeftX);
                }

                ApplyOpenState(opening, true);
                isTransitioning = false;
            });

        if (opening && enablePop)
        {
            glossaryPanel.localScale = basePanelScale * popScaleFrom;
            popTween = glossaryPanel
                .DOScale(basePanelScale, popDuration)
                .SetEase(popEase)
                .SetUpdate(true);
        }
        else
        {
            glossaryPanel.localScale = basePanelScale;
        }
    }

    private void ApplyOpenState(bool open, bool animateAudio)
    {
        IsGlossaryOpen = open;

        if (!open && glossaryRoot != null && glossaryRoot.activeSelf && glossaryPanel == null)
        {
            glossaryRoot.SetActive(false);
        }

        if (open && glossaryRoot != null && !glossaryRoot.activeSelf)
        {
            glossaryRoot.SetActive(true);
        }

        Time.timeScale = open ? 0f : 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetGameplayAudioPaused(open);
            if (muffleMusicWhenOpen)
            {
                float duration = animateAudio ? musicMuffleTransition : 0f;
                AudioManager.Instance.SetMusicMuffled(open, duration);
            }
            else
            {
                AudioManager.Instance.SetMusicMuffled(false, animateAudio ? musicMuffleTransition : 0f);
            }
        }
    }

    private bool IsKeyPressedThisFrame(Keyboard keyboard, Key key)
    {
        switch (key)
        {
            case Key.LeftCtrl:
                return keyboard.leftCtrlKey.wasPressedThisFrame;
            case Key.RightCtrl:
                return keyboard.rightCtrlKey.wasPressedThisFrame;
            case Key.LeftShift:
                return keyboard.leftShiftKey.wasPressedThisFrame;
            case Key.RightShift:
                return keyboard.rightShiftKey.wasPressedThisFrame;
            case Key.LeftAlt:
                return keyboard.leftAltKey.wasPressedThisFrame;
            case Key.RightAlt:
                return keyboard.rightAltKey.wasPressedThisFrame;
            case Key.Space:
                return keyboard.spaceKey.wasPressedThisFrame;
            case Key.Tab:
                return keyboard.tabKey.wasPressedThisFrame;
            default:
                return keyboard[key].wasPressedThisFrame;
        }
    }

    private void OnDisable()
    {
        // Evite de laisser le jeu en pause si l'objet est désactivé.
        moveTween?.Kill();
        popTween?.Kill();
        backdropTween?.Kill();
        isTransitioning = false;
        IsGlossaryOpen = false;
        if (isOpen)
        {
            isOpen = false;
            ApplyOpenState(false, true);
        }
        else
        {
            Time.timeScale = 1f;
        }

        SetBackdropState(false, false);
    }

    private void SetPanelPosition(float x)
    {
        if (glossaryPanel == null)
        {
            return;
        }

        Vector2 pos = glossaryPanel.anchoredPosition;
        pos.x = x;
        glossaryPanel.anchoredPosition = pos;
        glossaryPanel.localScale = basePanelScale;
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

    private void ResolveGlossaryReferences()
    {
        if (glossaryRoot == null && glossaryPanel != null)
        {
            glossaryRoot = glossaryPanel.gameObject;
        }

        if (glossaryPanel == null && glossaryRoot != null)
        {
            glossaryPanel = glossaryRoot.GetComponent<RectTransform>();
            if (glossaryPanel == null)
            {
                glossaryPanel = glossaryRoot.GetComponentInChildren<RectTransform>(true);
            }
        }
    }

    private void ResolveGlossaryReferencesDynamic()
    {
        // Retry resolution periodically to handle scene changes/prefab instantiation.
        if (Time.time - lastResolveAttemptTime < ResolveRetryInterval)
        {
            return;
        }

        lastResolveAttemptTime = Time.time;

        // Already found, keep it.
        if (glossaryRoot != null)
        {
            ResolveGlossaryReferences();
            return;
        }

        // First: check if glossaryRoot is on this GameObject or a parent/child.
        if (glossaryRoot == null)
        {
            glossaryRoot = gameObject;
        }

        if (glossaryPanel == null)
        {
            glossaryPanel = gameObject.GetComponent<RectTransform>();
            if (glossaryPanel == null)
            {
                glossaryPanel = gameObject.GetComponentInChildren<RectTransform>();
            }
        }

        ResolveGlossaryReferences();
        if (glossaryRoot != null)
        {
            return;
        }

        // Search by tag in current scene.
        if (!string.IsNullOrEmpty(glossarySearchByTag))
        {
            try
            {
                GameObject found = GameObject.FindWithTag(glossarySearchByTag);
                if (found != null)
                {
                    glossaryRoot = found;
                    ResolveGlossaryReferences();
                    return;
                }
            }
            catch (UnityEngine.UnityException)
            {
                // Tag doesn't exist; continue with name search.
            }
        }

        // Search by name in current scene.
        if (!string.IsNullOrEmpty(glossarySearchByName))
        {
            GameObject found = GameObject.Find(glossarySearchByName);
            if (found != null)
            {
                glossaryRoot = found;
                ResolveGlossaryReferences();
                return;
            }
        }

        // Fallback: look for any active Canvas with a glossary-like name.
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.gameObject.name.Contains("Glossary") || canvas.gameObject.name.Contains("glossary"))
            {
                glossaryRoot = canvas.gameObject;
                ResolveGlossaryReferences();
                return;
            }
        }
    }

    private bool HasUsableGlossary()
    {
        if (glossaryRoot != null)
        {
            return true;
        }

        if (!missingReferencesWarningLogged)
        {
            missingReferencesWarningLogged = true;
            Debug.LogWarning("[GlossaryToggleController] Glossary references missing. Assign glossaryRoot (or glossaryPanel) in inspector to enable Ctrl toggle.");
        }

        IsGlossaryOpen = false;
        Time.timeScale = 1f;
        return false;
    }
}
