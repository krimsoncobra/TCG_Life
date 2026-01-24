using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Plate that holds burger components in layers
/// EXACTLY MIRRORS CookingPan.cs structure
/// </summary>
public class BurgerPlate : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Plate Settings")]
    public Transform stackPosition;
    public float layerHeight = 0.02f;

    [Header("Pickup Settings")]
    public Vector3 handOffset = new Vector3(0, -0.1f, 0.3f);
    public Vector3 handRotation = new Vector3(0, 0, 0);
    public Vector3 handScale = Vector3.one;

    [Header("Layer Offsets")]
    public float foodItemOffset = -0.01f;
    public float bottomBunOffset = 0f;
    public float topBunOffset = 0f;

    [Header("Current State")]
    public List<GameObject> layers = new List<GameObject>();

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;

        if (stackPosition == null)
        {
            GameObject stackObj = new GameObject("StackPosition");
            stackObj.transform.SetParent(transform);
            stackObj.transform.localPosition = new Vector3(0, 0.01f, 0);
            stackObj.transform.localRotation = Quaternion.identity;
            stackPosition = stackObj.transform;
        }
    }

    public bool TryAddLayer(GameObject item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot add null item to plate!");
            return false;
        }

        float yOffset = layers.Count * layerHeight;

        // Apply offset based on item type
        float itemOffset = 0f;
        if (item.GetComponent<FoodItem>() != null)
            itemOffset = foodItemOffset;
        else if (item.GetComponent<BottomBun>() != null)
            itemOffset = bottomBunOffset;
        else if (item.GetComponent<TopBun>() != null)
            itemOffset = topBunOffset;

        // Parent to plate
        item.transform.SetParent(stackPosition);
        item.transform.localPosition = new Vector3(0, yOffset + itemOffset, 0);
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;

        // Disable physics
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = true;
            itemRb.useGravity = false;
        }

        Collider itemCol = item.GetComponent<Collider>();
        if (itemCol != null)
        {
            itemCol.enabled = false;
        }

        // Stop cooking
        FoodItem foodItem = item.GetComponent<FoodItem>();
        if (foodItem != null)
        {
            foodItem.StopCooking();
        }

        layers.Add(item);
        Debug.Log($"✅ Added {item.name} to plate (layer {layers.Count})");
        return true;
    }

    public GameObject RemoveTopLayer()
    {
        if (layers.Count == 0)
        {
            Debug.LogWarning("No layers to remove!");
            return null;
        }

        GameObject topLayer = layers[layers.Count - 1];
        layers.RemoveAt(layers.Count - 1);

        topLayer.transform.SetParent(null);

        Rigidbody itemRb = topLayer.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = false;
            itemRb.useGravity = true;
        }

        Collider itemCol = topLayer.GetComponent<Collider>();
        if (itemCol != null)
        {
            itemCol.enabled = true;
        }

        Debug.Log($"🗑️ Removed {topLayer.name} from plate");
        return topLayer;
    }

    // ═══════════════════════════════════════════════════════════════
    //  IHOLDABLE IMPLEMENTATION - EXACT COPY FROM COOKINGPAN
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

        Debug.Log($"✅ Picked up plate with {layers.Count} layers");
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

        Debug.Log($"📦 Dropped plate");
    }

    public void OnPlaceAt(Transform targetPosition)
    {
        transform.SetParent(targetPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;

        if (rb != null)
            rb.isKinematic = true;
    }

    public string GetItemName()
    {
        if (layers.Count == 0)
            return "Empty Plate";

        return $"Plate ({layers.Count} layers)";
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        // If holding pan with food, can transfer to this plate
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<CookingPan>())
        {
            CookingPan pan = PlayerHands.Instance.GetHeldItem<CookingPan>();
            if (pan.currentFood != null)
            {
                return $"E to Transfer {pan.currentFood.GetItemName()} to Plate";
            }
        }

        // If holding food directly, can add it to this plate
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<FoodItem>())
        {
            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();
            return $"E to Put {food.GetItemName()} on Plate";
        }

        // If hands empty, can pick up the plate
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Pick Up {GetItemName()}";
        }

        return "";
    }

    public void Interact()
    {
        // Case 1: Holding pan with food - transfer food to plate
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<CookingPan>())
        {
            CookingPan pan = PlayerHands.Instance.GetHeldItem<CookingPan>();

            if (pan.currentFood != null)
            {
                GameObject panObj = PlayerHands.Instance.currentItem;

                // Remove food from pan
                FoodItem food = pan.currentFood;
                GameObject foodObj = food.gameObject;
                pan.RemoveFood();

                // Reset food transform
                foodObj.transform.SetParent(null);
                foodObj.transform.localScale = Vector3.one;

                // Add to this plate
                TryAddLayer(foodObj);

                // Drop the pan
                PlayerHands.Instance.currentItem = null;
                pan.OnDrop();
                panObj.transform.position = transform.position + Vector3.right * 0.5f;

                // Pick up this plate
                PlayerHands.Instance.TryPickup(gameObject);

                Debug.Log($"🍽️ Transferred {food.GetItemName()} from pan to plate!");
                return;
            }
        }

        // Case 2: Holding food directly - add it to this plate
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<FoodItem>())
        {
            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();
            GameObject foodObj = PlayerHands.Instance.currentItem;

            // Remove from hands
            PlayerHands.Instance.currentItem = null;

            // Reset transform
            foodObj.transform.SetParent(null);
            foodObj.transform.localScale = Vector3.one;

            // Add to this plate
            TryAddLayer(foodObj);

            // Pick up the plate
            PlayerHands.Instance.TryPickup(gameObject);

            Debug.Log($"🍽️ Added {food.GetItemName()} to plate and picked it up!");
            return;
        }

        // Case 3: Empty hands - pick up the plate
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            PlayerHands.Instance.TryPickup(gameObject);
        }
    }
}