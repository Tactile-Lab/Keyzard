using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControler : MonoBehaviour
{
    private const float DirectionEpsilon = 0.01f;      // Seuil pour détecter le changement de direction
    private const float MovementSqrEpsilon = 0.0001f; // Seuil pour considérer que le joueur se déplace

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string moveBoolParameter = "Move";

    [Header("Staff")]
    [SerializeField] private Transform staffTransform;
    [SerializeField] private Transform staffTip;
    [SerializeField] private StaffTipLaunchVFX staffTipLaunchVfx;
    [SerializeField] private float staffSmoothSpeed = 12f;
    [SerializeField] private Vector2 staffOrbitRadii = new Vector2(0.28f, 0.4f);
    [SerializeField] private float diagonalReleaseBuffer = 0.12f;
    [SerializeField] private TypingSortManager typingSortManager;

    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 rawInput;
    private bool anyMoveKeyReleasedThisFrame;
    private bool anyMoveKeyPressedThisFrame;
    private bool facingRight = true;
    private Vector2 lastMoveDir = Vector2.right;
    private float lastReleaseTime = -999f;
    private float staffCurrentAngle = 0f;

    public Vector3 StaffTipPosition => staffTip != null ? staffTip.position : (staffTransform != null ? staffTransform.position : transform.position);

    private void Awake()
    {
        // Récupère automatiquement les composants si non assignés dans l'inspecteur.
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ResolveStaffTipVfx();

        // Empêche les collisions physiques directes joueur <-> ennemis.
        // Le gameplay de contact est géré ailleurs.
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }

    private void Update()
    {
        if (IsMovementBlocked())
        {
            // While glossary pause is open, keep player fully idle and avoid buffering movement input.
            ClearMovementState();
            UpdateAnimationAndFacing();
            UpdateStaffRotation();
            return;
        }

        // 1) Lit l'input en temps réel
        ReadMovementInput();

        // 2) Met à jour visuel + direction mémorisée
        UpdateAnimationAndFacing();

        // 3) Oriente le bâton selon cible / mouvement
        UpdateStaffRotation();
    }

    private void FixedUpdate()
    {
        if (IsMovementBlocked())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Déplacement physique dans FixedUpdate pour une vitesse stable.
        Vector2 velocity = rb.linearVelocity;

        velocity.x = input.x * moveSpeed;
        velocity.y = input.y * moveSpeed;

        rb.linearVelocity = velocity;
    }

    private void UpdateAnimationAndFacing()
    {
        // Mettre à jour la direction du sprite selon l'input X
        if (input.x > DirectionEpsilon)
        {
            facingRight = true;
        }
        else if (input.x < -DirectionEpsilon)
        {
            facingRight = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }

        if (animator != null && !string.IsNullOrEmpty(moveBoolParameter))
        {
            // Active l'animation de marche dès qu'il existe un input de mouvement.
            animator.SetBool(moveBoolParameter, input.sqrMagnitude > 0f);
        }

        if (input.sqrMagnitude > MovementSqrEpsilon)
        {
            // Keep the previous diagonal direction briefly when a key is released,
            // to avoid abrupt direction snapping during diagonal transitions.
            bool withinReleaseBuffer = Time.time - lastReleaseTime <= diagonalReleaseBuffer;

            // On met à jour la direction mémorisée si:
            // - une nouvelle touche vient d'être pressée, ou
            // - aucune touche n'a été relâchée ce frame et on est hors buffer.
            bool shouldUpdateDirection = anyMoveKeyPressedThisFrame || (!anyMoveKeyReleasedThisFrame && !withinReleaseBuffer);
            if (shouldUpdateDirection)
            {
                lastMoveDir = input;
            }
        }
    }

    private void UpdateStaffRotation()
    {
        // Sans pivot de bâton, rien à orienter
        if (staffTransform == null)
        {
            return;
        }

        GameObject closestEnemy = FindClosestEnemy();
        float targetAngle;

        if (closestEnemy != null)
        {
            // Si une cible existe, le bâton pointe directement vers elle
            Vector2 dirToEnemy = (closestEnemy.transform.position - transform.position);
            targetAngle = Mathf.Atan2(dirToEnemy.y, dirToEnemy.x) * Mathf.Rad2Deg;
        }
        else
        {
            // Sans cible, suivre la direction du mouvement avec buffer diagonal
            bool lastWasDiagonal = Mathf.Abs(lastMoveDir.x) > DirectionEpsilon && Mathf.Abs(lastMoveDir.y) > DirectionEpsilon;
            bool nowAxisOnly = Mathf.Abs(rawInput.x) <= DirectionEpsilon || Mathf.Abs(rawInput.y) <= DirectionEpsilon;
            bool withinReleaseBuffer = Time.time - lastReleaseTime <= diagonalReleaseBuffer;
            bool keepLastDiagonal = withinReleaseBuffer && lastWasDiagonal && nowAxisOnly;

            Vector2 dir = (input.sqrMagnitude > MovementSqrEpsilon && !keepLastDiagonal) ? input : lastMoveDir;
            targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        // Éviter le saut brutal entre -180° et 180° en normalisant l'angle
        while (targetAngle - staffCurrentAngle > 180f)
        {
            targetAngle -= 360f;
        }
        while (targetAngle - staffCurrentAngle < -180f)
        {
            targetAngle += 360f;
        }

        // Lissage exponentiel pour une interpolation stable indépendamment du frame rate
        staffCurrentAngle = Mathf.Lerp(
            staffCurrentAngle,
            targetAngle,
            1f - Mathf.Exp(-staffSmoothSpeed * Time.deltaTime)
        );

        staffTransform.localRotation = Quaternion.Euler(0f, 0f, staffCurrentAngle);

        // Le bâton suit une orbite elliptique autour du joueur
        float radians = staffCurrentAngle * Mathf.Deg2Rad;
        Vector2 orbitDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        Vector2 ellipseOffset = new Vector2(orbitDir.x * staffOrbitRadii.x, orbitDir.y * staffOrbitRadii.y);
        staffTransform.localPosition = (Vector3)ellipseOffset;
    }

    private void ReadMovementInput()
    {
        // Lecture directe du clavier via le New Input System.
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            // Aucun clavier détecté: on neutralise l'état d'entrée.
            rawInput = Vector2.zero;
            input = Vector2.zero;
            anyMoveKeyReleasedThisFrame = false;
            anyMoveKeyPressedThisFrame = false;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.downArrowKey.isPressed) y -= 1f;
        if (keyboard.upArrowKey.isPressed) y += 1f;

        // Flags "événementiels" du frame courant (utile pour la logique de buffer diagonal).
        anyMoveKeyReleasedThisFrame =
            keyboard.leftArrowKey.wasReleasedThisFrame ||
            keyboard.rightArrowKey.wasReleasedThisFrame ||
            keyboard.downArrowKey.wasReleasedThisFrame ||
            keyboard.upArrowKey.wasReleasedThisFrame;

        anyMoveKeyPressedThisFrame =
            keyboard.leftArrowKey.wasPressedThisFrame ||
            keyboard.rightArrowKey.wasPressedThisFrame ||
            keyboard.downArrowKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame;

        if (anyMoveKeyReleasedThisFrame)
        {
            // Horodatage du relâchement pour lisser les transitions de direction.
            lastReleaseTime = Time.time;
        }

        // rawInput conserve les axes bruts (-1, 0, 1), input est normalisé.
        rawInput = new Vector2(x, y);
        input = rawInput.normalized;
    }

    private bool IsMovementBlocked()
    {
        return GlossaryToggleController.IsGlossaryOpen || Time.timeScale <= 0f;
    }

    private void ClearMovementState()
    {
        rawInput = Vector2.zero;
        input = Vector2.zero;
        anyMoveKeyReleasedThisFrame = false;
        anyMoveKeyPressedThisFrame = false;
    }

    private GameObject FindClosestEnemy()
    {
        // Priority: selected target from typing system.
        if (typingSortManager != null && typingSortManager.SelectedEnemy != null)
        {
            return typingSortManager.SelectedEnemy.enemy;
        }

        if (GameManager.Instance == null || GameManager.Instance.list_enemies == null)
            return null;

        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var entry in GameManager.Instance.list_enemies)
        {
            if (entry == null || entry.enemy == null) continue;

            float distance = Vector2.Distance(transform.position, entry.enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = entry.enemy;
            }
        }

        return closest;
    }

    public bool TriggerSpellLaunchAnimation(string triggerName)
    {
        if (animator == null || string.IsNullOrEmpty(triggerName))
        {
            return false;
        }

        animator.SetTrigger(triggerName);
        return true;
    }

    public void OnSpellLaunchAnimationEvent()
    {
        typingSortManager?.FirePreparedSpellFromAnimationEvent();
    }

    public void PlayStaffLaunchStartVFX(Sort sortData)
    {
        ResolveStaffTipVfx();
        staffTipLaunchVfx?.PlayLaunchStart(sortData);
    }

    public void PlayStaffLaunchReleaseVFX(Sort sortData)
    {
        ResolveStaffTipVfx();
        staffTipLaunchVfx?.PlayLaunchRelease(sortData);
    }

    private void ResolveStaffTipVfx()
    {
        if (staffTipLaunchVfx != null)
        {
            return;
        }

        Transform tip = staffTip != null ? staffTip : staffTransform;
        if (tip != null)
        {
            staffTipLaunchVfx = tip.GetComponent<StaffTipLaunchVFX>();
        }
    }
}