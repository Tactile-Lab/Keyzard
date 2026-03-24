
using UnityEngine;
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private Animator animator;

    void Start()
    {
        player.Died += OnPlayerDied;
    }

    void OnPlayerDied()
    {
        Time.timeScale = 0f;

        GameObject staff = GameObject.Find("Magic Staff");
        staff.SetActive(false);
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.SetTrigger("Die");
    }

    void OnDestroy()
    {
        player.Died -= OnPlayerDied;
    }
}