using UnityEngine;

public class ThirdPersonMouseCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Distance")]
    public float distance = 6f;
    public float minDistance = 1.5f;   // how close camera can get when blocked
    public float collisionRadius = 0.3f;
    public float heightOffset = 1.5f;

    [Header("Mouse Control")]
    public float mouseSensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 80f;

    [Header("Starting Angles")]
    public float startYaw = 0f;
    public float startPitch = 45f;

    [Header("Smoothing")]
    public float rotationSmoothing = 0f;
    public float collisionSmoothSpeed = 15f; // smooth camera push-in when blocked

    private float currentYaw, currentPitch;
    private float targetYaw, targetPitch;
    private float currentDistance;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonMouseCamera: No target assigned!");
            enabled = false;
            return;
        }

        currentYaw = targetYaw = startYaw;
        currentPitch = targetPitch = startPitch;
        currentDistance = distance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // === MOUSE LOOK ===
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        targetYaw += mouseX;
        targetPitch -= mouseY;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

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

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // === DESIRED CAMERA POSITION BEFORE COLLISION CHECK ===
        Vector3 lookAt = target.position + Vector3.up * heightOffset;
        Vector3 desiredPos = lookAt - rotation * Vector3.forward * distance;

        // === CAMERA COLLISION PREVENTION USING SPHERECAST ===
        RaycastHit hit;
        float adjustedDistance = distance;

        if (Physics.SphereCast(
            lookAt,                      // ray origin (look point)
            collisionRadius,             // small sphere radius
            desiredPos - lookAt,         // direction
            out hit,
            distance,                    // max distance to check
            ~0,                          // collide with all layers
            QueryTriggerInteraction.Ignore
        ))
        {
            // pull camera in front of wall
            adjustedDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }

        // Smoothly interpolate distance for nicer effect
        currentDistance = Mathf.Lerp(currentDistance, adjustedDistance, Time.deltaTime * collisionSmoothSpeed);

        // Final camera position
        transform.position = lookAt - rotation * Vector3.forward * currentDistance;

        // Look at the target
        transform.LookAt(lookAt);

        // Escape to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Vector3 lookAt = target.position + Vector3.up * heightOffset;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lookAt, 0.15f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(lookAt, collisionRadius);
    }
}