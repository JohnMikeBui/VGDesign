using UnityEngine;

public class DishRack : MonoBehaviour
{
    [Header("Plate Settings")]
    public GameObject platePrefab; // Assign your plate prefab here

    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Player Hold Point")]
    public Transform holdPoint; // <-- Assign this in Inspector
    public Transform playerHoldPoint;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (holdPoint == null)
            Debug.LogError("[DishRack] ERROR: HoldPoint not assigned in Inspector!");
    }

    void Update()
    {
        if (player == null || holdPoint == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > interactRange) return;

        if (Input.GetKeyDown(interactKey))
            TryGivePlate();
    }

 public void TryGivePlate()
    {
        if (playerHoldPoint == null || platePrefab == null)
        {
            Debug.LogError("[DishRack] Missing hold point or plate prefab!");
            return;
        }

        GameObject plateObj = Instantiate(platePrefab, playerHoldPoint.position, playerHoldPoint.rotation);
        PlateItem plate = plateObj.GetComponent<PlateItem>();
        if (plate == null)
        {
            Debug.LogError("[DishRack] Plate prefab has no PlateItem script!");
            return;
        }

        // Attach plate visually
        plate.PickUp(playerHoldPoint);

        // ✨ Tell IngredientInteraction that player is holding this plate
        IngredientInteraction ii = player.GetComponent<IngredientInteraction>();
        if (ii != null)
        {
            ii.SetHeldPlate(plate);   // <-- NEW LINE!
        }

        Debug.Log("[DishRack] Plate given to player.");
    }
}
