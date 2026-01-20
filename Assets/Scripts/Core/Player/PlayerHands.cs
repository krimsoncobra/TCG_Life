using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages what the player is currently holding.
/// Items can be picked up, held, and placed down.
/// </summary>
public class PlayerHands : MonoBehaviour
{
    public static PlayerHands Instance;

    [Header("Hand Position")]
    [Tooltip("Where held items appear (create empty child at camera position)")]
    public Transform handPosition;

    [Header("Current State")]
    public IHoldable currentItem;
    public GameObject currentItemObject;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private InputAction dropAction;

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

        // Setup drop/throw input (Q key)
        dropAction = new InputAction("Drop", InputActionType.Button);
        dropAction.AddBinding("<Keyboard>/q");
        dropAction.performed += ctx => TryDrop();
    }

    void OnEnable() => dropAction.Enable();
    void OnDisable() => dropAction.Disable();

    void Start()
    {
        // Create hand position if not assigned
        if (handPosition == null)
        {
            GameObject handPos = new GameObject("HandPosition");
            handPos.transform.SetParent(Camera.main.transform);
            handPos.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f); // Slightly right and down from camera
            handPos.transform.localRotation = Quaternion.identity;
            handPosition = handPos.transform;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  PICKUP / DROP SYSTEM
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Pick up an item. Returns true if successful.
    /// </summary>
    public bool TryPickup(GameObject itemObject)
    {
        // Can't pick up if already holding something
        if (IsHoldingSomething())
        {
            if (showDebugLogs)
                Debug.LogWarning("⚠️ Already holding something!");
            return false;
        }

        IHoldable holdable = itemObject.GetComponent<IHoldable>();

        if (holdable == null)
        {
            Debug.LogError($"❌ {itemObject.name} doesn't have IHoldable component!");
            return false;
        }

        // Can this item be picked up right now?
        if (!holdable.CanPickup())
        {
            if (showDebugLogs)
                Debug.LogWarning($"⚠️ Can't pick up {itemObject.name} right now");
            return false;
        }

        // Pick it up!
        currentItem = holdable;
        currentItemObject = itemObject;

        holdable.OnPickup(handPosition);

        if (showDebugLogs)
            Debug.Log($"✅ Picked up: {itemObject.name}");

        return true;
    }

    /// <summary>
    /// Drop/place the currently held item
    /// </summary>
    public void TryDrop()
    {
        if (!IsHoldingSomething()) return;

        currentItem.OnDrop();

        if (showDebugLogs)
            Debug.Log($"📦 Dropped: {currentItemObject.name}");

        currentItem = null;
        currentItemObject = null;
    }

    /// <summary>
    /// Place held item at a specific position (for placing on stations)
    /// </summary>
    public bool TryPlaceAt(Transform targetPosition)
    {
        if (!IsHoldingSomething()) return false;

        currentItem.OnPlaceAt(targetPosition);

        if (showDebugLogs)
            Debug.Log($"📍 Placed {currentItemObject.name} at {targetPosition.name}");

        currentItem = null;
        currentItemObject = null;

        return true;
    }

    /// <summary>
    /// Check if player is holding something
    /// </summary>
    public bool IsHoldingSomething()
    {
        return currentItem != null && currentItemObject != null;
    }

    /// <summary>
    /// Check if player is holding a specific type of item
    /// </summary>
    public bool IsHolding<T>() where T : IHoldable
    {
        return IsHoldingSomething() && currentItem is T;
    }

    /// <summary>
    /// Get the currently held item of a specific type
    /// </summary>
    public T GetHeldItem<T>() where T : IHoldable
    {
        if (IsHolding<T>())
            return (T)currentItem;

        return default(T);
    }
}

// ═══════════════════════════════════════════════════════════════
//  HOLDABLE INTERFACE
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// Interface for items that can be picked up and held by the player
/// </summary>
public interface IHoldable
{
    bool CanPickup();
    void OnPickup(Transform handPosition);
    void OnDrop();
    void OnPlaceAt(Transform targetPosition);
    string GetItemName();
}