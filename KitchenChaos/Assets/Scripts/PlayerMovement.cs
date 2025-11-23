using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpForce = 7f;
    public float gravity = 10f;

    [Header("Rotation")]
    public float rotationSpeed = 10f; // How fast player rotates to face camera direction

    [Header("References")]
    public Animator animator;
    public Transform model;
    public GameObject dialoguePanel;

    [Header("Camera (Auto-assigned if not set)")]
    public Transform cameraTransform; // Camera to use for relative movement

    private CharacterController controller;
    private float verticalVelocity;

    [HideInInspector] public bool canMove = true;
    [SerializeField] private string dialoguePanelTag = "DialoguePanel";

    // Reference to ingredient catcher
    private IngredientCatcher ingredientCatcher;

    private void Awake()
    {
        // Auto-wire dialogue panel if not assigned
        if (!dialoguePanel)
        {
            var panel = GameObject.FindGameObjectWithTag(dialoguePanelTag);
            if (panel) dialoguePanel = panel;
        }

        if (dialoguePanel) dialoguePanel.SetActive(false);

        controller = GetComponent<CharacterController>();
        ingredientCatcher = GetComponent<IngredientCatcher>();

        // Auto-assign main camera if not set
        if (cameraTransform == null)
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("PlayerMovement: No camera assigned and Camera.main not found!");
            }
        }
    }

    private void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);

        // Lock and hide cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!canMove)
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetFloat("Horizontal", 0f);
                animator.SetFloat("Vertical", 0f);
            }
            return;
        }

        // Toggle cursor lock with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // === GET INPUT (Keyboard + Controller) ===
        float horizontal = Input.GetAxis("Horizontal"); // smoothed
        float vertical = Input.GetAxis("Vertical");   // smoothed

        // === FORTNITE-STYLE: Player faces camera direction ===
        if (cameraTransform != null)
        {
            // Get camera's forward direction (flattened)
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            // Smoothly rotate player to face camera direction (always faces forward)
            if (cameraForward.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        // === CAMERA-RELATIVE MOVEMENT (Strafe-style) ===
        Vector3 moveDirection = Vector3.zero;

        if (cameraTransform != null)
        {
            // Get camera's forward and right directions (flattened to XZ plane)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Flatten to ignore Y component (keep movement on ground plane)
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction relative to camera
            // W/S = forward/back, A/D = strafe left/right
            moveDirection = (cameraForward * vertical + cameraRight * horizontal);
        }
        else
        {
            // Fallback: world-space movement if no camera
            moveDirection = new Vector3(horizontal, 0f, vertical);
        }

        // Normalize diagonal movement
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // === SPEED ===
        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire3");
        float speed = running ? runSpeed : walkSpeed;

        bool moving = moveDirection.sqrMagnitude > 0.01f;

        // === APPLY MOVEMENT ===
        Vector3 move = Vector3.zero;
        if (moving)
        {
            move = moveDirection * speed * Time.deltaTime;
        }

        // === JUMPING & GRAVITY ===
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            // Jump: Space or controller A button
            if (Input.GetButtonDown("Jump"))
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 verticalMove = new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        controller.Move(move + verticalMove);

        // === ANIMATION ===
        if (animator != null)
        {
            // These drive your 2D blend tree
            animator.SetFloat("Horizontal", horizontal); // strafing
            animator.SetFloat("Vertical", vertical);   // forward/back

            // Optional: still keep Speed if you use it anywhere else
            Vector3 flatVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
            animator.SetFloat("Speed", flatVelocity.magnitude);
        }

        // === MODEL ROTATION (matches player rotation) ===
        if (model != null)
        {
            model.rotation = Quaternion.Slerp(model.rotation, transform.rotation, Time.deltaTime * 10f);
        }
    }

    // Helper method to get ingredient catcher
    public IngredientCatcher GetIngredientCatcher()
    {
        return ingredientCatcher;
    }
}
