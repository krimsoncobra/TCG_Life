using UnityEngine;

/// <summary>
/// Universal Trash Can - Handles disposal of all items in one place
/// Supports: Burnt pans, fries, baskets, and any other disposable items
/// Attach this to your trash can GameObject
/// </summary>
public class TrashCan : MonoBehaviour, IInteractable
{
    [Header("Replacement Prefabs")]
    [Tooltip("Clean pan to spawn when disposing burnt pan")]
    public GameObject cleanPanPrefab;

    [Tooltip("Clean basket to spawn when disposing basket")]
    public GameObject cleanBasketPrefab;

    [Header("Disposal Settings")]
    [Tooltip("Auto-pickup clean replacements after disposal")]
    public bool autoPickupReplacement = true;

    [Tooltip("Where to spawn replacements (if null, uses trash can position)")]
    public Transform replacementSpawnPoint;

    [Header("Visual Feedback")]
    public ParticleSystem disposeEffect; // Optional: particles when trashing
    public AudioClip trashSound; // Optional: trash can sound

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && trashSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        if (PlayerHands.Instance == null || !PlayerHands.Instance.IsHoldingSomething())
            return "Trash Can";

        GameObject heldItem = PlayerHands.Instance.currentItem;

        // Check what player is holding
        if (IsBurntPan(heldItem))
            return "E to Trash Burnt Pan (Get Clean Pan)";

        if (IsBasket(heldItem))
        {
            FryerBasket basket = heldItem.GetComponent<FryerBasket>();
            if (basket != null && basket.currentFries != null)
                return "E to Trash Basket with Fries (Get Clean Basket)";
            return "E to Trash Empty Basket (Get Clean Basket)";
        }

        if (IsFries(heldItem))
            return "E to Trash Fries";

        if (IsFood(heldItem))
            return "E to Trash Food";

