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

        flipAction = new InputAction("FlipFood", InputActionType.Button);
        flipAction.AddBinding($"<Keyboard>/{flipKey.ToString().ToLower()}");
        flipAction.performed += ctx => TryStartFlipMinigame();

        if (foodPosition == null)
        {
            GameObject foodPos = new GameObject("FoodPosition");
            foodPos.transform.SetParent(transform);
            foodPos.transform.localPosition = new Vector3(0, 0.05f, 0);
            foodPos.transform.localRotation = Quaternion.identity;
            foodPosition = foodPos.transform;
        }
    }

    void OnEnable() => flipAction?.Enable();
    void OnDisable() => flipAction?.Disable();

    void Update()
    {
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
               PlayerHands.Instance != null &&
               PlayerHands.Instance.IsHolding<CookingPan>();
    }

    void TryStartFlipMinigame()
    {
        if (!CanFlip()) return;

        if (currentFood.currentState != CookingState.Cooking &&
            currentFood.currentState != CookingState.Cooked)
        {
            Debug.LogWarning("Food isn't ready to flip yet!");
            return;
        }

        if (BurgerFlipMinigame.Instance != null)
        {
            BurgerFlipMinigame.Instance.StartMinigame(currentFood);
        }
        else
        {
            Debug.LogError("BurgerFlipMinigame not found!");
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

        // Empty hands -> pick up pan
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