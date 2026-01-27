using UnityEngine;

/// <summary>
/// Cooked fries - ready to serve!
/// This is the final product that gets handed to the ticket window
/// Created at the CookedFryStation when transferring from basket
/// </summary>
public class CookedFries : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Fries Info")]
    public bool isBurnt = false;
    public string friesName = "French Fries";

    [Header("Pickup Settings")]
    public Vector3 handOffset = Vector3.zero;
    public Vector3 handRotation = Vector3.zero;
    public Vector3 handScale = Vector3.one;

    [Header("Visual")]
    public Renderer friesRenderer;
    public Color cookedColor = new Color(0.9f, 0.7f, 0.2f); // Golden
    public Color burntColor = new Color(0.2f, 0.15f, 0.1f); // Dark brown

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;

        // Auto-add physics if missing
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning($"⚠️ Added Rigidbody to {gameObject.name}");
        }

        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            Debug.LogWarning($"⚠️ Added BoxCollider to {gameObject.name}");
        }
    }

    void Start()
    {
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (friesRenderer == null) return;

        friesRenderer.material.color = isBurnt ? burntColor : cookedColor;
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
        if (isBurnt)
            return "Burnt Fries";
        return friesName;
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

    /// <summary>
    /// Create cooked fries from UncookedFries state
    /// </summary>
    public static CookedFries CreateFromUncookedFries(UncookedFries uncookedFries, GameObject cookedFriesPrefab)
    {
        if (cookedFriesPrefab == null)
        {
            Debug.LogError("❌ No cooked fries prefab provided!");
            return null;
        }

        // Get cooking state
        bool isBurnt = uncookedFries.currentState == CookingState.Burnt;

        // Spawn cooked fries at same position
        Vector3 spawnPos = uncookedFries.transform.position;
        Quaternion spawnRot = uncookedFries.transform.rotation;

        GameObject cookedObj = Instantiate(cookedFriesPrefab, spawnPos, spawnRot);
        CookedFries cooked = cookedObj.GetComponent<CookedFries>();

        if (cooked != null)
        {
            cooked.isBurnt = isBurnt;
            cooked.UpdateVisual();
            Debug.Log($"🍟 Created {(isBurnt ? "burnt" : "cooked")} fries!");
        }

        return cooked;
    }
}