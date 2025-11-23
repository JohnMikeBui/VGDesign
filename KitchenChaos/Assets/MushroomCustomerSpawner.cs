using System.Collections;
using UnityEngine;
using TMPro;

public class MushroomCustomerSpawner : MonoBehaviour
{
    [Header("Prefabs & Spawn Point")]
    public GameObject[] mushroomCustomerPrefabs;
    public Transform waypoint1;
    public Transform waypoint2;

    [Header("Scene References to Inject")]
    public Transform playerTransform;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Spawning Rules")]
    public float spawnIntervalSeconds = 120f;
    public int maxAlive = 2;
    public string mushroomTag = "MushroomCustomer";

    [Header("Naming")]
    public Transform spawnAt;
    public string baseName = "MushroomCustomer";
    private int nextIndex = 1;

    private void Start()
    {
        // Fallbacks if not set in inspector
        if (!waypoint1)
        {
            var w = GameObject.FindGameObjectWithTag("Waypoint1");
            if (w) waypoint1 = w.transform;
        }
        if (!waypoint2)
        {
            var w2 = GameObject.FindGameObjectWithTag("Waypoint2");
            if (w2) waypoint2 = w2.transform;
        }

        // FIX: Spawn at waypoint2 (outside) instead of waypoint1 (inside)
        if (!spawnAt) spawnAt = waypoint2;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (CountAlive() < maxAlive)
                SpawnOne();

            yield return new WaitForSeconds(spawnIntervalSeconds);
        }
    }

    private int CountAlive() => GameObject.FindGameObjectsWithTag(mushroomTag).Length;

    private void SpawnOne()
    {
        if (mushroomCustomerPrefabs == null || mushroomCustomerPrefabs.Length == 0)
        {
            Debug.LogWarning("[Spawner] No prefabs assigned.");
            return;
        }

        if (!spawnAt || !waypoint1 || !waypoint2 || !playerTransform || !dialoguePanel || !dialogueText)
        {
            Debug.LogWarning("[Spawner] Missing references (spawnAt/waypoints/player/UI). Assign them in the Inspector.");
            return;
        }

        var prefab = mushroomCustomerPrefabs[Random.Range(0, mushroomCustomerPrefabs.Length)];
        var pos = spawnAt.position;
        var rot = spawnAt.rotation;
        var go = Instantiate(prefab, pos, rot);

        go.tag = mushroomTag;
        go.name = $"{baseName}{nextIndex++}";

        var ai = go.GetComponent<CustomerAI>();
        if (!ai)
        {
            Debug.LogWarning("[Spawner] Spawned prefab has no CustomerAI component.");
            return;
        }

        ai.Initialize(playerTransform, waypoint1, waypoint2, dialoguePanel, dialogueText);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        var t = spawnAt ? spawnAt : waypoint2 ? waypoint2 : transform;
        Gizmos.DrawWireSphere(t.position, 0.3f);
    }
#endif
}