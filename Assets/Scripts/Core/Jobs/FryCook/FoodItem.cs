using UnityEngine;

/// <summary>
/// Cooking states for food items
/// </summary>
public enum CookingState
{
    Raw,
    Cooking,
    Cooked,
    Burnt,
    Prepared  // Smashed/chopped/ready to cook
}

/// <summary>
/// Base class for all food items (meat, fries, etc)
/// </summary>
public class FoodItem : MonoBehaviour, IHoldable
{
    [Header("Food Info")]
    public string foodName = "Food";
    public CookingState currentState = CookingState.Raw;

    [Header("Cooking Times")]
    public float cookTime = 5f;           // Time to go from raw to cooked
    public float burnTime = 3f;           // Time from cooked to burnt
    public float flipBonus = 3f;          // Extra burn protection from perfect flip

    [Header("Current Cooking")]
    public float currentCookTimer = 0f;
    public bool isOnGrill = false;
    public bool hasBeenFlipped = false;
    public float flipBonusTimer = 0f;

    [Header("Components")]
    private Rigidbody rb;
    private Collider col;
    private Transform originalParent;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Cook if on grill
        if (isOnGrill && currentState != CookingState.Burnt)
        {
            UpdateCooking();
        }

        // Countdown flip bonus
        if (flipBonusTimer > 0f)
        {
            flipBonusTimer -= Time.deltaTime;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  COOKING LOGIC
    // ═══════════════════════════════════════════════════════════════

    void UpdateCooking()
    {
        currentCookTimer += Time.deltaTime;

        // Update cooking state based on timer
        if (currentState == CookingState.Prepared || currentState == CookingState.Raw)
        {
            if (currentCookTimer < cookTime)
            {
                currentState = CookingState.Cooking;
            }
            else
            {
                currentState = CookingState.Cooked;
                currentCookTimer = 0f; // Reset for burn timer
            }
        }
        else if (currentState == CookingState.Cooked)
        {
            // Check if we should burn (with flip bonus protection)
            float effectiveBurnTime = burnTime + flipBonusTimer;

            if (currentCookTimer >= effectiveBurnTime)
            {
                currentState = CookingState.Burnt;
                Debug.Log($"🔥 {foodName} BURNT!");
            }
        }
    }

    public void StartCooking()
    {
        isOnGrill = true;
        currentCookTimer = 0f;
        Debug.Log($"🍳 Started cooking {foodName}");
    }

    public void StopCooking()
    {
        isOnGrill = false;
        Debug.Log($"⏸️ Stopped cooking {foodName}");
    }

    public void ApplyFlipBonus()
    {
        hasBeenFlipped = true;
        flipBonusTimer = flipBonus;

        // Speed up cooking slightly
        currentCookTimer += 1f;

        Debug.Log($"✨ Perfect flip! {flipBonus}s burn protection");
    }

    // ═══════════════════════════════════════════════════════════════
    //  IHOLDABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public bool CanPickup()
    {
        // Can always pick up food
        return true;
    }

    public void OnPickup(Transform handPosition)
    {
        originalParent = transform.parent;

        // Parent to hand
        transform.SetParent(handPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;

        // Disable physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (col != null)
        {
            col.isTrigger = true;
        }

        // Stop cooking when picked up
        StopCooking();
    }

    public void OnDrop()
    {
        // Unparent
        transform.SetParent(originalParent);

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    public void OnPlaceAt(Transform targetPosition)
    {
        // Place at target
        transform.SetParent(targetPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Keep physics disabled when placed on station
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public string GetItemName()
    {
        return foodName;
    }

    // ═══════════════════════════════════════════════════════════════
    //  STATE QUERIES
    // ═══════════════════════════════════════════════════════════════

    public float GetCookProgress()
    {
        if (currentState == CookingState.Cooking || currentState == CookingState.Raw)
            return Mathf.Clamp01(currentCookTimer / cookTime);

        return 1f;
    }

    public float GetBurnProgress()
    {
        if (currentState == CookingState.Cooked)
        {
            float effectiveBurnTime = burnTime + flipBonusTimer;
            return Mathf.Clamp01(currentCookTimer / effectiveBurnTime);
        }

        return 0f;
    }

    public bool IsEdible()
    {
        return currentState == CookingState.Cooked;
    }

    public bool IsBurnt()
    {
        return currentState == CookingState.Burnt;
    }
}