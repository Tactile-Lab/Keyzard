using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string moveBoolParameter = "Move";

    [Header("Staff")]
    [SerializeField] private Transform staffTransform;
    [SerializeField] private float staffAngleOffset = 0f;
    [SerializeField] private Vector2 staffLocalOffset = new Vector2(0.25f, 0f);
    [SerializeField] private bool flipStaffSprite = true;
    [SerializeField] private SpriteRenderer staffSpriteRenderer;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool facingRight = true;
    private Vector2 lastMoveDir = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (staffSpriteRenderer == null && staffTransform != null)
        {
            staffSpriteRenderer = staffTransform.GetComponent<SpriteRenderer>();
        }
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

        UpdateAnimationAndFacing();
        UpdateStaffRotation();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }

    private void UpdateAnimationAndFacing()
    {
        if (input.x > 0.01f)
        {
            facingRight = true;
        }
        else if (input.x < -0.01f)
        {
            facingRight = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }

        if (animator != null && !string.IsNullOrEmpty(moveBoolParameter))
        {
            animator.SetBool(moveBoolParameter, input.sqrMagnitude > 0f);
        }

        if (input.sqrMagnitude > 0.0001f)
        {
            lastMoveDir = input;
        }
    }

    private void UpdateStaffRotation()
    {
        if (staffTransform == null) return;

        Vector2 dir = (input.sqrMagnitude > 0.0001f) ? input : lastMoveDir;

        // Skip staff updates when moving mostly up/down
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + staffAngleOffset;

        if (!facingRight)
        {
            angle = 180f - angle;
        }

        staffTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

        // Keep staff in front based on facing direction
        float x = Mathf.Abs(staffLocalOffset.x) * (facingRight ? 1f : -1f);
        staffTransform.localPosition = new Vector3(x, staffLocalOffset.y, staffTransform.localPosition.z);

        if (flipStaffSprite && staffSpriteRenderer != null)
        {
            staffSpriteRenderer.flipX = !facingRight;
        }
    }
}