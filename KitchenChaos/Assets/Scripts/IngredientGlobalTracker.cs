using UnityEngine;
using System.Collections.Generic;

public class IngredientGlobalTracker : MonoBehaviour
{
    public static IngredientGlobalTracker Instance;

    [System.Serializable]
    public class IngredientLimit
    {
        public string ingredientName;  
        public int maxCount = 5;
        [HideInInspector] public int currentCount = 0;
    }

    [Header("Global Ingredient Limits")]
    public IngredientLimit[] ingredientLimits;

    private Dictionary<string, IngredientLimit> limitLookup;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        limitLookup = new Dictionary<string, IngredientLimit>();
        foreach (var limit in ingredientLimits)
        {
            if (!limitLookup.ContainsKey(limit.ingredientName))
                limitLookup.Add(limit.ingredientName, limit);
        }
    }

    public bool CanSpawn(string ingredientName)
    {
        if (!limitLookup.ContainsKey(ingredientName))
            return true; // If not registered, unlimited

        return limitLookup[ingredientName].currentCount < limitLookup[ingredientName].maxCount;
    }

    public void Register(string ingredientName)
    {
        if (!limitLookup.ContainsKey(ingredientName))
            return;

        limitLookup[ingredientName].currentCount++;
    }

    public void Unregister(string ingredientName)
    {
        if (!limitLookup.ContainsKey(ingredientName))
            return;

        limitLookup[ingredientName].currentCount--;
        if (limitLookup[ingredientName].currentCount < 0)
            limitLookup[ingredientName].currentCount = 0;
    }

    public int GetCurrentCount(string ingredientName)
    {
        return limitLookup.ContainsKey(ingredientName)
            ? limitLookup[ingredientName].currentCount : 0;
    }
}