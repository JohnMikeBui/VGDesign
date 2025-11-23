using UnityEngine;
using System.Collections.Generic;

public class IngredientFlee : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 3f;
    public float detectionRange = 5f;
    public float wanderSpeed = 1f;

    [Header("Score")]
    public int pointValue = 10;

    [Header("Highlight")]
    public Color highlightColor = new Color(0.3f, 0.5f, 1f, 1f);
    public float highlightIntensity = 2f;

    private Transform player;
    private Vector3 wanderDirection;
    private float wanderTimer;
    private bool isCaught = false;
    private bool isPlacedOnPlate = false;
    private Transform holdTransform;

    // Highlight materials
    private Renderer[] renderers;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Renderer, Material[]> highlightMaterials = new Dictionary<Renderer, Material[]>();
    private bool isHighlighted = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("IngredientFlee: No GameObject with tag 'Player' found!");
        }

        ChooseNewWanderDirection();
        SetupHighlightMaterials();
    }

    void SetupHighlightMaterials()
    {
        renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            // Store original materials
            originalMaterials[rend] = rend.materials;

            // Create highlight materials
            Material[] highlightMats = new Material[rend.materials.Length];
            for (int i = 0; i < rend.materials.Length; i++)
            {
                highlightMats[i] = new Material(rend.materials[i]);
                highlightMats[i].EnableKeyword("_EMISSION");
                highlightMats[i].SetColor("_EmissionColor", highlightColor * highlightIntensity);
            }
            highlightMaterials[rend] = highlightMats;
        }
    }

    public void SetHighlight(bool highlight)
    {
        if (isHighlighted == highlight) return;
        isHighlighted = highlight;

        foreach (Renderer rend in renderers)
        {
            if (highlight)
            {
                rend.materials = highlightMaterials[rend];
            }
            else
            {
                rend.materials = originalMaterials[rend];
            }
        }
    }

    void Update()
    {
        // Don't do anything if placed on plate
        if (isPlacedOnPlate) return;

        if (isCaught)
        {
            // If caught, stay at hold position (KEEP THIS - needed for proper holding!)
            if (holdTransform != null)
            {
                transform.position = holdTransform.position;
                transform.rotation = holdTransform.rotation;
            }
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            // Run away from player
            Vector3 runDirection = (transform.position - player.position).normalized;
            transform.position += runDirection * runSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(runDirection);
        }
        else
        {
            // Wander around
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0)
            {
                ChooseNewWanderDirection();
            }

            transform.position += wanderDirection * wanderSpeed * Time.deltaTime;
            if (wanderDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(wanderDirection);
            }
        }
    }

    void ChooseNewWanderDirection()
    {
        wanderDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        wanderTimer = Random.Range(2f, 5f);
    }

    // Called when caught by player
    public void Catch(Transform holdPoint)
    {
        isCaught = true;
        holdTransform = holdPoint;

        // Parent to hold point FIRST
        transform.SetParent(holdPoint);

        // Then set LOCAL position and rotation to zero
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        SetHighlight(false);

        // Disable physics if any
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // FIXED: Disable ALL colliders (including children)
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    // Called when placed on plate - permanently disable movement
    // Called when placed on plate - permanently disable movement
    public void PlaceOnPlate()
    {
        isPlacedOnPlate = true;
        isCaught = false;
        holdTransform = null;
        SetHighlight(false);

        // Ensure physics is completely frozen
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false; // NEW: Turn off collision detection entirely
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // Disable ALL colliders (including children)
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Disable this script entirely
        this.enabled = false;
    }

    // Called when released back to world (not used in new system but kept for flexibility)
    public void Release(Vector3 position, Quaternion rotation)
    {
        if (isPlacedOnPlate) return; // Can't release if on plate

        isCaught = false;
        holdTransform = null;
        transform.SetParent(null);
        transform.position = position;
        transform.rotation = rotation;

        // Re-enable physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Re-enable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        ChooseNewWanderDirection();
    }

    public bool IsCaught() => isCaught;
    public bool IsOnPlate() => isPlacedOnPlate;
    public int GetPointValue() => pointValue;

    void OnDestroy()
    {
        // Clean up created materials
        foreach (var matArray in highlightMaterials.Values)
        {
            foreach (var mat in matArray)
            {
                if (mat != null) Destroy(mat);
            }
        }
    }
}