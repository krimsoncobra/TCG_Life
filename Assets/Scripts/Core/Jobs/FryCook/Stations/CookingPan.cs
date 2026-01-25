using UnityEngine;
using UnityEngine.InputSystem;

public class CookingPan : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Pan Settings")]
    public Transform foodPosition;

    [Header("Pickup Settings")]
    public Vector3 handOffset = new Vector3(0, -0.1f, 0.3f);
    public Vector3 handRotation = new Vector3(0, 0, 0);
    public Vector3 handScale = Vector3.one;

    [Header("Grill Placement Settings")]
    public Vector3 grillPlacementOffset = Vector3.zero;
    public Vector3 grillPlacementRotation = Vector3.zero;

    [Header("Current State")]
    public FoodItem currentFood;
    public bool isOnGrill = false;

    // Note: F key is now handled in PlayerInteract.cs, not here

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;

        if (foodPosition == null)
        {
            GameObject foodPos = new GameObject("FoodPosition");
            foodPos.transform.SetParent(transform);
            foodPos.transform.localPosition = new Vector3(0, 0.05f, 0);
            foodPos.transform.localRotation = Quaternion.identity;
            foodPosition = foodPos.transform;
        }
    }

    void Update()
    {
        // Handle cooking state
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

        // Show/hide flip prompt based on conditions
        UpdateFlipPrompt();
    }

    void UpdateFlipPrompt()
    {
        // Show prompt when LOOKING at pan on grill with cooking food
        // (not when holding it - that doesn't make sense!)

        bool hasFood = currentFood != null;
        bool onGrill = isOnGrill;
        bool isCooking = hasFood && currentFood.currentState == CookingState.Cooking;
        bool notFlipped = hasFood && !currentFood.hasBeenFlipped;
        bool notHolding = PlayerHands.Instance == null || !PlayerHands.Instance.IsHoldingSomething();

        bool shouldShowPrompt =
            hasFood &&
            onGrill &&
            isCooking &&
            notFlipped &&
            notHolding; // Only show when NOT holding anything

        // Debug logging (only log when state changes)
        if (shouldShowPrompt != lastPromptState)
        {
            lastPromptState = shouldShowPrompt;

            if (shouldShowPrompt)
            {
                Debug.Log("🟢 Flip prompt should SHOW (looking at cooking pan on grill)");
            }
            else
            {
                Debug.Log($"🔴 Flip prompt should HIDE | " +
                    $"HasFood:{hasFood} OnGrill:{onGrill} Cooking:{isCooking} " +
                    $"NotFlipped:{notFlipped} NotHolding:{notHolding}");
            }
        }

        // Update prompt visibility
        if (FlipPromptUI.Instance != null)
        {
            if (shouldShowPrompt)
            {
                FlipPromptUI.Instance.ShowPrompt();
            }
            else
            {
                FlipPromptUI.Instance.HidePrompt();
            }
        }
    }

    private bool lastPromptState = false;

    public bool TryAddFood(FoodItem food)
    {
        if (currentFood != null)
        {
            Debug.LogWarning("Pan already has food!");
            return false;
        }

        currentFood = food;
        food.transform.SetParent(foodPosition);
        food.transform.localPosition = Vector3.zero;
        food.transform.localRotation = Quaternion.identity;
        food.transform.localScale = Vector3.one;

        Collider foodCol = food.GetComponent<Collider>();
        if (foodCol != null)
        {
            foodCol.enabled = false;
        }

        Rigidbody foodRb = food.GetComponent<Rigidbody>();
        if (foodRb != null)
        {
            foodRb.isKinematic = true;
            foodRb.useGravity = false;
        }

        Debug.Log($"Added {food.GetItemName()} to pan");
        return true;
    }

    public FoodItem RemoveFood()
    {
        if (currentFood == null) return null;

        FoodItem food = currentFood;
        currentFood = null;
        food.transform.SetParent(null);

        Collider foodCol = food.GetComponent<Collider>();
        if (foodCol != null)
        {
            foodCol.enabled = true;
        }

        Debug.Log($"Removed {food.GetItemName()} from pan");
        return food;
    }

    public bool CanFlip()
    {
        return currentFood != null &&
               isOnGrill &&
               !currentFood.hasBeenFlipped && // Can only flip once
               currentFood.currentState == CookingState.Cooking && // ONLY during cooking (0-99%)
               PlayerHands.Instance != null &&
               !PlayerHands.Instance.IsHoldingSomething(); // NOT holding anything
    }

    public void TryStartFlipMinigame()
    {
        Debug.Log("🔍 TryStartFlipMinigame called!");

        if (!CanFlip())
        {
            Debug.LogWarning($"⚠️ Cannot flip! " +
                $"Food: {(currentFood != null ? currentFood.name : "NULL")}, " +
                $"IsOnGrill: {isOnGrill}, " +
                $"State: {(currentFood != null ? currentFood.currentState.ToString() : "NULL")}, " +
                $"HasBeenFlipped: {(currentFood != null ? currentFood.hasBeenFlipped : false)}, " +
                $"PlayerHands: {(PlayerHands.Instance != null ? "EXISTS" : "NULL")}, " +
                $"IsHoldingSomething: {(PlayerHands.Instance != null ? PlayerHands.Instance.IsHoldingSomething() : false)}");
            return;
        }

        // Double-check state (shouldn't be raw or burnt)
        if (currentFood.currentState == CookingState.Raw)
        {
            Debug.LogWarning("Food is still raw! Wait for it to start cooking.");
            return;
        }

        if (currentFood.currentState == CookingState.Burnt)
        {
            Debug.LogWarning("Food is burnt! Too late to flip.");
            return;
        }

        if (currentFood.hasBeenFlipped)
        {
            Debug.LogWarning("Already flipped this burger!");
            return;
        }

        // Start the minigame!
        Debug.Log("✅ All checks passed! Starting minigame...");

        if (BurgerFlipMinigame.Instance != null)
        {
            Debug.Log("✅ BurgerFlipMinigame.Instance found!");
            BurgerFlipMinigame.Instance.StartMinigame(currentFood);
        }
        else
        {
            Debug.LogError("❌ BurgerFlipMinigame.Instance is NULL!");
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

        isOnGrill = false;

        // Unregister from UI when picked up
        if (CookingProgressUI.Instance != null)
        {
            CookingProgressUI.Instance.UnregisterCookingPan(this);
        }
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

        // Hide flip prompt when dropping pan
        if (FlipPromptUI.Instance != null)
        {
            FlipPromptUI.Instance.HidePrompt();
        }
    }

    public void OnPlaceAt(Transform targetPosition)
    {
        transform.SetParent(targetPosition);
        transform.localPosition = grillPlacementOffset;
        transform.localRotation = Quaternion.Euler(grillPlacementRotation);
        transform.localScale = originalScale;

        if (rb != null)
            rb.isKinematic = true;

        Grill grill = targetPosition.GetComponentInParent<Grill>();

        if (grill != null)
        {
            isOnGrill = true;

            if (CookingProgressUI.Instance != null && currentFood != null)
            {
                CookingProgressUI.Instance.RegisterCookingPan(this);
            }
        }
    }

    public string GetItemName()
    {
        if (currentFood != null)
            return $"Pan ({currentFood.GetItemName()})";

        return "Empty Pan";
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        // Priority 1: Show flip prompt if cooking and not flipped yet
        if (currentFood != null &&
            isOnGrill &&
            currentFood.currentState == CookingState.Cooking &&
            !currentFood.hasBeenFlipped &&
            PlayerHands.Instance != null &&
            !PlayerHands.Instance.IsHoldingSomething())
        {
            return "F to Flip Burger!";
        }

        // Holding plate with food -> transfer to pan
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            BurgerPlate plate = PlayerHands.Instance.GetHeldItem<BurgerPlate>();
            if (plate.layers.Count > 0)
            {
                GameObject topLayer = plate.layers[plate.layers.Count - 1];
                FoodItem food = topLayer.GetComponent<FoodItem>();

                if (food != null && currentFood == null)
                {
                    return $"E to Transfer {food.GetItemName()} to Pan";
                }
                else if (currentFood != null)
                {
                    return "Pan Already Has Food";
                }
            }
        }

        // Holding food -> add to pan
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<FoodItem>())
        {
            if (currentFood == null)
            {
                FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();
                return $"E to Put {food.GetItemName()} in Pan";
            }
            return "Pan Already Has Food";
        }

        // Empty hands -> pick up pan (shows when cooked or not cooking)
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Pick Up {GetItemName()}";
        }

        return "";
    }

    public void Interact()
    {
        // Case 1: Holding plate with food
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            BurgerPlate plate = PlayerHands.Instance.GetHeldItem<BurgerPlate>();

            if (plate.layers.Count > 0 && currentFood == null)
            {
                GameObject plateObj = PlayerHands.Instance.currentItem;
                GameObject topLayer = plate.RemoveTopLayer();
                FoodItem food = topLayer.GetComponent<FoodItem>();

                if (food != null)
                {
                    topLayer.transform.SetParent(null);
                    topLayer.transform.localScale = Vector3.one;
                    TryAddFood(food);

                    // Drop plate
                    PlayerHands.Instance.currentItem = null;
                    plate.OnDrop();
                    plateObj.transform.position = transform.position + Vector3.right * 0.5f;

                    // Make plate pickupable
                    Collider plateCol = plateObj.GetComponent<Collider>();
                    if (plateCol != null)
                    {
                        plateCol.enabled = true;
                        plateCol.isTrigger = false;
                    }

                    Rigidbody plateRb = plateObj.GetComponent<Rigidbody>();
                    if (plateRb != null)
                    {
                        plateRb.isKinematic = false;
                        plateRb.useGravity = true;
                    }

                    Renderer[] renderers = plateObj.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderers)
                    {
                        r.enabled = true;
                    }

                    plateObj.transform.localScale = Vector3.one;

                    if (!plateObj.CompareTag("Interactable"))
                    {
                        plateObj.tag = "Interactable";
                    }

                    // Pick up pan
                    PlayerHands.Instance.TryPickup(gameObject);
                    Debug.Log($"🍳 Transferred {food.GetItemName()} from plate to pan!");
                    return;
                }
            }
        }

        // Case 2: Holding food directly
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<FoodItem>())
        {
            if (currentFood == null)
            {
                FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();
                GameObject foodObj = PlayerHands.Instance.currentItem;

                PlayerHands.Instance.currentItem = null;

                foodObj.transform.SetParent(null);
                foodObj.transform.localScale = Vector3.one;

                TryAddFood(food);
                PlayerHands.Instance.TryPickup(gameObject);

                Debug.Log($"🍳 Added {food.GetItemName()} to pan!");
                return;
            }
        }

        // Case 3: Empty hands - pick up
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            PlayerHands.Instance.TryPickup(gameObject);
        }
    }
}