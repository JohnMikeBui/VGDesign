using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The player transform this target follows")]
    public Transform playerTransform;

    [Header("Rotation Mode")]
    [Tooltip("How should this target's rotation be determined?")]
    public RotationMode rotationMode = RotationMode.AlwaysForward;

    [Header("Always Forward Settings")]
    [Tooltip("When using AlwaysForward mode, this is the fixed forward direction")]
    public Vector3 fixedForward = Vector3.forward; // World forward (0,0,1)

    [Header("Follow Player Settings (Optional)")]
    [Tooltip("When using FollowPlayer mode, smooth rotation speed")]
    public float rotationSmoothSpeed = 5f;

    [Tooltip("When using FollowPlayer mode, delay before starting to rotate")]
    public float rotationDelay = 0.5f;

    private float timeSinceLastMovement = 0f;
    private Quaternion targetRotation;
    private Vector3 lastPlayerPosition;

    public enum RotationMode
    {
        AlwaysForward,      // Always faces world forward (recommended)
        FollowPlayerSmooth, // Smoothly rotates to match player
        FollowPlayerInstant // Instantly matches player rotation
    }

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("CameraFollowTarget: No player transform assigned!");
            return;
        }

        // Initialize rotation based on mode
        switch (rotationMode)
        {
            case RotationMode.AlwaysForward:
                transform.rotation = Quaternion.LookRotation(fixedForward);
                break;
            case RotationMode.FollowPlayerSmooth:
            case RotationMode.FollowPlayerInstant:
                transform.rotation = playerTransform.rotation;
                break;
        }

        lastPlayerPosition = playerTransform.position;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // Always follow player's position
        transform.position = playerTransform.position;

        // Handle rotation based on mode
        switch (rotationMode)
        {
            case RotationMode.AlwaysForward:
                // Keep fixed rotation (camera always recenters to same direction)
                transform.rotation = Quaternion.LookRotation(fixedForward);
                break;

            case RotationMode.FollowPlayerSmooth:
                // Detect if player is moving
                bool isMoving = Vector3.Distance(playerTransform.position, lastPlayerPosition) > 0.001f;

                if (isMoving)
                {
                    timeSinceLastMovement = 0f;
                    // Smoothly rotate toward player's facing direction while moving
                    targetRotation = playerTransform.rotation;
                }
                else
                {
                    timeSinceLastMovement += Time.deltaTime;

                    // After delay, start rotating back to forward
                    if (timeSinceLastMovement > rotationDelay)
                    {
                        targetRotation = Quaternion.LookRotation(fixedForward);
                    }
                }

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSmoothSpeed * Time.deltaTime
                );
                break;

            case RotationMode.FollowPlayerInstant:
                // Instantly match player rotation
                transform.rotation = playerTransform.rotation;
                break;
        }

        lastPlayerPosition = playerTransform.position;
    }

    // Visualize the forward direction in editor
    void OnDrawGizmos()
    {
        if (rotationMode == RotationMode.AlwaysForward)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, fixedForward * 2f);
            Gizmos.DrawWireSphere(transform.position + fixedForward * 2f, 0.2f);
        }
    }
}