using UnityEngine;

public class PotTrigger : MonoBehaviour
{
    [Header("Player Hand Slot")]
    public Transform playerHandSlot;
    public KeyCode interactKey = KeyCode.E;

    [Header("Cooking Result")]
    public GameObject ramenPotPrefab;    // The cooked pot prefab
    public Transform spawnPoint;

    // Ingredient tracking
    private bool hasNoodles = false;
    private bool hasHam = false;
    private bool hasOnion = false;
    private bool hasBroth = false;

    private void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        if (playerHandSlot == null)
            Debug.LogError("[POT] Player hand slot NOT assigned.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        if (playerHandSlot.childCount == 0)
        {
            Debug.Log("[POT] Player holding nothing.");
            return;
        }

        Transform held = playerHandSlot.GetChild(0);
        string name = held.name.ToLower();

        Debug.Log("[POT] Player holding: " + held.name);

        if (name.Contains("noodle")) hasNoodles = true;
        else if (name.Contains("ham")) hasHam = true;
        else if (name.Contains("onion")) hasOnion = true;
        else if (name.Contains("broth")) hasBroth = true;
        else return;

        Destroy(held.gameObject);

        TryCook();
    }

    void TryCook()
    {
        if (!hasNoodles || !hasHam || !hasOnion || !hasBroth) return;

        Debug.Log("🍲 All ingredients added! Spawning ramen pot...");

        // Spawn cooked pot ABOVE empty pot — but DO NOT destroy the empty pot
        Instantiate(
            ramenPotPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}