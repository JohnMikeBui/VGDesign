using UnityEngine;
using System.Collections.Generic;

public class PlateInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("Recipe Requirements")]
    public List<string> requiredIngredients = new List<string> { "Tomato", "Lettuce", "Cheese", "Patty" };
    public bool requireExactIngredients = true; // If true, only accept ingredients in the list

    [Header("Completed Dish")]
    public GameObject completeDishPrefab; // The burger/sandwich sprite
    public Vector3 dishSpawnOffset = Vector3.zero; // Set to zero to spawn at plate level
    public float dishScale = 1f;
    public bool hideOriginalPlate = true; // Hide the original plate when burger spawns
    public int dishPointValue = 100; // Points given when delivered to customer

    [Header("Ingredient Stacking")]
    public float ingredientStackHeight = 0.15f; // How high each ingredient sits
    public float ingredientScale = 0.7f; // Scale down ingredients on plate

    private bool playerNearby = false;
    private IngredientInteraction playerInteraction;
    private List<GameObject> ingredientsOnPlate = new List<GameObject>();
    private HashSet<string> ingredientNames = new HashSet<string>();
    private bool dishCompleted = false;

    void Update()
    {
        if (dishCompleted) return; // Don't accept more ingredients after completion

        if (playerNearby && Input.GetKeyDown(interactionKey))
        {
            if (playerInteraction != null && playerInteraction.IsCarryingIngredient())
            {
                IngredientFlee heldIngredient = playerInteraction.GetHeldIngredient();

                // Check if we should accept this ingredient
                if (ShouldAcceptIngredient(heldIngredient.gameObject))
                {
                    PlaceIngredientOnPlate(heldIngredient);
                    CheckRecipeCompletion();
                }
                else
                {
                    Debug.Log($"{heldIngredient.name} is not needed for this recipe!");
                }
            }
            else
            {
                Debug.Log("No ingredient to place!");
            }
        }
    }

    bool ShouldAcceptIngredient(GameObject ingredient)
    {
        string ingredientName = ingredient.name.Replace("(Clone)", "").Trim();

        // If we don't require exact ingredients, accept anything
        if (!requireExactIngredients) return true;

        // Check if this ingredient is in the required list
        if (!requiredIngredients.Contains(ingredientName))
        {
            return false;
        }

        // Check if we already have this ingredient (optional: allow duplicates)
        // Uncomment the line below if you want to prevent duplicate ingredients
        // if (ingredientNames.Contains(ingredientName)) return false;

        return true;
    }

    void PlaceIngredientOnPlate(IngredientFlee ingredient)
    {
        if (ingredient == null) return;

        // CRITICAL: Disable the flee script IMMEDIATELY before anything else!
        ingredient.enabled = false;

        // Store the original scale BEFORE parenting
        Vector3 originalScale = ingredient.transform.localScale;

        // Parent to plate FIRST
        ingredient.transform.SetParent(transform);

        // Apply scale relative to original scale
        ingredient.transform.localScale = originalScale * ingredientScale;

        // Set rotation
        ingredient.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // Calculate bottom offset using mesh bounds
        float bottomOffset = 0f;
        MeshFilter meshFilter = ingredient.GetComponentInChildren<MeshFilter>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            float meshBottomLocalY = meshBounds.min.y;
            bottomOffset = -meshBottomLocalY * ingredient.transform.localScale.y;
        }

        // Calculate stack position
        float stackHeight = ingredientsOnPlate.Count * ingredientStackHeight;
        ingredient.transform.localPosition = new Vector3(0, stackHeight + bottomOffset, 0);

        // Change to Plate layer
        ingredient.gameObject.layer = LayerMask.NameToLayer("Plate");
        foreach (Transform child in ingredient.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("Plate");
        }

        // DISABLE CHARACTER CONTROLLER (this was the culprit!)
        CharacterController charController = ingredient.GetComponentInChildren<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
            Destroy(charController);
        }

        // Disable NavMeshAgent if present
        UnityEngine.AI.NavMeshAgent navAgent = ingredient.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
            Destroy(navAgent);
        }

        // Disable ALL colliders IMMEDIATELY
        Collider[] colliders = ingredient.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = false;
            Destroy(col);
        }

        // Handle Rigidbody
        Rigidbody rb = ingredient.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Destroy(rb);
        }

        // Call PlaceOnPlate
        ingredient.PlaceOnPlate();

        ingredientsOnPlate.Add(ingredient.gameObject);
        string ingredientName = ingredient.name.Replace("(Clone)", "").Trim();
        ingredientNames.Add(ingredientName);

        Debug.Log($"Placed {ingredientName} on plate at local Y: {ingredient.transform.localPosition.y} ({ingredientsOnPlate.Count}/{requiredIngredients.Count})");

        playerInteraction.PlaceIngredientOnPlate(transform);
    }


    void CheckRecipeCompletion()
    {
        // Check if we have all required ingredients
        bool recipeComplete = true;

        if (requireExactIngredients)
        {
            // Must have all required ingredients
            foreach (string required in requiredIngredients)
            {
                if (!ingredientNames.Contains(required))
                {
                    recipeComplete = false;
                    break;
                }
            }
        }
        else
        {
            // Just need the right number of ingredients
            recipeComplete = ingredientsOnPlate.Count >= requiredIngredients.Count;
        }

        if (recipeComplete)
        {
            CompleteDish();
        }
    }

    void CompleteDish()
    {
        dishCompleted = true;

        Debug.Log("Recipe Complete! Creating finished dish...");

        // Hide/destroy all individual ingredients
        foreach (GameObject ingredient in ingredientsOnPlate)
        {
            if (ingredient != null)
            {
                ingredient.SetActive(false); // Hide instead of destroy for now
                // Or use: Destroy(ingredient);
            }
        }

        // OPTIONAL: Hide the original plate's mesh so only the burger's plate shows
        if (hideOriginalPlate)
        {
            MeshRenderer plateMesh = GetComponent<MeshRenderer>();
            if (plateMesh != null)
            {
                plateMesh.enabled = false;
            }
        }

        // Spawn the complete dish
        if (completeDishPrefab != null)
        {
            Vector3 spawnPos = transform.position + dishSpawnOffset;
            GameObject completeDish = Instantiate(completeDishPrefab, spawnPos, Quaternion.identity, transform);
            completeDish.transform.localScale = Vector3.one * dishScale;

            // Add the CompletedDish component so it can be picked up
            CompletedDish dishScript = completeDish.GetComponent<CompletedDish>();
            if (dishScript == null)
            {
                dishScript = completeDish.AddComponent<CompletedDish>();
            }
            dishScript.pointValue = dishPointValue;

            Debug.Log("Burger/Sandwich complete! 🍔 Pick it up with 'E' and deliver it!");

            // Add points for completing the recipe
            ScoreManager.Instance?.AddPoints(100);
        }
        else
        {
            Debug.LogWarning("No complete dish prefab assigned!");
        }
    }

    // Reset the plate (optional - for reusing the plate)
    public void ResetPlate()
    {
        foreach (GameObject ingredient in ingredientsOnPlate)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        ingredientsOnPlate.Clear();
        ingredientNames.Clear();
        dishCompleted = false;

        // Re-enable the plate mesh if it was hidden
        if (hideOriginalPlate)
        {
            MeshRenderer plateMesh = GetComponent<MeshRenderer>();
            if (plateMesh != null)
            {
                plateMesh.enabled = true;
            }
        }

        Debug.Log("Plate reset!");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerInteraction = other.GetComponent<IngredientInteraction>();

            if (playerInteraction == null)
            {
                Debug.LogWarning("Player doesn't have IngredientInteraction component!");
            }
            else if (!dishCompleted)
            {
                Debug.Log($"Press 'E' to place ingredient ({ingredientsOnPlate.Count}/{requiredIngredients.Count})");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerInteraction = null;
        }
    }

    // Visualize in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + dishSpawnOffset, Vector3.one * 0.3f);
    }
}