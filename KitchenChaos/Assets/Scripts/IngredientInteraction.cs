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

    // --- HELD OBJECTS ---
    private IngredientFlee heldIngredient;      // raw ingredient
    private CompletedDish heldDish;             // completed dish
    private CutIngredient heldCutIngredient;    // cut ingredient
    private PlateItem heldPlate;                // plate (behaves like dish, but separate slot)

    // --- HIGHLIGHTED OBJECTS (nearest in range) ---
    private IngredientFlee highlightedIngredient;
    private CompletedDish highlightedDish;
    private CutIngredient highlightedCutIngredient;
    private PlateItem highlightedPlate;

    void Update()
    {
        // If hands are empty → look for something to pick up
        if (!IsCarryingAnything())
        {
            FindNearestPickupable();

            if (Input.GetKeyDown(pickupKey))
            {
                if (highlightedIngredient != null)
                    PickupIngredient(highlightedIngredient);
                else if (highlightedDish != null)
                    PickupDish(highlightedDish);
                else if (highlightedCutIngredient != null)
                    PickupCutIngredient(highlightedCutIngredient);
                else if (highlightedPlate != null)
                    PickupPlate(highlightedPlate);
            }
        }
        // If holding something → allow dropping with Q
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            DropHeldItem();
        }
    }

    // ------------------------------------------------------------------
    // FIND NEAREST PICKUPABLE
    // ------------------------------------------------------------------
    void FindNearestPickupable()
    {
        // Clear previous highlight on raw ingredient
        if (highlightedIngredient != null)
        {
            highlightedIngredient.SetHighlight(false);
            highlightedIngredient = null;
        }

        highlightedDish = null;
        highlightedCutIngredient = null;
        highlightedPlate = null;

        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, ingredientLayer);
        float closestDistance = pickupRange;

        IngredientFlee closestIngredient = null;
        CompletedDish closestDish = null;
        CutIngredient closestCut = null;
        PlateItem closestPlate = null;

        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance > closestDistance) continue;

            // Plate?
            PlateItem plate = col.GetComponent<PlateItem>();
            if (plate != null)
            {
                closestDistance = distance;
                closestPlate = plate;
                closestIngredient = null;
                closestDish = null;
                closestCut = null;
                continue;
            }

            // Cut ingredient?
            CutIngredient cut = col.GetComponent<CutIngredient>();
            if (cut != null)
            {
                closestDistance = distance;
                closestCut = cut;
                closestIngredient = null;
                closestDish = null;
                closestPlate = null;
                continue;
            }

            // Raw ingredient?
            IngredientFlee ingredient = col.GetComponent<IngredientFlee>();
            if (ingredient != null && !ingredient.IsCaught() && !ingredient.IsOnPlate())
            {
                closestDistance = distance;
                closestIngredient = ingredient;
                closestDish = null;
                closestCut = null;
                closestPlate = null;
                continue;
            }

            // Completed dish?
            CompletedDish dish = col.GetComponent<CompletedDish>();
            if (dish != null && !dish.IsBeingHeld())
            {
                closestDistance = distance;
                closestDish = dish;
                closestIngredient = null;
                closestCut = null;
                closestPlate = null;
                continue;
            }
        }

        // Apply highlight and store references
        if (closestIngredient != null)
        {
            highlightedIngredient = closestIngredient;
            highlightedIngredient.SetHighlight(true);
        }
        else if (closestDish != null)
        {
            highlightedDish = closestDish;
        }
        else if (closestCut != null)
        {
            highlightedCutIngredient = closestCut;
        }
        else if (closestPlate != null)
        {
            highlightedPlate = closestPlate;
        }
    }

    // ------------------------------------------------------------------
    // PICKUP
    // ------------------------------------------------------------------
    void PickupIngredient(IngredientFlee ingredient)
    {
        if (ingredient == null || holdPoint == null) return;

        heldIngredient = ingredient;
        heldIngredient.Catch(holdPoint);
        heldIngredient.SetHighlight(false);

        Debug.Log($"[IngredientInteraction] Picked up ingredient: {ingredient.name}");
    }

    void PickupDish(CompletedDish dish)
    {
        if (dish == null || holdPoint == null) return;

        heldDish = dish;
        heldDish.Pickup(holdPoint);

        Debug.Log($"[IngredientInteraction] Picked up dish: {dish.name}");
    }

    void PickupCutIngredient(CutIngredient cut)
    {
        if (cut == null || holdPoint == null) return;

        heldCutIngredient = cut;
        cut.OnPickedUp(holdPoint);

        Debug.Log($"[IngredientInteraction] Picked up cut ingredient: {cut.name}");
    }

    void PickupPlate(PlateItem plate)
    {
        if (plate == null || holdPoint == null) return;

        heldPlate = plate;
        plate.PickUp(holdPoint);

        Debug.Log($"[IngredientInteraction] Picked up plate: {plate.name}");
    }

    // ------------------------------------------------------------------
    // DROP
    // ------------------------------------------------------------------
    public void DropHeldItem()
    {
        Vector3 dropPos = holdPoint.position + transform.forward * 1f;

        if (heldPlate != null)
        {
            heldPlate.Drop(dropPos);
            Debug.Log("[IngredientInteraction] Dropped plate.");
            heldPlate = null;
            return;
        }

        if (heldIngredient != null)
        {
            heldIngredient.Release(dropPos, Quaternion.identity);
            Debug.Log("[IngredientInteraction] Dropped ingredient.");
            heldIngredient = null;
            return;
        }

        if (heldDish != null)
        {
            heldDish.Drop();
            Debug.Log("[IngredientInteraction] Dropped dish.");
            heldDish = null;
            return;
        }

        if (heldCutIngredient != null)
        {
            heldCutIngredient.OnDropped(dropPos);
            Debug.Log("[IngredientInteraction] Dropped cut ingredient.");
            heldCutIngredient = null;
            return;
        }

        Debug.Log("[IngredientInteraction] DropHeldItem called, but nothing is held.");
    }

    // ------------------------------------------------------------------
    // PUBLIC API FOR OTHER SCRIPTS (LEGACY + NEW)
    // ------------------------------------------------------------------

    // Raw ingredient
    public IngredientFlee GetHeldIngredient() => heldIngredient;
    public bool IsCarryingIngredient() => heldIngredient != null;
    public void ClearHeldIngredient() => heldIngredient = null;

    // Cut ingredient
    public CutIngredient GetHeldCutIngredient() => heldCutIngredient;
    public bool IsCarryingCutIngredient() => heldCutIngredient != null;
    public void ClearHeldCutIngredient() => heldCutIngredient = null;

    // Completed dish
    public CompletedDish GetHeldDish() => heldDish;
    public bool IsCarryingDish() => heldDish != null;
    public void ClearHeldDish() => heldDish = null;

    // Plate
    public PlateItem GetHeldPlate() => heldPlate;
    public bool IsCarryingPlate() => heldPlate != null;
    public void ClearHeldPlate() => heldPlate = null;

    // Called by DishRack when it spawns a plate directly into the hand
    public void SetHeldPlate(PlateItem plate)
    {
        heldPlate = plate;
        Debug.Log("[IngredientInteraction] Plate set from DishRack: " + plate.name);
    }

    // “Anything” helper
    public bool IsCarryingAnything()
    {
        return heldIngredient != null ||
               heldDish != null ||
               heldCutIngredient != null ||
               heldPlate != null;
    }

    // Used by older plate / cutting-board code: just clears the ingredient hand
    public void PlaceIngredientOnPlate(Transform plateTransform)
    {
        heldIngredient = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}