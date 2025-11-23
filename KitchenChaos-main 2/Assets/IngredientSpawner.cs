using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class IngredientSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class IngredientSpawnData
    {
        public GameObject prefab;
        public int maxCount = 3;
        [HideInInspector] public int currentCount = 0;
    }

    [Header("Spawn Settings")]
    public IngredientSpawnData[] ingredientTypes;
    public float spawnHeight = 5f; // Y position to spawn from
    public float spawnRadius = 15f; // How far from center to search for spawn points
    public Vector3 cafeCenter = Vector3.zero; // Center of your cafe

    [Header("NavMesh Settings")]
    public int maxNavMeshSampleAttempts = 30;
    public float navMeshSampleDistance = 2f;

    private static IngredientSpawnManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple IngredientSpawnManagers found! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Spawn initial ingredients
        foreach (var ingredientData in ingredientTypes)
        {
            for (int i = 0; i < ingredientData.maxCount; i++)
            {
                SpawnIngredient(ingredientData);
            }
        }
    }

    // Public static method for ingredients to call when placed on plate
    public static void OnIngredientPlacedOnPlate(GameObject ingredient)
    {
        if (instance == null)
        {
            Debug.LogWarning("IngredientSpawnManager instance not found!");
            return;
        }

        instance.HandleIngredientPlaced(ingredient);
    }

    private void HandleIngredientPlaced(GameObject placedIngredient)
    {
        // Find which ingredient type this was
        foreach (var ingredientData in ingredientTypes)
        {
            if (placedIngredient.name.Contains(ingredientData.prefab.name))
            {
                ingredientData.currentCount--;
                if (ingredientData.currentCount < 0) ingredientData.currentCount = 0;

                // Spawn a replacement immediately
                SpawnIngredient(ingredientData);
                break;
            }
        }
    }

    private void SpawnIngredient(IngredientSpawnData ingredientData)
    {
        if (ingredientData.currentCount >= ingredientData.maxCount)
        {
            return; // Already at max capacity
        }

        Vector3? spawnPosition = GetRandomNavMeshPosition();

        if (spawnPosition.HasValue)
        {
            // Spawn at the height, will fall down naturally
            Vector3 skyPosition = new Vector3(spawnPosition.Value.x, spawnHeight, spawnPosition.Value.z);

            GameObject newIngredient = Instantiate(ingredientData.prefab, skyPosition, Quaternion.identity);

            // Make sure it has a rigidbody for falling
            Rigidbody rb = newIngredient.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newIngredient.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false;
            rb.useGravity = true;

            ingredientData.currentCount++;

            Debug.Log($"Spawned {ingredientData.prefab.name} at {skyPosition} (will fall to {spawnPosition.Value})");
        }
        else
        {
            Debug.LogWarning($"Failed to find valid NavMesh position for {ingredientData.prefab.name}");
        }
    }

    private Vector3? GetRandomNavMeshPosition()
    {
        for (int i = 0; i < maxNavMeshSampleAttempts; i++)
        {
            // Random point within circle around cafe center
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPoint = cafeCenter + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Try to find nearest point on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return null; // Failed to find valid position
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cafeCenter, spawnRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(cafeCenter + Vector3.up * spawnHeight, Vector3.one * 0.5f);
    }
}