using UnityEngine;
using UnityEngine.InputSystem;

public class GlossaryToggleController : MonoBehaviour
{
    public static bool IsGlossaryOpen { get; private set; }

    [Header("Glossary UI")]
    [SerializeField] private GameObject glossaryRoot;

    [Header("Input")]
    [SerializeField] private Key toggleKey = Key.LeftCtrl;
    [SerializeField] private bool acceptRightCtrl = true;

    [Header("Audio")]
    [SerializeField] private bool muffleMusicWhenOpen = true;
    [SerializeField] private float musicMuffleTransition = 0.25f;

    private bool isOpen;

    private void Awake()
    {
        if (glossaryRoot != null)
        {
            isOpen = glossaryRoot.activeSelf;
            if (isOpen)
            {
                ApplyOpenState(true, false);
            }
            else
            {
                ApplyOpenState(false, false);
            }
        }
        else
        {
            ApplyOpenState(false, false);
        }
    }

    private void Update()
    {
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
        SetGlossaryOpen(!isOpen);
    }

    public void SetGlossaryOpen(bool open)
    {
        if (isOpen == open)
        {
            return;
        }

        isOpen = open;
        ApplyOpenState(open, true);
    }

    private void ApplyOpenState(bool open, bool animateAudio)
    {
        IsGlossaryOpen = open;

        if (glossaryRoot != null)
        {
            glossaryRoot.SetActive(open);
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
        IsGlossaryOpen = false;
        if (isOpen)
        {
            isOpen = false;
            ApplyOpenState(false, true);
        }
    }
}
