using UnityEngine;

public class ThirdPersonMouseCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Distance")]
    public float distance = 6f;
    public float heightOffset = 1.5f; // Look at this height above player's feet

    [Header("Mouse Control")]
    public float mouseSensitivity = 3f;
    public float minPitch = -20f;  // Look down limit
    public float maxPitch = 80f;   // Look up limit

    [Header("Starting Angles")]
    public float startYaw = 0f;    // Horizontal rotation (0 = behind player)
    public float startPitch = 45f; // Vertical angle (45 = looking down at angle)

    [Header("Smoothing (Optional)")]
    public float rotationSmoothing = 0f; // 0 = instant, higher = smoother

    // Internal state
    private float currentYaw;
    private float currentPitch;
    private float targetYaw;
    private float targetPitch;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonMouseCamera: No target assigned!");
            enabled = false;
            return;
        }

        // Initialize angles
        currentYaw = startYaw;
        currentPitch = startPitch;
        targetYaw = startYaw;
        targetPitch = startPitch;

        // Lock and hide cursor for better camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Update target angles based on mouse movement
        targetYaw += mouseX;
        targetPitch -= mouseY; // Inverted (moving mouse up = look up)

        // Clamp pitch to prevent flipping
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // Apply smoothing if enabled
        if (rotationSmoothing > 0f)
        {
            currentYaw = Mathf.Lerp(currentYaw, targetYaw, rotationSmoothing * Time.deltaTime);
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, rotationSmoothing * Time.deltaTime);
        }
        else
        {
            currentYaw = targetYaw;
            currentPitch = targetPitch;
        }

        // Calculate camera position using spherical coordinates
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = rotation * (Vector3.back * distance); // back = -Z direction

        // Position camera at distance behind target (with height offset for look-at point)
        Vector3 lookAtPoint = target.position + Vector3.up * heightOffset;
        transform.position = lookAtPoint + offset;

        // Make camera look at the target point
        transform.LookAt(lookAtPoint);

        // Toggle cursor lock with ESC
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
    }

    // Optional: Visualize camera setup in editor
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Draw look-at point
        Vector3 lookAtPoint = target.position + Vector3.up * heightOffset;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lookAtPoint, 0.2f);

        // Draw distance sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lookAtPoint, distance);

        // Draw line from look point to camera
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(lookAtPoint, transform.position);
    }
}