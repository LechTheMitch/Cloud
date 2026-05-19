using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private Vector2 moveInput;
    private bool canMove = true;
    private bool wasMovingLastFrame = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    private void Update()
    {
        if (!canMove || !GameManager.Instance.IsPlaying()) return;

        if (DialogueManager.Instance.IsDialogueActive())
        {
            rb.linearVelocity = Vector2.zero;
            // reset footstep state so re-entry works cleanly
            if (wasMovingLastFrame)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.StopFootsteps();
                wasMovingLastFrame = false;
            }
            return;
        }

        // if pressing shift, run instead of walk
        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            moveSpeed = 5f;
        else
            moveSpeed = 3f;

        GatherInput();
        HandleFootstepAudio();
    }


    private void HandleFootstepAudio()
    {
        if (AudioManager.Instance == null) return;

        bool isMovingNow = moveInput.magnitude > 0.1f;

        if (isMovingNow && !wasMovingLastFrame)
        {
            // transition: stopped → moving
            AudioManager.Instance.StartFootsteps();
        }
        else if (!isMovingNow && wasMovingLastFrame)
        {
            // transition: moving → stopped
            AudioManager.Instance.StopFootsteps();
        }

        wasMovingLastFrame = isMovingNow;
    }

    private void Start()
    {
        if (animator != null)
        {
            // default facing south at game start
            animator.SetFloat("LastMoveX", 0f);
            animator.SetFloat("LastMoveY", -1f);
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GatherInput()
    {
        // supports WASD, arrow keys, and controller left stick / d-pad
        Vector2 keyboard = Vector2.zero;

        // WASD
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            keyboard.y += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            keyboard.y -= 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            keyboard.x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            keyboard.x += 1f;

        // controller left stick + d-pad
        Vector2 stick = Vector2.zero;
        Vector2 dpad = Vector2.zero;

        if (Gamepad.current != null)
        {
            stick = Gamepad.current.leftStick.ReadValue();
            dpad = Gamepad.current.dpad.ReadValue();
        }

        // take whichever input has the highest magnitude
        moveInput = keyboard.magnitude > stick.magnitude
            ? keyboard
            : stick.magnitude > dpad.magnitude ? stick : dpad;

        // normalize so diagonal isn't faster
        if (moveInput.magnitude > 1f)
            moveInput = moveInput.normalized;
    }

    private void Move()
    {
        rb.linearVelocity = moveInput * moveSpeed;

        if (animator != null)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetFloat("Speed", moveInput.magnitude);

            // only record last direction when actually moving
            if (moveInput.magnitude > 0.1f)
            {
                animator.SetFloat("LastMoveX", moveInput.x);
                animator.SetFloat("LastMoveY", moveInput.y);
            }
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!value) rb.linearVelocity = Vector2.zero;
    }
}