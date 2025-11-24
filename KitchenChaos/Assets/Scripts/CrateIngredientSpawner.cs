using UnityEngine;

public class CrateIngredientSpawner : MonoBehaviour
{
    [Header("Ingredient")]
    public GameObject ingredientPrefab;
    public string ingredientName;   // "Noodle", "Fish", "Ham", etc.

    [Header("Spawn Behavior")]
    public Transform spawnPoint;
    public float spawnForce = 8f;
    public float upwardForce = 3f;

    [Header("Cooldown")]
    public float spawnCooldown = 2f;
    private float nextSpawnTime = 0f;

    [Header("Interaction")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
            TrySpawn();
    }

    void TrySpawn()
    {
        if (player == null) return;

        if (Vector3.Distance(player.position, transform.position) > interactRange)
            return;

        if (Time.time < nextSpawnTime)
            return;

        // Check global limit
        if (!IngredientGlobalTracker.Instance.CanSpawn(ingredientName))
        {
            Debug.Log($"{ingredientName} LIMIT REACHED!");
            return;
        }

        SpawnIngredient();
    }

    void SpawnIngredient()
    {
        nextSpawnTime = Time.time + spawnCooldown;

        Transform point = spawnPoint != null ? spawnPoint : transform;

        GameObject obj = Instantiate(ingredientPrefab, point.position, Quaternion.identity);

        // Register ingredient globally
        IngredientGlobalTracker.Instance.Register(ingredientName);

        // Apply jump force toward player
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (!rb) rb = obj.AddComponent<Rigidbody>();

        Vector3 dirToPlayer = (player.position - point.position).normalized;
        Vector3 force = dirToPlayer * spawnForce + Vector3.up * upwardForce;

        rb.AddForce(force, ForceMode.VelocityChange);

        Debug.Log($"Spawned {ingredientName}. Now: {IngredientGlobalTracker.Instance.GetCurrentCount(ingredientName)}");
    }
}