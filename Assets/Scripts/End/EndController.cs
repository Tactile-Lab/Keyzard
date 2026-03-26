using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class EndController : MonoBehaviour
{
    public void EnableReturn()
    {
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
                TransitionManager.Instance.LoadScene(0);
                yield break;
            }

            yield return null;
        }
    }
}