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
    [SerializeField] private float staffSmoothSpeed = 12f;
    [SerializeField] private Vector2 staffOrbitRadii = new Vector2(0.28f, 0.4f);
    [SerializeField] private float diagonalReleaseBuffer = 0.12f;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 rawInput;
    private bool anyMoveKeyReleasedThisFrame;
    private bool anyMoveKeyPressedThisFrame;
    private bool facingRight = true;
    private Vector2 lastMoveDir = Vector2.right;
    private float lastReleaseTime = -999f;
    private float staffCurrentAngle = 0f;

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

        anyMoveKeyReleasedThisFrame =
            kb.leftArrowKey.wasReleasedThisFrame ||
            kb.rightArrowKey.wasReleasedThisFrame ||
            kb.downArrowKey.wasReleasedThisFrame ||
            kb.upArrowKey.wasReleasedThisFrame;

        anyMoveKeyPressedThisFrame =
            kb.leftArrowKey.wasPressedThisFrame ||
            kb.rightArrowKey.wasPressedThisFrame ||
            kb.downArrowKey.wasPressedThisFrame ||
            kb.upArrowKey.wasPressedThisFrame;

        if (anyMoveKeyReleasedThisFrame)
        {
            lastReleaseTime = Time.time;
        }

        rawInput = new Vector2(x, y);
        input = rawInput.normalized;

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
            bool withinReleaseBuffer = Time.time - lastReleaseTime <= diagonalReleaseBuffer;
            bool shouldUpdateDirection = anyMoveKeyPressedThisFrame || (!anyMoveKeyReleasedThisFrame && !withinReleaseBuffer);
            if (shouldUpdateDirection)
            {
                lastMoveDir = input;
            }
        }
    }

    private void UpdateStaffRotation()
    {
        if (staffTransform == null) return;

        bool lastWasDiagonal = Mathf.Abs(lastMoveDir.x) > 0.01f && Mathf.Abs(lastMoveDir.y) > 0.01f;
        bool nowAxisOnly = Mathf.Abs(rawInput.x) <= 0.01f || Mathf.Abs(rawInput.y) <= 0.01f;
        bool withinReleaseBuffer = Time.time - lastReleaseTime <= diagonalReleaseBuffer;
        bool keepLastDiagonal = withinReleaseBuffer && lastWasDiagonal && nowAxisOnly;

        Vector2 dir = (input.sqrMagnitude > 0.0001f && !keepLastDiagonal) ? input : lastMoveDir;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Normalize angle to maintain continuity (avoid jumping between -180 and 180)
        while (targetAngle - staffCurrentAngle > 180f) targetAngle -= 360f;
        while (targetAngle - staffCurrentAngle < -180f) targetAngle += 360f;

        // Smoothly interpolate angle
        staffCurrentAngle = Mathf.Lerp(
            staffCurrentAngle,
            targetAngle,
            1f - Mathf.Exp(-staffSmoothSpeed * Time.deltaTime)
        );

        // Apply rotation and position based on the current interpolated angle
        staffTransform.localRotation = Quaternion.Euler(0f, 0f, staffCurrentAngle);

        float radians = staffCurrentAngle * Mathf.Deg2Rad;
        Vector2 orbitDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        Vector2 ellipseOffset = new Vector2(orbitDir.x * staffOrbitRadii.x, orbitDir.y * staffOrbitRadii.y);
        staffTransform.localPosition = (Vector3)ellipseOffset;
    }
}