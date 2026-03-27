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
        StartCoroutine(WaitForSpace());
    }

    private IEnumerator WaitForSpace()
    {
        yield return new WaitForSeconds(0.2f); // anti double input

        while (true)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                SpellInventoryManager.Instance.ResetInventory();
                IsEndMenuOpen = false;
                TransitionManager.Instance.LoadScene(0);
                yield break;
            }

            yield return null;
        }
    }
}