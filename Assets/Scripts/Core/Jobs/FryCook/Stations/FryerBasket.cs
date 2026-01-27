using UnityEngine;

/// <summary>
/// Fryer basket - holds fries and cooks them in oil
/// Similar to CookingPan but for fries
/// </summary>
public class FryerBasket : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Basket Settings")]
    public Transform friesPosition;

    [Tooltip("Scale multiplier for fries when in basket (e.g., 1.5 = 50% bigger)")]
    public float friesScaleMultiplier = 1.5f;

    [Header("Pickup Settings")]
    public Vector3 handOffset = new Vector3(0, -0.1f, 0.3f);
    public Vector3 handRotation = Vector3.zero;
    public Vector3 handScale = Vector3.one;

    [Header("Fryer Placement Settings")]
    [Tooltip("Position offset when placed in fryer")]
    public Vector3 fryerPlacementOffset = Vector3.zero;

    [Tooltip("Rotation when placed in fryer (try 0, 180, 0 to flip around)")]
    public Vector3 fryerPlacementRotation = new Vector3(0, 180, 0); // Try this to flip 180 degrees

    [Header("Current State")]
    public FoodItem currentFries; // Holds UncookedFries
    public bool isInFryer = false;

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;

        if (friesPosition == null)
        {
            GameObject friesPos = new GameObject("FriesPosition");
            friesPos.transform.SetParent(transform);
            friesPos.transform.localPosition = new Vector3(0, 0.05f, 0);
            friesPos.transform.localRotation = Quaternion.identity;
            friesPosition = friesPos.transform;
        }
    }

    void Update()
    {
        // Handle cooking state
        if (isInFryer && currentFries != null)
        {
            if (!currentFries.isOnGrill)
            {
                currentFries.StartCooking();
            }
        }
        else if (currentFries != null && currentFries.isOnGrill)
        {
            currentFries.StopCooking();
        }
    }

    public bool TryAddFries(FoodItem fries)
    {
        if (currentFries != null)
        {
            Debug.LogWarning("Basket already has fries!");
            return false;
        }

        currentFries = fries;
        fries.transform.SetParent(friesPosition);
        fries.transform.localPosition = Vector3.zero;
        fries.transform.localRotation = Quaternion.identity;
        fries.transform.localScale = Vector3.one * friesScaleMultiplier; // Scale up fries

        Collider friesCol = fries.GetComponent<Collider>();
        if (friesCol != null)
        {
            friesCol.enabled = false;
        }

        Rigidbody friesRb = fries.GetComponent<Rigidbody>();
        if (friesRb != null)
        {
            friesRb.isKinematic = true;
            friesRb.useGravity = false;
        }

        Debug.Log($"Added {fries.GetItemName()} to basket (scaled to {friesScaleMultiplier}x)");
        return true;
    }

    public FoodItem RemoveFries()
    {
        if (currentFries == null) return null;

        FoodItem fries = currentFries;
        currentFries = null;
        fries.transform.SetParent(null);

        Collider friesCol = fries.GetComponent<Collider>();
        if (friesCol != null)
        {
            friesCol.enabled = true;
        }

        Debug.Log($"Removed {fries.GetItemName()} from basket");
        return fries;
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

        isInFryer = false;

        // Unregister from UI when picked up
        if (CookingProgressUI.Instance != null)
        {
            CookingProgressUI.Instance.UnregisterFryerBasket(this);
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

        isInFryer = false;
    }

    public void OnPlaceAt(Transform targetPosition)
    {
        transform.SetParent(targetPosition);
        transform.localPosition = fryerPlacementOffset;
        transform.localRotation = Quaternion.Euler(fryerPlacementRotation);
        transform.localScale = originalScale;

        if (rb != null)
            rb.isKinematic = true;

        FryerStation fryer = targetPosition.GetComponentInParent<FryerStation>();

        if (fryer != null)
        {
            isInFryer = true;

            // Register with cooking progress UI
            if (CookingProgressUI.Instance != null && currentFries != null)
            {
                CookingProgressUI.Instance.RegisterFryerBasket(this);
            }
        }
    }

    public string GetItemName()
    {
        if (currentFries != null)
            return $"Basket ({currentFries.GetItemName()})";

        return "Empty Basket";
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        // Holding fries -> add to basket
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<UncookedFries>())
        {
            if (currentFries == null)
            {
                return "E to Put Fries in Basket";
            }
            return "Basket Already Has Fries";
        }

        // Empty hands -> pick up basket
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Pick Up {GetItemName()}";
        }

        return "";
    }

    public void Interact()
    {
        // Case 1: Holding fries directly
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<UncookedFries>())
        {
            if (currentFries == null)
            {
                UncookedFries fries = PlayerHands.Instance.GetHeldItem<UncookedFries>();
                GameObject friesObj = PlayerHands.Instance.currentItem;

                PlayerHands.Instance.currentItem = null;

                friesObj.transform.SetParent(null);
                friesObj.transform.localScale = Vector3.one;

                TryAddFries(fries);
                PlayerHands.Instance.TryPickup(gameObject);

                Debug.Log($"🍟 Added fries to basket!");
                return;
            }
        }

        // Case 2: Empty hands - pick up
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            PlayerHands.Instance.TryPickup(gameObject);
        }
    }
}