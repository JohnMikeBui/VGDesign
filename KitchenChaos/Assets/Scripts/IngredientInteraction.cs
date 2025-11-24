using UnityEngine;
using System.Collections.Generic;

public class IngredientInteraction : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;
    public LayerMask ingredientLayer;

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(0.3f, 0.5f, 1f, 1f);
    public float highlightIntensity = 2f;

    private IngredientFlee heldIngredient;
    private CompletedDish heldDish;
    private IngredientFlee highlightedIngredient;
    private CompletedDish highlightedDish;

    void Update()
    {
        if (heldIngredient == null && heldDish == null)
        {
            FindNearestPickupable();

            if (Input.GetKeyDown(pickupKey))
            {
                if (highlightedIngredient != null)
                {
                    PickupIngredient(highlightedIngredient);
                }
                else if (highlightedDish != null)
                {
                    PickupDish(highlightedDish);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q)) // Optional: Drop with Q
        {
            DropHeldItem();
        }
    }

    void FindNearestPickupable()
    {
        // Clear previous highlights
        if (highlightedIngredient != null)
        {
            highlightedIngredient.SetHighlight(false);
            highlightedIngredient = null;
        }
        highlightedDish = null;

        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, ingredientLayer);
        float closestDistance = pickupRange;
        IngredientFlee closestIngredient = null;
        CompletedDish closestDish = null;

        foreach (Collider col in colliders)
        {
            // Check for ingredients
            IngredientFlee ingredient = col.GetComponent<IngredientFlee>();
            if (ingredient != null && !ingredient.IsCaught() && !ingredient.IsOnPlate())
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIngredient = ingredient;
                    closestDish = null; // Prioritize ingredient if closer
                }
            }

            // Check for completed dishes
            CompletedDish dish = col.GetComponent<CompletedDish>();
            if (dish != null && !dish.IsBeingHeld())
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIngredient = null; // Prioritize dish if closer
                    closestDish = dish;
                }
            }
        }

        if (closestIngredient != null)
        {
            highlightedIngredient = closestIngredient;
            highlightedIngredient.SetHighlight(true);
        }
        else if (closestDish != null)
        {
            highlightedDish = closestDish;
            // You can add a highlight effect to dishes if desired
        }
    }

    void PickupIngredient(IngredientFlee ingredient)
    {
        if (ingredient == null || !holdPoint) return;

        heldIngredient = ingredient;
        heldIngredient.Catch(holdPoint);
        heldIngredient.SetHighlight(false);
        Debug.Log($"Picked up: {ingredient.name}");
    }

    void PickupDish(CompletedDish dish)
    {
        if (dish == null || !holdPoint) return;

        heldDish = dish;
        heldDish.Pickup(holdPoint);
        Debug.Log($"Picked up completed dish!");
    }

    void DropHeldItem()
    {
        // DROP RAW INGREDIENT
        if (heldIngredient != null)
        {
            Vector3 dropPos = holdPoint.position + transform.forward * 1f;

            heldIngredient.Release(dropPos, Quaternion.identity);
            Debug.Log($"Dropped ingredient: {heldIngredient.name}");

            heldIngredient = null;
            return;
        }

        // DROP DISH
        if (heldDish != null)
        {
            heldDish.Drop();
            heldDish = null;
            Debug.Log("Dropped dish!");
            return;
        }
    }


    // Called by PlateInteraction after it handles the placement
    public void PlaceIngredientOnPlate(Transform plateTransform)
    {
        // Just clear the reference - PlateInteraction handles the actual placement
        heldIngredient = null;
    }

    public bool IsCarryingIngredient()
    {
        return heldIngredient != null;
    }

    public bool IsCarryingDish()
    {
        return heldDish != null;
    }

    public bool IsCarryingAnything()
    {
        return heldIngredient != null || heldDish != null;
    }

    public IngredientFlee GetHeldIngredient()
    {
        return heldIngredient;
    }

    public CompletedDish GetHeldDish()
    {
        return heldDish;
    }

    public void ClearHeldDish()
    {
        heldDish = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}