using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Smart Burger Plate - automatically organizes layers in correct order
/// Order: Bottom Bun → Patty → Condiments → Top Bun
/// </summary>
public class BurgerPlate : MonoBehaviour, IHoldable, IInteractable
{
    [Header("Plate Settings")]
    public Transform stackPosition;
    public float layerHeight = 0.02f;

    [Header("Pickup Settings")]
    public Vector3 handOffset = new Vector3(0, -0.1f, 0.3f);
    public Vector3 handRotation = Vector3.zero;
    public Vector3 handScale = Vector3.one;

    [Header("Layer Offsets")]
    public float foodItemOffset = -0.01f;
    public float bottomBunOffset = 0f;
    public float topBunOffset = 0f;
    public float condimentOffset = 0f;

    [Header("Current State")]
    public List<GameObject> layers = new List<GameObject>();

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;

    // Layer ordering priority (lower = bottom)
    enum LayerPriority
    {
        BottomBun = 0,
        Patty = 1,
        Condiment = 2,
        TopBun = 3
    }

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

        // Add to layers list
        layers.Add(item);

        // Reorganize all layers in correct order
        ReorganizeLayers();

        Debug.Log($"✅ Added {item.name} to plate. Burger now has {layers.Count} layers.");
        return true;
    }

    /// <summary>
    /// Reorganizes all layers in the correct burger order
    /// Bottom Bun → Patty → Condiments → Top Bun
    /// </summary>
    void ReorganizeLayers()
    {
        // Sort layers by priority
        layers = layers.OrderBy(layer => GetLayerPriority(layer)).ToList();

        // Reposition each layer
        for (int i = 0; i < layers.Count; i++)
        {
            GameObject layer = layers[i];
            float yOffset = i * layerHeight;
            float itemOffset = GetLayerOffset(layer);
            float finalY = yOffset + itemOffset;

            // Parent to plate
            layer.transform.SetParent(stackPosition);
            layer.transform.localPosition = new Vector3(0, finalY, 0);
            layer.transform.localRotation = Quaternion.identity;
            // Keep original scale

            // Disable physics
            Rigidbody itemRb = layer.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                itemRb.isKinematic = true;
                itemRb.useGravity = false;
            }

            Collider itemCol = layer.GetComponent<Collider>();
            if (itemCol != null)
            {
                itemCol.enabled = false;
            }

            // Stop cooking if food
            FoodItem foodItem = layer.GetComponent<FoodItem>();
            if (foodItem != null)
            {
                foodItem.StopCooking();
            }
        }

        Debug.Log($"🔄 Reorganized burger: {GetBurgerDescription()}");
    }

    /// <summary>
    /// Get the priority/order for a layer
    /// </summary>
    int GetLayerPriority(GameObject layer)
    {
        if (layer.GetComponent<BottomBun>() != null)
            return (int)LayerPriority.BottomBun;

        if (layer.GetComponent<FoodItem>() != null)
            return (int)LayerPriority.Patty;

        if (layer.GetComponent<TopBun>() != null)
            return (int)LayerPriority.TopBun;

        // Future: Condiments can go here
        // if (layer.GetComponent<Condiment>() != null)
        //     return (int)LayerPriority.Condiment;

        return (int)LayerPriority.Condiment; // Default to middle
    }

    /// <summary>
    /// Get the Y offset for a specific layer type
    /// </summary>
    float GetLayerOffset(GameObject layer)
    {
        if (layer.GetComponent<FoodItem>() != null)
            return foodItemOffset;
        if (layer.GetComponent<BottomBun>() != null)
            return bottomBunOffset;
        if (layer.GetComponent<TopBun>() != null)
            return topBunOffset;

        return condimentOffset;
    }

    /// <summary>
    /// Get human-readable description of burger layers
    /// </summary>
    string GetBurgerDescription()
    {
        List<string> layerNames = new List<string>();
        foreach (GameObject layer in layers)
        {
            if (layer.GetComponent<BottomBun>() != null)
                layerNames.Add("Bottom Bun");
            else if (layer.GetComponent<TopBun>() != null)
                layerNames.Add("Top Bun");
            else if (layer.GetComponent<FoodItem>() != null)
                layerNames.Add(layer.GetComponent<FoodItem>().GetItemName());
            else
                layerNames.Add(layer.name);
        }
        return "[" + string.Join(", ", layerNames) + "]";
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

    /// <summary>
    /// Check if burger is complete (has all required components)
    /// </summary>
    public bool IsComplete()
    {
        bool hasBottomBun = false;
        bool hasPatty = false;
        bool hasTopBun = false;

        foreach (GameObject layer in layers)
        {
            if (layer.GetComponent<BottomBun>() != null)
                hasBottomBun = true;
            if (layer.GetComponent<FoodItem>() != null)
                hasPatty = true;
            if (layer.GetComponent<TopBun>() != null)
                hasTopBun = true;
        }

        return hasBottomBun && hasPatty && hasTopBun;
    }

    /// <summary>
    /// Check if burger has a specific component
    /// </summary>
    public bool HasBottomBun()
    {
        return layers.Any(layer => layer.GetComponent<BottomBun>() != null);
    }

    public bool HasPatty()
    {
        return layers.Any(layer => layer.GetComponent<FoodItem>() != null);
    }

    public bool HasTopBun()
    {
        return layers.Any(layer => layer.GetComponent<TopBun>() != null);
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

        Debug.Log($"✅ Picked up plate with {layers.Count} layers");
    }

    public void OnDrop()
    {
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
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

        if (rb != null)
            rb.isKinematic = true;
    }

    public string GetItemName()
    {
        if (layers.Count == 0)
            return "Empty Plate";

        if (IsComplete())
            return "Burger (Complete)";

        return $"Plate ({layers.Count} layers)";
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<CookingPan>())
        {
            CookingPan pan = PlayerHands.Instance.GetHeldItem<CookingPan>();
            if (pan.currentFood != null)
            {
                return $"E to Transfer {pan.currentFood.GetItemName()} to Plate";
            }
        }

        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<FoodItem>())
        {
            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();
            return $"E to Put {food.GetItemName()} on Plate";
        }

        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Pick Up {GetItemName()}";
        }

        return "";
    }

    public void Interact()
    {
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<CookingPan>())
        {
            CookingPan pan = PlayerHands.Instance.GetHeldItem<CookingPan>();

            if (pan.currentFood != null)
            {
                GameObject panObj = PlayerHands.Instance.currentItem;
                FoodItem food = pan.currentFood;
                GameObject foodObj = food.gameObject;
                pan.RemoveFood();

                foodObj.transform.SetParent(null);
                foodObj.transform.localScale = Vector3.one;

                TryAddLayer(foodObj);

                PlayerHands.Instance.currentItem = null;
                pan.OnDrop();
                panObj.transform.position = transform.position + Vector3.right * 0.5f;

                Collider plateCol = panObj.GetComponent<Collider>();
                if (plateCol != null)
                {
                    plateCol.enabled = true;
                    plateCol.isTrigger = false;
                }

                Rigidbody plateRb = panObj.GetComponent<Rigidbody>();
                if (plateRb != null)
                {
                    plateRb.isKinematic = false;
                    plateRb.useGravity = true;
                }

                foreach (Renderer r in panObj.GetComponentsInChildren<Renderer>())
                {
                    r.enabled = true;
                }

                panObj.transform.localScale = Vector3.one;

                if (!panObj.CompareTag("Interactable"))
                {
                    panObj.tag = "Interactable";
                }

                PlayerHands.Instance.TryPickup(gameObject);

                Debug.Log($"🍽️ Transferred {food.GetItemName()} from pan to plate!");
                return;
            }
        }

        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<FoodItem>())
        {
            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();
            GameObject foodObj = PlayerHands.Instance.currentItem;

            PlayerHands.Instance.currentItem = null;

            foodObj.transform.SetParent(null);
            foodObj.transform.localScale = Vector3.one;

            TryAddLayer(foodObj);

            PlayerHands.Instance.TryPickup(gameObject);

            Debug.Log($"🍽️ Added {food.GetItemName()} to plate and picked it up!");
            return;
        }

        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            PlayerHands.Instance.TryPickup(gameObject);
        }
    }
}