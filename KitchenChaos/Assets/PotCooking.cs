using System.Collections.Generic;
using UnityEngine;

public class PotCooking : MonoBehaviour
{
    [Header("References")]
    public Transform ingredientDropPoint;         // Assign in inspector
    public GameObject finishedStewPrefab;         // Assign in inspector

    [Header("Recipe Requirements")]
    public bool needNoodles = true;
    public bool needHamSlice = true;
    public bool needOnionRings = true;
    public bool needRamenBroth = true;

    // Tracking what has been added
    private bool hasNoodles = false;
    private bool hasHamSlice = false;
    private bool hasOnionRings = false;
    private bool hasRamenBroth = false;

    private List<GameObject> ingredientsInPot = new List<GameObject>();

    public void AddIngredient(GameObject ingredient)
    {
        // Move ingredient into pot
        ingredient.transform.SetParent(ingredientDropPoint);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localRotation = Quaternion.identity;

        ingredientsInPot.Add(ingredient);

        // auto-detect by name
        string name = ingredient.name.ToLower();

        if (name.Contains("noodle"))
            hasNoodles = true;

        if (name.Contains("ham"))
            hasHamSlice = true;

        if (name.Contains("onion"))
            hasOnionRings = true;

        if (name.Contains("broth"))
            hasRamenBroth = true;

        CheckRecipe();
    }

    void CheckRecipe()
    {
        if (needNoodles && !hasNoodles) return;
        if (needHamSlice && !hasHamSlice) return;
        if (needOnionRings && !hasOnionRings) return;
        if (needRamenBroth && !hasRamenBroth) return;

        CompleteRecipe();
    }

    void CompleteRecipe()
    {
        Debug.Log("Stew completed!");

        // remove ingredients
        foreach (GameObject g in ingredientsInPot)
            Destroy(g);

        // spawn completed stew
        Instantiate(finishedStewPrefab, transform.position, transform.rotation);

        // remove raw pot
        Destroy(gameObject);
    }
}