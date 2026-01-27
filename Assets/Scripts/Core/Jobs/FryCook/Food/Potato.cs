using UnityEngine;

/// <summary>
/// Raw potato that needs to be cut into fries
/// </summary>
public class Potato : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Prefab References")]
    [Tooltip("What this becomes when cut")]
    public GameObject uncookedFriesPrefab;

    [Header("Pickup Settings")]
    public Vector3 handOffset = Vector3.zero;
    public Vector3 handRotation = Vector3.zero;
    public Vector3 handScale = Vector3.one;

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

    public string GetItemName() => "Potato";

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHoldingSomething())
            return "";

        return "E to Pick Up Potato";
    }

    public void Interact()
    {
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            PlayerHands.Instance.TryPickup(gameObject);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CUTTING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Convert this potato into uncooked fries
    /// </summary>
    public GameObject CutIntoFries()
    {
        if (uncookedFriesPrefab == null)
        {
            Debug.LogError("❌ No uncooked fries prefab assigned to Potato!");
            return null;
        }

        // Store position before destroying
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;
        Transform parentTransform = transform.parent;
        Vector3 localPos = transform.localPosition;

        // Spawn fries at same location
        GameObject fries = Instantiate(uncookedFriesPrefab, spawnPos, spawnRot);

        Debug.Log($"🔪 Spawning fries at {spawnPos}");

        // Copy parent if we had one
        if (parentTransform != null)
        {
            fries.transform.SetParent(parentTransform);
            fries.transform.localPosition = localPos;
            Debug.Log($"📍 Parented fries to {parentTransform.name} at local pos {localPos}");
        }

        // Destroy the potato
        Destroy(gameObject);

        Debug.Log($"✅ Cut potato into fries! Fries object: {fries.name}");

        return fries;
    }
}