        // Generic disposal
        return "E to Trash Item";
    }

    public void Interact()
    {
        if (PlayerHands.Instance == null || !PlayerHands.Instance.IsHoldingSomething())
        {
            Debug.Log("🗑️ Nothing to trash!");
            return;
        }

        GameObject itemToTrash = PlayerHands.Instance.currentItem;

        Debug.Log($"🗑️ Trashing: {itemToTrash.name}");

        // Determine what type of item and handle accordingly
        if (IsBurntPan(itemToTrash))
        {
            DisposeBurntPan(itemToTrash);
        }
        else if (IsBasket(itemToTrash))
        {
            DisposeBasket(itemToTrash);
        }
        else if (IsFries(itemToTrash))
        {
            DisposeFries(itemToTrash);
        }
        else if (IsFood(itemToTrash))
        {
            DisposeFood(itemToTrash);
        }
        else
        {
            // Generic disposal
            DisposeGeneric(itemToTrash);
        }

        // Play effects
        PlayDisposeEffects();
    }

    // ═══════════════════════════════════════════════════════════════
    //  DISPOSAL METHODS
    // ═══════════════════════════════════════════════════════════════

    void DisposeBurntPan(GameObject burntPan)
    {
        Debug.Log("🗑️ Disposing burnt pan...");

        // Remove from player hands first
        PlayerHands.Instance.currentItem = null;

        // Spawn clean pan replacement
        if (cleanPanPrefab != null)
        {
            SpawnReplacement(cleanPanPrefab, "clean pan");
        }
        else
        {
            Debug.LogWarning("⚠️ No clean pan prefab assigned to TrashCan!");
        }

        // Destroy burnt pan
        Destroy(burntPan);

        Debug.Log("✅ Burnt pan disposed!");
    }

    void DisposeBasket(GameObject basket)
    {
        Debug.Log("🗑️ Disposing basket...");

        FryerBasket basketComponent = basket.GetComponent<FryerBasket>();

        // If basket has fries, destroy them
        if (basketComponent != null && basketComponent.currentFries != null)
        {
            Debug.Log($"🗑️ Destroying fries in basket: {basketComponent.currentFries.name}");
            Destroy(basketComponent.currentFries.gameObject);
            basketComponent.currentFries = null;
        }

        // Remove from player hands
        PlayerHands.Instance.currentItem = null;

        // Spawn clean basket replacement
        if (cleanBasketPrefab != null)
        {
            SpawnReplacement(cleanBasketPrefab, "clean basket");
        }
        else
        {
            Debug.LogWarning("⚠️ No clean basket prefab assigned to TrashCan!");
        }

        // Destroy old basket
        Destroy(basket);

        Debug.Log("✅ Basket disposed!");
    }

    void DisposeFries(GameObject fries)
    {
        Debug.Log($"🗑️ Disposing fries: {fries.name}");

        // Remove from player hands
        PlayerHands.Instance.currentItem = null;

        // Destroy fries (no replacement)
        Destroy(fries);

        Debug.Log("✅ Fries disposed!");
    }

    void DisposeFood(GameObject food)
    {
        Debug.Log($"🗑️ Disposing food: {food.name}");

        // Remove from player hands
        PlayerHands.Instance.currentItem = null;

        // Destroy food (no replacement)
        Destroy(food);

        Debug.Log("✅ Food disposed!");
    }

    void DisposeGeneric(GameObject item)
    {
        Debug.Log($"🗑️ Disposing generic item: {item.name}");

        // Remove from player hands
        PlayerHands.Instance.currentItem = null;

        // Destroy item
        Destroy(item);

        Debug.Log("✅ Item disposed!");
    }

    // ═══════════════════════════════════════════════════════════════
    //  HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    void SpawnReplacement(GameObject prefab, string itemName)
    {
        if (prefab == null)
        {
            Debug.LogError($"❌ Cannot spawn {itemName} - prefab is null!");
            return;
        }

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ Could not find Player!");
            return;
        }

        // Determine spawn position
        Vector3 spawnPos;
        if (replacementSpawnPoint != null)
        {
            spawnPos = replacementSpawnPoint.position;
        }
        else
        {
            // Spawn in front of player
            spawnPos = player.transform.position + player.transform.forward * 1.5f;
            spawnPos.y = player.transform.position.y + 1f;
        }

        Debug.Log($"📍 Spawning {itemName} at position: {spawnPos}");

        // Spawn the replacement
        GameObject replacement = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (replacement == null)
        {
            Debug.LogError($"❌ Failed to instantiate {itemName}!");
            return;
        }

        Debug.Log($"✨ Successfully spawned {itemName}: {replacement.name}");

        // Auto-pickup if enabled
        if (autoPickupReplacement && PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            Debug.Log($"🤲 Attempting to auto-equip {itemName}...");

            bool pickedUp = PlayerHands.Instance.TryPickup(replacement);
            if (pickedUp)
            {
                Debug.Log($"✅ Auto-equipped {itemName} to player hands");
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to auto-equip {itemName} - player might need to pick it up manually");
            }
        }
        else
        {
            Debug.Log($"ℹ️ {itemName} left on ground (auto-pickup disabled or hands full)");
        }
    }

    void PlayDisposeEffects()
    {
        // Play particle effect
        if (disposeEffect != null)
        {
            disposeEffect.Play();
        }

        // Play sound
        if (trashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(trashSound);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  TYPE CHECKING METHODS
    // ═══════════════════════════════════════════════════════════════

    bool IsBurntPan(GameObject item)
    {
        if (item == null) return false;

        // Check if it's a cooking pan with burnt food
        CookingPan pan = item.GetComponent<CookingPan>();
        if (pan != null && pan.currentFood != null)
        {
            return pan.currentFood.currentState == CookingState.Burnt;
        }

        // Check if item name contains "burnt" and "pan"
        string itemName = item.name.ToLower();
        return itemName.Contains("burnt") && itemName.Contains("pan");
    }

    bool IsBasket(GameObject item)
    {
        if (item == null) return false;
        return item.GetComponent<FryerBasket>() != null;
    }

    bool IsFries(GameObject item)
    {
        if (item == null) return false;

        // Check for UncookedFries
        if (item.GetComponent<UncookedFries>() != null)
            return true;

        // Check for CookedFries
        if (item.GetComponent<CookedFries>() != null)
            return true;

        // Check name
        string itemName = item.name.ToLower();
        return itemName.Contains("fries") || itemName.Contains("fry");
    }

    bool IsFood(GameObject item)
    {
        if (item == null) return false;

        // Check for FoodItem component
        if (item.GetComponent<FoodItem>() != null)
            return true;

        // Check for common food names
        string itemName = item.name.ToLower();
        return itemName.Contains("burger") ||
               itemName.Contains("patty") ||
               itemName.Contains("bun") ||
               itemName.Contains("lettuce") ||
               itemName.Contains("tomato") ||
               itemName.Contains("cheese");
    }

    // ═══════════════════════════════════════════════════════════════
    //  TRIGGER-BASED DISPOSAL (Optional Alternative)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Optional: If you want items to be trashed by throwing them into the trash can
    /// Enable this by adding a Trigger collider to the trash can
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Skip if this is the player
        if (other.CompareTag("Player"))
            return;

        // Skip if player is holding this item (they should use E to trash)
        if (PlayerHands.Instance != null &&
            PlayerHands.Instance.currentItem != null &&
            other.gameObject == PlayerHands.Instance.currentItem)
        {
            return;
        }

        // This is for items THROWN into the trash (not held)
        GameObject item = other.gameObject;

        Debug.Log($"🗑️ Item entered trash can: {item.name}");

        // Check what type and dispose accordingly
        if (IsBurntPan(item))
        {
            // Don't spawn replacement for thrown items
            Destroy(item);
            Debug.Log("✅ Thrown burnt pan disposed!");
        }
        else if (IsBasket(item))
        {
            FryerBasket basket = item.GetComponent<FryerBasket>();
            if (basket != null && basket.currentFries != null)
            {
                Destroy(basket.currentFries.gameObject);
            }
            Destroy(item);
            Debug.Log("✅ Thrown basket disposed!");
        }
        else if (IsFries(item) || IsFood(item))
        {
            Destroy(item);
            Debug.Log("✅ Thrown food disposed!");
        }

        PlayDisposeEffects();
    }

    // ═══════════════════════════════════════════════════════════════
    //  GIZMOS
    // ═══════════════════════════════════════════════════════════════

    void OnDrawGizmos()
    {
        // Draw replacement spawn point
        if (replacementSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(replacementSpawnPoint.position, 0.2f);
        }
    }
}