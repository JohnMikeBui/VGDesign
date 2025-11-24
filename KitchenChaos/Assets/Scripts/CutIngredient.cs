using UnityEngine;

public class CutIngredient : MonoBehaviour
{
    public string ingredientName; // example: "OnionSlices"

    private Rigidbody rb;
    private Collider[] cols;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();

        // Freeze movement
        if (rb != null)
            rb.isKinematic = true;
    }

    public void OnPickedUp(Transform holdPoint)
    {
        // Parent to player
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Disable physics
        if (rb != null)
            rb.isKinematic = true;

        foreach (var c in cols)
            c.enabled = false;
    }

    public void OnDropped(Vector3 dropPos)
    {
        transform.SetParent(null);
        transform.position = dropPos;

        // Re-enable physics
        if (rb != null)
            rb.isKinematic = false;

        foreach (var c in cols)
            c.enabled = true;
    }
}