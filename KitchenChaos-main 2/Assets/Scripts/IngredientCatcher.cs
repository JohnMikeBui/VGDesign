using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class IngredientCatcher : MonoBehaviour
{
    public Transform holdPoint;
    public string objectTag = "";
    public KeyCode dropKey = KeyCode.G;
    public float pickupCooldown = 0.25f;

    private IngredientFlee carrying;
    private Collider myTrigger;
    private float nextPickupTime = 0f;

    void Awake()
    {
        myTrigger = GetComponent<Collider>();
        var rb = GetComponent<Rigidbody>();

        if (!myTrigger || !myTrigger.isTrigger)
            Debug.LogWarning("IngredientCatcher: This GameObject needs a 3D Collider with IsTrigger = true.");

        if (!rb)
            Debug.LogWarning("IngredientCatcher: Add a kinematic Rigidbody.");
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (carrying != null) return;
        if (Time.time < nextPickupTime) return;

        var flee = other.GetComponentInParent<IngredientFlee>();
        if (flee == null || flee.IsCaught()) return;

        if (!holdPoint)
        {
            Debug.LogWarning("HoldPoint not assigned");
            return;
        }

        flee.Catch(holdPoint);
        carrying = flee;
        nextPickupTime = Time.time + pickupCooldown;

        if (ScoreManager.Instance != null)
        {
            int points = flee.GetPointValue();
            ScoreManager.Instance.AddPoints(points);
            ScoreManager.Instance.ShowPointPopup(flee.transform.position, points);
        }

        Debug.Log($"Caught ingredient: {carrying.name}");
    }

    void Update()
    {
        if (carrying != null && Input.GetKeyDown(dropKey))
        {
            DropIngredient();
        }
    }

    // Drop the ingredient on the ground
    public void DropIngredient()
    {
        if (carrying == null) return;

        Vector3 dropPos = holdPoint.position + transform.forward * 0.8f + Vector3.down * 0.1f;
        carrying.Release(dropPos, Quaternion.identity);
        Debug.Log($"Dropped: {carrying.name}");
        carrying = null;
        nextPickupTime = Time.time + pickupCooldown;

        if (myTrigger && myTrigger.enabled)
            StartCoroutine(DisableTriggerTemporarily(pickupCooldown));
    }

    // Place ingredient on plate (called by PlateInteraction)
    public void PlaceIngredientOnPlate(Transform plateTransform)
    {
        if (carrying == null) return;

        // Move ingredient to plate
        carrying.transform.SetParent(plateTransform);
        carrying.transform.localPosition = Vector3.up * 0.5f;
        carrying.transform.localRotation = Quaternion.identity;

        // Disable the flee behavior
        carrying.enabled = false;

        Debug.Log($"Placed {carrying.name} on plate!");

        carrying = null;
        nextPickupTime = Time.time + pickupCooldown;
    }

    IEnumerator DisableTriggerTemporarily(float seconds)
    {
        myTrigger.enabled = false;
        yield return new WaitForSeconds(seconds);
        myTrigger.enabled = true;
    }

    public bool IsCarrying() => carrying != null;

    public IngredientFlee GetCarriedIngredient() => carrying;
}