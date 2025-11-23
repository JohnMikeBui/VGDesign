using UnityEngine;

public class CompletedDish : MonoBehaviour
{
    [Header("Pickup Settings")]
    public bool canBePickedUp = true;
    public int pointValue = 100; // Points given when delivered to customer

    private Transform holdPoint;
    private bool isBeingHeld = false;
    private Rigidbody rb;
    private Collider[] colliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            // Add a collider if none exists
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = false;
        }

        // Make sure it's on a layer the player can interact with
        gameObject.layer = LayerMask.NameToLayer("Ingredient");
    }

    public void Pickup(Transform newHoldPoint)
    {
        if (!canBePickedUp) return;

        holdPoint = newHoldPoint;
        isBeingHeld = true;

        // Disable physics
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable collisions while held
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Parent to hold point
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        if (!isBeingHeld) return;

        isBeingHeld = false;
        holdPoint = null;

        // Re-enable physics
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Re-enable collisions
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        // Unparent
        transform.SetParent(null);
    }

    public bool IsBeingHeld()
    {
        return isBeingHeld;
    }

    public int GetPointValue()
    {
        return pointValue;
    }
}
