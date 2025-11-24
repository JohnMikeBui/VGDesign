using UnityEngine;

public class RamenServing : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject ramenBowlPrefab;

    [Header("Settings")]
    public float interactRange = 2f;

    private IngredientInteraction player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")
                           .GetComponent<IngredientInteraction>();
    }

    void Update()
    {
        float dist = Vector3.Distance(player.transform.position, transform.position);
        if (dist <= interactRange && Input.GetKeyDown(KeyCode.E))
            TryServeRamen();
    }

    void TryServeRamen()
    {
        PlateItem plate = player.GetHeldPlate();
        if (plate == null)
        {
            Debug.Log("[RamenServing] Player not holding a plate.");
            return;
        }

        // Remove plate
        Destroy(plate.gameObject);
        player.ClearHeldPlate();

        // Spawn ramen bowl INTO HAND
        Transform hand = player.holdPoint;
        GameObject ramen = Instantiate(ramenBowlPrefab, hand.position, hand.rotation);

        ramen.transform.SetParent(hand);
        ramen.transform.localPosition = Vector3.zero;
        ramen.transform.localRotation = Quaternion.identity;

        // Make sure it's treated as a dish
        CompletedDish dish = ramen.GetComponent<CompletedDish>();
        if (dish == null)
            dish = ramen.AddComponent<CompletedDish>();

        dish.Pickup(hand);

        // Delete ONLY the ramen pot
        Destroy(gameObject);
    }
}
