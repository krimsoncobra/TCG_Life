using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Enhanced PlayerHands - allows dropping items anywhere
/// Press Q to drop current item on the ground
/// </summary>
public class PlayerHands : MonoBehaviour
{
    [Header("Hand Settings")]
    public Transform handPosition;
    public float pickupRange = 3f;

    [Header("Drop Settings")]
    [Tooltip("How far in front of player to drop items")]
    public float dropDistance = 1f;

    [Tooltip("Height above ground to drop items")]
    public float dropHeight = 0.5f;

    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer;

    [Header("Input")]
    public KeyCode dropKey = KeyCode.Q;
    private InputAction dropAction;

    [Header("Current State")]
    public GameObject currentItem;

    public static PlayerHands Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Setup drop input
        dropAction = new InputAction("Drop", InputActionType.Button);
        dropAction.AddBinding($"<Keyboard>/{dropKey.ToString().ToLower()}");
        dropAction.performed += ctx => TryDropItem();
    }

    void OnEnable() => dropAction?.Enable();
    void OnDisable() => dropAction?.Disable();

    void Start()
    {
        if (handPosition == null)
        {
            Debug.LogError("❌ PlayerHands: Hand Position not assigned!");
        }
    }

    /// <summary>
    /// Pick up an item (called from PlayerInteract)
    /// </summary>
    public bool TryPickup(GameObject item)
    {
        if (currentItem != null)
        {
            Debug.LogWarning("⚠️ Hands are full! Drop current item first (Press Q)");
            return false;
        }

        IHoldable holdable = item.GetComponent<IHoldable>();
        if (holdable == null)
        {
            Debug.LogWarning($"⚠️ {item.name} is not holdable!");
            return false;
        }

        if (!holdable.CanPickup())
        {
            return false;
        }

        currentItem = item;
        holdable.OnPickup(handPosition);

        Debug.Log($"✅ Picked up: {holdable.GetItemName()}");
        return true;
    }

    /// <summary>
    /// Drop the current item on the ground in front of player
    /// </summary>
    void TryDropItem()
    {
        if (currentItem == null)
        {
            Debug.Log("Nothing to drop!");
            return;
        }

        IHoldable holdable = currentItem.GetComponent<IHoldable>();
        if (holdable == null)
        {
            Debug.LogError($"❌ Current item {currentItem.name} is not IHoldable!");
            return;
        }

        // Calculate drop position
        Vector3 dropPosition = CalculateDropPosition();

        // Drop the item
        holdable.OnDrop();
        currentItem.transform.position = dropPosition;

        // CRITICAL: Make sure item is interactable after drop
        // Re-enable collider
        Collider col = currentItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        // Make sure it has the Interactable tag
        if (!currentItem.CompareTag("Interactable"))
        {
            currentItem.tag = "Interactable";
            Debug.Log($"✅ Set {currentItem.name} tag to Interactable");
        }

        // Re-enable rigidbody physics
        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"📦 Dropped: {holdable.GetItemName()} at {dropPosition}");

        currentItem = null;
    }

    /// <summary>
    /// Calculate where to drop the item (in front of player, on ground)
    /// </summary>
    Vector3 CalculateDropPosition()
    {
        // Start position: in front of player
        Vector3 forwardPosition = transform.position + transform.forward * dropDistance;

        // Raycast down to find ground
        Ray ray = new Ray(forwardPosition + Vector3.up * 2f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, groundLayer))
        {
            // Drop on the ground surface
            return hit.point + Vector3.up * dropHeight;
        }
        else
        {
            // No ground found, drop at player height
            return forwardPosition + Vector3.up * dropHeight;
        }
    }

    /// <summary>
    /// Try to place current item at a specific location (e.g. on grill)
    /// </summary>
    public bool TryPlaceAt(Transform targetPosition)
    {
        if (currentItem == null)
        {
            Debug.LogWarning("⚠️ No item to place!");
            return false;
        }

        IHoldable holdable = currentItem.GetComponent<IHoldable>();
        if (holdable == null)
        {
            Debug.LogError($"❌ {currentItem.name} is not IHoldable!");
            return false;
        }

        holdable.OnPlaceAt(targetPosition);
        Debug.Log($"📍 Placed {holdable.GetItemName()} at {targetPosition.name}");

        currentItem = null;
        return true;
    }

    /// <summary>
    /// Check if holding any item
    /// </summary>
    public bool IsHoldingSomething()
    {
        return currentItem != null;
    }

    /// <summary>
    /// Check if holding a specific type
    /// </summary>
    public bool IsHolding<T>() where T : Component
    {
        if (currentItem == null) return false;
        return currentItem.GetComponent<T>() != null;
    }

    /// <summary>
    /// Get the currently held item
    /// </summary>
    public T GetHeldItem<T>() where T : Component
    {
        if (currentItem == null) return null;
        return currentItem.GetComponent<T>();
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (!enabled) return;

        // Show drop position
        Vector3 dropPos = CalculateDropPosition();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(dropPos, 0.2f);
        Gizmos.DrawLine(transform.position, dropPos);
    }
}