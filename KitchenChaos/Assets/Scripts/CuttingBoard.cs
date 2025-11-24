using UnityEngine;

public class CuttingBoard : MonoBehaviour
{
    [Header("Highlight")]
    public Color highlightColor = new Color(1f, 0.7f, 0.2f);
    public float highlightIntensity = 2f;

    [Header("Cutting Setup")]
    public Transform boardPoint;    // where ingredients sit on the board
    public float interactRange = 2f;
    public KeyCode interactKey = KeyCode.E;

    private Renderer[] renderers;
    private Material[] originalMats;
    private Material[] highlightMats;

    private IngredientFlee ingredientOnBoard;
    private bool isHighlighted = false;

    private Transform player;
    private IngredientInteraction playerInteraction;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerInteraction = player.GetComponent<IngredientInteraction>();

        SetupHighlight();
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        bool playerHolding = playerInteraction != null && playerInteraction.IsCarryingIngredient();

        bool playerNear = dist < interactRange;

        // Highlight rules:
        // 1. Player is near AND holding ingredient
        // 2. OR player is near AND board has ingredient (for cutting)
        bool shouldHighlight = playerNear && (playerHolding || ingredientOnBoard != null);

        SetHighlight(shouldHighlight);

        // E pressed?
        if (playerNear && Input.GetKeyDown(interactKey))
        {
            if (playerHolding)
            {
                TryPlaceIngredient();
            }
            else if (ingredientOnBoard != null)
            {
                TryCutIngredient();
            }
        }
    }

    void SetupHighlight()
    {
        renderers = GetComponentsInChildren<Renderer>();

        originalMats = renderers[0].materials;

        highlightMats = new Material[originalMats.Length];
        for (int i = 0; i < originalMats.Length; i++)
        {
            highlightMats[i] = new Material(originalMats[i]);
            highlightMats[i].EnableKeyword("_EMISSION");
            highlightMats[i].SetColor("_EmissionColor", highlightColor * highlightIntensity);
        }
    }

    void SetHighlight(bool on)
    {
        if (isHighlighted == on) return;
        isHighlighted = on;

        foreach (Renderer r in renderers)
        {
            r.materials = on ? highlightMats : originalMats;
        }
    }

    void TryPlaceIngredient()
    {
        IngredientFlee held = playerInteraction.GetHeldIngredient();
        if (held == null) return;

        // Place ingredient on board
        ingredientOnBoard = held;
        held.PlaceOnBoard(boardPoint);

        // Remove from player interaction
        playerInteraction.PlaceIngredientOnPlate(boardPoint);

        Debug.Log($"Placed {held.name} on cutting board.");
    }

    void TryCutIngredient()
    {
        if (ingredientOnBoard == null) return;

        IngredientCutData cutData = ingredientOnBoard.GetComponent<IngredientCutData>();
        if (cutData == null || cutData.cutPrefab == null)
        {
            Debug.LogWarning("This ingredient has no cut version assigned!");
            return;
        }

        // Remove old ingredient
        Destroy(ingredientOnBoard.gameObject);

        // Spawn cut version
        GameObject cutObj = Instantiate(cutData.cutPrefab, boardPoint.position, boardPoint.rotation);

        // Cut version should not move
        Rigidbody rb = cutObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        ingredientOnBoard = null;

        Debug.Log($"Cut ingredient into {cutObj.name}");
    }
}