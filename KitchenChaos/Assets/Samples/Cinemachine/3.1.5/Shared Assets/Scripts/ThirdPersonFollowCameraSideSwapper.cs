using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;         // Player transform

    [Header("Settings")]
    public float distance = 4f;      // How far behind player
    public float height = 2f;        // Height offset
    public float followSpeed = 10f;  // How smoothly camera follows
    public float rotationSpeed = 200f;

    [Header("Rotation Limits")]
    public float minY = -30f;
    public float maxY = 60f;

    private float yaw;
    private float pitch;

    void Start()
    {
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // === Mouse Look ===
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * rotationSpeed * Time.deltaTime;
        pitch -= mouseY * rotationSpeed * Time.deltaTime; 
        pitch = Mathf.Clamp(pitch, minY, maxY);

        // === Calculate rotation ===
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // === Calculate desired camera position ===
        Vector3 offset = rotation * new Vector3(0, height, -distance);
        Vector3 desiredPosition = target.position + offset;

        // === Smoothly move camera ===
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // === Always look at player ===
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
