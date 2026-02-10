using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
        {
            input = Vector2.zero;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.rightArrowKey.isPressed) x += 1f;
        if (kb.downArrowKey.isPressed) y -= 1f;
        if (kb.upArrowKey.isPressed) y += 1f;

        input = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }
}