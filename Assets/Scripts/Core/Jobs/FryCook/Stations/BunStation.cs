using UnityEngine;

/// <summary>
/// Bun Station - add buns to burgers with proper order enforcement
/// Bottom bun MUST exist before top bun can be added
/// </summary>
public class BunStation : MonoBehaviour, IInteractable
{
    [Header("Prefabs")]
    public GameObject bottomBunPrefab;
    public GameObject topBunPrefab;

    [Header("Inventory")]
    public int bunCount = 999; // Unlimited buns

    public string GetPromptText()
    {
        // Must be holding a plate
        if (!PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            return "Bun Station (Need Plate)";
        }

        BurgerPlate plate = PlayerHands.Instance.GetHeldItem<BurgerPlate>();

        // Empty plate → Add bottom bun first
        if (plate.layers.Count == 0)
        {
            return "E to Add Bottom Bun";
        }

        // Check what's in the burger
        bool hasBottomBun = HasBottomBun(plate);
        bool hasPatty = HasPatty(plate);
        bool hasTopBun = HasTopBun(plate);
        GameObject topLayer = plate.layers[plate.layers.Count - 1];

        // Already complete
        if (hasTopBun)
        {
            return "Burger Complete!";
        }

        // Has bottom bun AND patty → Can add top bun
        if (hasBottomBun && hasPatty)
        {
            return "E to Add Top Bun";
        }

        // Has bottom bun but no patty → Need patty
        if (hasBottomBun && !hasPatty)
        {
            return "Add Patty First (Plate Station)";
        }

        // Has patty but no bottom bun → Add bottom bun
        if (hasPatty && !hasBottomBun)
        {
            return "E to Add Bottom Bun";
        }

        // Nothing recognized → Add bottom bun
        return "E to Add Bottom Bun";
    }

    public void Interact()
    {
        // Must be holding a plate
        if (!PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            Debug.LogWarning("⚠️ You need to hold a plate!");
            return;
        }

        BurgerPlate plate = PlayerHands.Instance.GetHeldItem<BurgerPlate>();

        // Check what's in the burger
        bool hasBottomBun = HasBottomBun(plate);
        bool hasPatty = HasPatty(plate);
        bool hasTopBun = HasTopBun(plate);

        // Already complete
        if (hasTopBun)
        {
            Debug.Log("✅ Burger is already complete!");
            return;
        }

        // Has bottom bun AND patty → Add top bun
        if (hasBottomBun && hasPatty)
        {
            AddTopBun(plate);
            Debug.Log("🍞 Added top bun! Burger complete! 🍔");
            return;
        }

        // Has bottom bun but no patty → Need patty
        if (hasBottomBun && !hasPatty)
        {
            Debug.LogWarning("⚠️ Add a patty first! Use the Plate Station.");
            return;
        }

        // Missing bottom bun (might have patty or not) → Add bottom bun
        if (!hasBottomBun)
        {
            AddBottomBun(plate);
            if (hasPatty)
            {
                Debug.Log("🍞 Added bottom bun! (Next: add top bun)");
            }
            else
            {
                Debug.Log("🍞 Added bottom bun! (Next: add patty)");
            }
            return;
        }

        Debug.LogWarning("⚠️ Unclear burger state!");
    }

    bool HasBottomBun(BurgerPlate plate)
    {
        foreach (GameObject layer in plate.layers)
        {
            if (layer.GetComponent<BottomBun>() != null)
            {
                return true;
            }
        }
        return false;
    }

    bool HasPatty(BurgerPlate plate)
    {
        foreach (GameObject layer in plate.layers)
        {
            if (layer.GetComponent<FoodItem>() != null)
            {
                return true;
            }
        }
        return false;
    }

    bool HasTopBun(BurgerPlate plate)
    {
        foreach (GameObject layer in plate.layers)
        {
            if (layer.GetComponent<TopBun>() != null)
            {
                return true;
            }
        }
        return false;
    }

    void AddBottomBun(BurgerPlate plate)
    {
        if (bottomBunPrefab == null)
        {
            Debug.LogError("❌ Bottom Bun Prefab not assigned!");
            return;
        }

        GameObject bottomBun = Instantiate(bottomBunPrefab);
        plate.TryAddLayer(bottomBun);
        Debug.Log("🍞 Added bottom bun!");
    }

    void AddTopBun(BurgerPlate plate)
    {
        if (topBunPrefab == null)
        {
            Debug.LogError("❌ Top Bun Prefab not assigned!");
            return;
        }

        GameObject topBun = Instantiate(topBunPrefab);
        plate.TryAddLayer(topBun);
        Debug.Log("🍞 Added top bun!");
    }
}