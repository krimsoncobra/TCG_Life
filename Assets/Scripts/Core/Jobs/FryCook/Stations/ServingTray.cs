using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Serving Tray - Holds up to 3 burgers and 3 fries
/// Positions items in a grid layout
/// </summary>
public class ServingTray : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Tray Capacity")]
    public int maxBurgers = 3;
    public int maxFries = 3;

    [Header("Positions")]
    [Tooltip("Grid positions for burgers (create 3 empty children)")]
    public Transform[] burgerPositions = new Transform[3];

    [Tooltip("Grid positions for fries (create 3 empty children)")]
    public Transform[] friesPositions = new Transform[3];

    [Header("Pickup Settings")]
    public Vector3 handOffset = new Vector3(0, -0.1f, 0.3f);
    public Vector3 handRotation = Vector3.zero;
    public Vector3 handScale = Vector3.one;

    [Header("Current Contents")]
    public List<GameObject> burgers = new List<GameObject>();
    public List<GameObject> fries = new List<GameObject>();

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;

        // Auto-create positions if not assigned
        SetupPositions();
    }

    void SetupPositions()
    {
        // Create burger positions if needed
        for (int i = 0; i < maxBurgers; i++)
        {
            if (burgerPositions[i] == null)
            {
                GameObject posObj = new GameObject($"BurgerPos_{i}");
                posObj.transform.SetParent(transform);
                // Arrange in a row: left, center, right
                float x = -0.15f + (i * 0.15f);
                posObj.transform.localPosition = new Vector3(x, 0.05f, 0.1f);
                burgerPositions[i] = posObj.transform;
            }
        }

        // Create fries positions if needed
        for (int i = 0; i < maxFries; i++)
        {
            if (friesPositions[i] == null)
            {
                GameObject posObj = new GameObject($"FriesPos_{i}");
                posObj.transform.SetParent(transform);
                // Arrange in a row: left, center, right
                float x = -0.15f + (i * 0.15f);
                posObj.transform.localPosition = new Vector3(x, 0.05f, -0.1f);
                friesPositions[i] = posObj.transform;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ADD/REMOVE ITEMS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Try to add burger (plate or loose patty) to tray
    /// </summary>
    public bool TryAddBurger(GameObject burger)
    {
        if (burgers.Count >= maxBurgers)
        {
            Debug.LogWarning($"Tray full! Already has {maxBurgers} burgers.");
            return false;
        }

        // Check if it's a valid burger item
        BurgerPlate plate = burger.GetComponent<BurgerPlate>();
        FoodItem food = burger.GetComponent<FoodItem>();

        if (plate == null && food == null)
        {
            Debug.LogWarning("Not a valid burger item!");
            return false;
        }

        // Don't accept raw or burnt burgers (optional - remove if you want to allow)
        if (food != null)
        {
            if (food.currentState == CookingState.Raw)
            {
                Debug.LogWarning("Can't serve raw burger!");
                return false;
            }
        }

        // Add to next available position
        int slotIndex = burgers.Count;
        burgers.Add(burger);

        burger.transform.SetParent(burgerPositions[slotIndex]);
        burger.transform.localPosition = Vector3.zero;
        burger.transform.localRotation = Quaternion.identity;
        burger.transform.localScale = Vector3.one * 0.6f; // Scale down to fit

        // Disable physics
        DisablePhysics(burger);

        Debug.Log($"✅ Added burger to tray! ({burgers.Count}/{maxBurgers})");
        return true;
    }

    /// <summary>
    /// Try to add fries to tray
    /// </summary>
    public bool TryAddFries(GameObject friesItem)
    {
        if (fries.Count >= maxFries)
        {
            Debug.LogWarning($"Tray full! Already has {maxFries} fries.");
            return false;
        }

        // Check if it's cooked fries
        CookedFries cookedFries = friesItem.GetComponent<CookedFries>();
        if (cookedFries == null)
        {
            Debug.LogWarning("Only cooked fries can go on tray!");
            return false;
        }

        // Optional: Don't accept burnt fries (remove if you want to allow)
        if (cookedFries.isBurnt)
        {
            Debug.LogWarning("Can't serve burnt fries!");
            return false;
        }

        // Add to next available position
        int slotIndex = fries.Count;
        fries.Add(friesItem);

        friesItem.transform.SetParent(friesPositions[slotIndex]);
        friesItem.transform.localPosition = Vector3.zero;
        friesItem.transform.localRotation = Quaternion.identity;
        friesItem.transform.localScale = Vector3.one * 0.6f; // Scale down to fit

        // Disable physics
        DisablePhysics(friesItem);

        Debug.Log($"✅ Added fries to tray! ({fries.Count}/{maxFries})");
        return true;
    }

    /// <summary>
    /// Remove last burger from tray
    /// </summary>
    public GameObject RemoveBurger()
    {
        if (burgers.Count == 0) return null;

        int lastIndex = burgers.Count - 1;
        GameObject burger = burgers[lastIndex];
        burgers.RemoveAt(lastIndex);

        burger.transform.SetParent(null);
        burger.transform.localScale = Vector3.one;

        // Re-enable physics
        EnablePhysics(burger);

        Debug.Log($"Removed burger from tray ({burgers.Count}/{maxBurgers} remaining)");
        return burger;
    }

    /// <summary>
    /// Remove last fries from tray
    /// </summary>
    public GameObject RemoveFries()
    {
        if (fries.Count == 0) return null;

        int lastIndex = fries.Count - 1;
        GameObject friesItem = fries[lastIndex];
        fries.RemoveAt(lastIndex);

        friesItem.transform.SetParent(null);
        friesItem.transform.localScale = Vector3.one;

        // Re-enable physics
        EnablePhysics(friesItem);

        Debug.Log($"Removed fries from tray ({fries.Count}/{maxFries} remaining)");
        return friesItem;
    }

    /// <summary>
    /// Clear all items from tray (destroys them)
    /// </summary>
    public void ClearTray()
    {
        foreach (GameObject burger in burgers)
        {
            if (burger != null)
                Destroy(burger);
        }
        burgers.Clear();

        foreach (GameObject friesItem in fries)
        {
            if (friesItem != null)
                Destroy(friesItem);
        }
        fries.Clear();

        Debug.Log("🗑️ Cleared tray");
    }

    /// <summary>
    /// Get number of burgers on tray
    /// </summary>
    public int GetBurgerCount()
    {
        return burgers.Count;
    }

    /// <summary>
    /// Get number of fries on tray
    /// </summary>
    public int GetFriesCount()
    {
        return fries.Count;
    }

    void DisablePhysics(GameObject obj)
    {
        Rigidbody objRb = obj.GetComponent<Rigidbody>();
        if (objRb != null)
        {
            objRb.isKinematic = true;
            objRb.useGravity = false;
        }

        Collider objCol = obj.GetComponent<Collider>();
        if (objCol != null)
        {
            objCol.enabled = false;
        }
    }

    void EnablePhysics(GameObject obj)
    {
        Rigidbody objRb = obj.GetComponent<Rigidbody>();
        if (objRb != null)
        {
            objRb.isKinematic = false;
            objRb.useGravity = true;
        }

        Collider objCol = obj.GetComponent<Collider>();
        if (objCol != null)
        {
            objCol.enabled = true;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  IHOLDABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public bool CanPickup() => true;

    public void OnPickup(Transform handPosition)
    {
        transform.SetParent(handPosition);
        transform.localPosition = handOffset;
        transform.localRotation = Quaternion.Euler(handRotation);
        transform.localScale = Vector3.Scale(originalScale, handScale);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (col != null)
            col.isTrigger = true;
    }

    public void OnDrop()
    {
        transform.SetParent(null);
        transform.localScale = originalScale;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (col != null)
            col.isTrigger = false;
    }

    public void OnPlaceAt(Transform targetPosition)
    {
        transform.SetParent(targetPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;
    }

    public string GetItemName()
    {
        if (burgers.Count > 0 && fries.Count > 0)
            return $"Tray ({burgers.Count}x Burger, {fries.Count}x Fries)";
        else if (burgers.Count > 0)
            return $"Tray ({burgers.Count}x Burger)";
        else if (fries.Count > 0)
            return $"Tray ({fries.Count}x Fries)";
        else
            return "Empty Tray";
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        if (PlayerHands.Instance == null) return "";

        // Holding burger plate -> add to tray
        if (PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            if (burgers.Count < maxBurgers)
                return $"E to Add Burger to Tray ({burgers.Count}/{maxBurgers})";
            else
                return $"Tray Full! ({maxBurgers} Burgers)";
        }

        // Holding cooked fries -> add to tray
        if (PlayerHands.Instance.IsHolding<CookedFries>())
        {
            if (fries.Count < maxFries)
                return $"E to Add Fries to Tray ({fries.Count}/{maxFries})";
            else
                return $"Tray Full! ({maxFries} Fries)";
        }

        // Holding loose food item (patty, etc.) -> add to tray
        if (PlayerHands.Instance.IsHolding<FoodItem>())
        {
            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();

            // Don't accept uncooked fries
            if (food is UncookedFries)
            {
                return "Use Cooked Fries from Fry Station";
            }

            if (burgers.Count < maxBurgers)
                return $"E to Add Food to Tray ({burgers.Count}/{maxBurgers})";
            else
                return $"Tray Full! ({maxBurgers} Burgers)";
        }

        // Empty hands -> pick up tray
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Pick Up {GetItemName()}";
        }

        return "";
    }

    public void Interact()
    {
        if (PlayerHands.Instance == null) return;

        // Case 1: Holding burger plate -> add to tray
        if (PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            if (burgers.Count < maxBurgers)
            {
                GameObject plateObj = PlayerHands.Instance.currentItem;
                PlayerHands.Instance.currentItem = null;

                bool added = TryAddBurger(plateObj);

                if (added)
                {
                    // Pick up tray
                    PlayerHands.Instance.TryPickup(gameObject);
                }
                return;
            }
        }

        // Case 2: Holding cooked fries -> add to tray
        if (PlayerHands.Instance.IsHolding<CookedFries>())
        {
            if (fries.Count < maxFries)
            {
                GameObject friesObj = PlayerHands.Instance.currentItem;
                PlayerHands.Instance.currentItem = null;

                bool added = TryAddFries(friesObj);

                if (added)
                {
                    // Pick up tray
                    PlayerHands.Instance.TryPickup(gameObject);
                }
                return;
            }
        }

        // Case 3: Holding loose food -> add to tray
        if (PlayerHands.Instance.IsHolding<FoodItem>())
        {
            if (burgers.Count < maxBurgers)
            {
                GameObject foodObj = PlayerHands.Instance.currentItem;
                PlayerHands.Instance.currentItem = null;

                bool added = TryAddBurger(foodObj);

                if (added)
                {
                    // Pick up tray
                    PlayerHands.Instance.TryPickup(gameObject);
                }
                return;
            }
        }

        // Case 4: Empty hands -> pick up tray
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            PlayerHands.Instance.TryPickup(gameObject);
        }
    }
}