using UnityEngine;
using Unity.Cinemachine;

public class CameraAxisResetter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The FreeLook camera to control")]
    public CinemachineCamera freeLookCamera;

    [Header("Settings")]
    [Tooltip("Enable debug logging")]
    public bool debugMode = false;

    private CinemachineOrbitalFollow orbitalFollow;

    void Start()
    {
        if (freeLookCamera == null)
        {
            freeLookCamera = GetComponent<CinemachineCamera>();
        }

        if (freeLookCamera == null)
        {
            Debug.LogError("CameraAxisResetter: No CinemachineCamera assigned or found!");
            enabled = false;
            return;
        }

        orbitalFollow = freeLookCamera.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null)
        {
            Debug.LogError("CameraAxisResetter: No CinemachineOrbitalFollow component found on camera!");
            enabled = false;
            return;
        }

        // Disable recentering completely
        InputAxis horizontalAxis = orbitalFollow.HorizontalAxis;
        horizontalAxis.Recentering.Enabled = false;
        orbitalFollow.HorizontalAxis = horizontalAxis;

        if (debugMode)
        {
            Debug.Log("Camera initialized: Recentering disabled");
        }
    }
}