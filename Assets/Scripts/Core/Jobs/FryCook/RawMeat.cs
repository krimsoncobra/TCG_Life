using UnityEngine;

/// <summary>
/// Raw meat chunk that needs to be smashed
/// </summary>
public class RawMeat : MonoBehaviour, IHoldable
{
    [Header("Prefab References")]
    public GameObject burgerPattyPrefab;  // What this becomes when smashed

    [Header("Pickup Settings")]
    [Tooltip("Offset position when held in hand")]
    public Vector3 handOffset = Vector3.zero;

    [Tooltip("Rotation when held in hand")]
    public Vector3 handRotation = Vector3.zero;

    [Tooltip("Scale when held (1,1,1 = no change)")]
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

    public string GetItemName() => "Raw Meat";

    /// <summary>
    /// Convert this meat into a burger patty
    /// </summary>
    public GameObject SmashIntoPatty()
    {
        if (burgerPattyPrefab == null)
        {
            Debug.LogError("❌ No burger patty prefab assigned to RawMeat!");
            return null;
        }

        // Store position before destroying
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;
        Transform parentTransform = transform.parent;
        Vector3 localPos = transform.localPosition;

        // Spawn patty at same location
        GameObject patty = Instantiate(burgerPattyPrefab, spawnPos, spawnRot);

        Debug.Log($"🔨 Spawning patty at {spawnPos}");

        // Copy parent if we had one
        if (parentTransform != null)
        {
            patty.transform.SetParent(parentTransform);
            patty.transform.localPosition = localPos;
            Debug.Log($"📍 Parented patty to {parentTransform.name} at local pos {localPos}");
        }

        // Destroy the raw meat
        Destroy(gameObject);

        Debug.Log($"✅ Smashed meat into patty! Patty object: {patty.name}");

        return patty;
    }
}