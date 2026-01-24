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
public class FoodItem : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Food Info")]
    public string foodName = "Food";
    public CookingState currentState = CookingState.Raw;

    [Header("Cooking Times")]
    public float cookTime = 5f;
    public float burnTime = 3f;
    public float flipBonus = 3f;

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

        // Transition from Prepared/Raw to Cooking
        if (currentState == CookingState.Prepared || currentState == CookingState.Raw)
        {
            currentState = CookingState.Cooking;
        }

        // Transition from Cooking to Cooked
        if (currentState == CookingState.Cooking)
        {
            if (currentCookTimer >= cookTime)
            {
                currentState = CookingState.Cooked;
                currentCookTimer = 0f;
                Debug.Log("🍔 Food is now COOKED!");
            }
        }
        // Transition from Cooked to Burnt
        else if (currentState == CookingState.Cooked)
        {
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
        currentCookTimer += 1f;
        Debug.Log($"✨ Perfect flip! {flipBonus}s burn protection");
    }

    // ═══════════════════════════════════════════════════════════════
    //  IHOLDABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public bool CanPickup()
    {
        return true;
    }

    public void OnPickup(Transform handPosition)
    {
        originalParent = transform.parent;

        transform.SetParent(handPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (col != null)
        {
            col.isTrigger = true;
        }

        StopCooking();
    }

    public void OnDrop()
    {
        transform.SetParent(originalParent);

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
        transform.SetParent(targetPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

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

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHoldingSomething())
            return "";

        return $"E to Pick Up {GetItemName()}";
    }

    public void Interact()
    {
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            PlayerHands.Instance.TryPickup(gameObject);
        }
    }
}