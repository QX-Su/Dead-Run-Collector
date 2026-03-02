using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 12f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -25f;

    [Header("Ground Assist")]
    public float groundStickVelocity = -2f; // 贴地用，避免小抖动导致一直不算落地
    public float jumpBufferTime = 0.12f;
    public float coyoteTime = 0.10f;

    [Header("Camera Relative Move (Recommended)")]
    public Transform cameraTransform;

    [Header("Animation (Optional)")]
    public Animator animator;
    public string speedParam = "Speed";          // float
    public string groundedParam = "IsGrounded";  // bool
    public string jumpTrigger = "Jump";          // trigger
    public float animSpeedSmoothing = 12f;

    CharacterController cc;

    // Input cache
    Vector2 moveInput;
    bool sprintHeld;

    // Jump assist
    float jumpBufferTimer;
    float coyoteTimer;

    float verticalVelocity;
    float smoothedAnimSpeed;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // ---- Read Input (every frame) ----
        moveInput = ReadMoveInput();
        sprintHeld = IsSprintHeld();

        if (WasJumpPressedThisFrame())
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // Grounded
        bool grounded = cc.isGrounded;
        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // ---- Movement Direction ----
        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = cameraTransform.right;     right.y = 0f;   right.Normalize();
            moveDir = (right * moveInput.x + forward * moveInput.y);
        }

        // 防止轻微漂移：给一个死区，解决“站着也跑”
        if (moveDir.sqrMagnitude < 0.0004f) moveDir = Vector3.zero; // ~0.02 的死区
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);

        float speed = sprintHeld ? sprintSpeed : walkSpeed;

        // ---- Rotate to face move dir ----
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // ---- Vertical (gravity + jump) ----
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = groundStickVelocity;

        // jump: buffer + coyote
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            verticalVelocity = Mathf.Sqrt(2f * jumpHeight * -gravity);

            if (animator != null && !string.IsNullOrEmpty(jumpTrigger))
                animator.SetTrigger(jumpTrigger);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * speed;
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);

        // ---- Animation update ----
        UpdateAnimator(moveDir, speed, cc.isGrounded);
    }

    void UpdateAnimator(Vector3 moveDir, float currentSpeed, bool grounded)
    {
        if (animator == null) return;

        // 0..1 的移动强度（站着=0）
        float target = moveDir.magnitude;

        // 平滑一下，让Idle切换更自然
        smoothedAnimSpeed = Mathf.Lerp(smoothedAnimSpeed, target, animSpeedSmoothing * Time.deltaTime);

        animator.SetFloat(speedParam, smoothedAnimSpeed);

        if (!string.IsNullOrEmpty(groundedParam))
            animator.SetBool(groundedParam, grounded);
    }

    // ---- Input System (keyboard) ----
    Vector2 ReadMoveInput()
    {
        Vector2 v = Vector2.zero;
        if (Keyboard.current == null) return v;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  v.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) v.x += 1;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    v.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  v.y -= 1;

        return v.normalized;
    }

    bool WasJumpPressedThisFrame()
    {
        if (Keyboard.current == null) return false;
        return Keyboard.current.spaceKey.wasPressedThisFrame;
    }

    bool IsSprintHeld()
    {
        if (Keyboard.current == null) return false;
        return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
    }
}