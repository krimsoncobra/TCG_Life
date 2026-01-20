using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cooking pan/skillet that holds food and can be placed on grill
/// </summary>
public class CookingPan : MonoBehaviour, IHoldable
{
    [Header("Pickup Settings")]
    [Tooltip("Offset position when held in hand")]
    public Vector3 handOffset = Vector3.zero;

    [Tooltip("Rotation when held in hand")]
    public Vector3 handRotation = Vector3.zero;

    [Tooltip("Scale when held")]
    public Vector3 handScale = Vector3.one;

    [Header("Pan Settings")]
    public Transform foodPosition;  // Where food sits in pan

    [Header("Current State")]
    public FoodItem currentFood;    // What's in the pan
    public bool isOnGrill = false;

    [Header("Flip Minigame")]
    public KeyCode flipKey = KeyCode.F;
    private InputAction flipAction;

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;

        // Setup flip input
        flipAction = new InputAction("FlipFood", InputActionType.Button);
        flipAction.AddBinding($"<Keyboard>/{flipKey.ToString().ToLower()}");
        flipAction.performed += ctx => TryStartFlipMinigame();

        // Create food position if not assigned
        if (foodPosition == null)
        {
            GameObject foodPos = new GameObject("FoodPosition");
            foodPos.transform.SetParent(transform);
            foodPos.transform.localPosition = new Vector3(0, 0.05f, 0); // Slightly above pan
            foodPos.transform.localRotation = Quaternion.identity;
            foodPosition = foodPos.transform;
        }
    }

    void OnEnable() => flipAction?.Enable();
    void OnDisable() => flipAction?.Disable();

    void Update()
    {
        // Update food cooking state if on grill
        if (isOnGrill && currentFood != null)
        {
            if (!currentFood.isOnGrill)
            {
                currentFood.StartCooking();
            }
        }
        else if (currentFood != null && currentFood.isOnGrill)
        {
            currentFood.StopCooking();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  FOOD MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Try to add food to the pan
    /// </summary>
    public bool TryAddFood(FoodItem food)
    {
        if (currentFood != null)
        {
            Debug.LogWarning("⚠️ Pan already has food!");
            return false;
        }

        currentFood = food;

        // Parent food to pan
        food.transform.SetParent(foodPosition);
        food.transform.localPosition = Vector3.zero;
        food.transform.localRotation = Quaternion.identity;

        Debug.Log($"✅ Added {food.GetItemName()} to pan");

        return true;
    }

    /// <summary>
    /// Remove food from pan (if cooked/burnt)
    /// </summary>
    public FoodItem RemoveFood()
    {
        if (currentFood == null) return null;

        FoodItem food = currentFood;
        currentFood = null;

        // Unparent food
        food.transform.SetParent(null);

        Debug.Log($"📤 Removed {food.GetItemName()} from pan");

        return food;
    }

    /// <summary>
    /// Check if pan has food and player is holding it
    /// </summary>
    public bool CanFlip()
    {
        return currentFood != null &&
               PlayerHands.Instance != null &&
               PlayerHands.Instance.IsHolding<CookingPan>();
    }

    void TryStartFlipMinigame()
    {
        if (!CanFlip()) return;

        // Only flip if food is cooking
        if (currentFood.currentState != CookingState.Cooking &&
            currentFood.currentState != CookingState.Cooked)
        {
            Debug.LogWarning("⚠️ Food isn't ready to flip yet!");
            return;
        }

        // Start minigame
        if (BurgerFlipMinigame.Instance != null)
        {
            BurgerFlipMinigame.Instance.StartMinigame(currentFood);
        }
        else
        {
            Debug.LogError("❌ BurgerFlipMinigame not found!");
        }
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
            col.isTrigger = true;

        // Stop cooking when picked up
        isOnGrill = false;
    }

    public void OnDrop()
    {
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (col != null)
            col.isTrigger = false;

        isOnGrill = false;
    }

    public void OnPlaceAt(Transform targetPosition)
    {
        transform.SetParent(targetPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
            rb.isKinematic = true;

        // If placed on grill, start cooking
        Grill grill = targetPosition.GetComponentInParent<Grill>();
        if (grill != null)
        {
            isOnGrill = true;
            Debug.Log("🔥 Pan placed on grill - cooking started!");
        }
    }

    public string GetItemName()
    {
        if (currentFood != null)
            return $"Pan ({currentFood.GetItemName()})";

        return "Empty Pan";
    }
}