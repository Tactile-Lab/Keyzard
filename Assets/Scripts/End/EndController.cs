using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class EndController : MonoBehaviour
{
    public static bool IsEndMenuOpen { get; private set; }

    public void Awake()
    {
        IsEndMenuOpen = false;
    }
    public void EnableReturn()
    {
        IsEndMenuOpen = true;
        Time.timeScale = 0f;
        AudioManager.Instance?.PlaySFXEvent(SFXEventKey.EndDemoOpen);
        StartCoroutine(WaitForSpace());
    }

    private IEnumerator WaitForSpace()
    {
        yield return new WaitForSeconds(0.2f); // anti double input

        while (true)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                AudioManager.Instance?.PlaySFXEvent(SFXEventKey.EndDemoConfirm);
                SpellInventoryManager.Instance.ResetInventory();
                AudioManager.Instance?.ResetMusicRuntime();
                IsEndMenuOpen = false;
                Time.timeScale = 1f;
                TransitionManager.Instance.LoadScene(0);
                yield break;
            }

            yield return null;
        }
    }
}