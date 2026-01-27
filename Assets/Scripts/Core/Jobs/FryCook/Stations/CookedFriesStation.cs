using UnityEngine;

/// <summary>
/// Cooked Fry Station - Transfer cooked fries from basket to serving container
/// Similar to how PlateStation adds layers to plates
/// </summary>
public class CookedFryStation : MonoBehaviour, IInteractable
{
    [Header("Prefab Settings")]
    [Tooltip("The final cooked fries prefab (CookedFries.cs)")]
    public GameObject cookedFriesPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Where the cooked fries appear")]
    public Transform spawnPoint;

    [Header("Visual Feedback")]
    public ParticleSystem transferEffect; // Optional: particle effect on transfer

    public string GetPromptText()
    {
        // Check if holding a fryer basket with cooked fries
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHolding<FryerBasket>())
        {
            FryerBasket basket = PlayerHands.Instance.GetHeldItem<FryerBasket>();

            if (basket.currentFries != null)
            {
                UncookedFries fries = basket.currentFries as UncookedFries;

                if (fries != null)
                {
                    // Check if fries are cooked or burnt
                    if (fries.currentState == CookingState.Cooked || fries.currentState == CookingState.Burnt)
                    {
                        bool isBurnt = fries.currentState == CookingState.Burnt;
                        return $"E to Transfer {(isBurnt ? "Burnt" : "Cooked")} Fries";
                    }
                    else
                    {
                        return "Fries Not Ready Yet!";
                    }
                }
            }
            else
            {
                return "Basket is Empty!";
            }
        }

        return "Cooked Fry Station";
    }

    public void Interact()
    {
        if (PlayerHands.Instance == null || !PlayerHands.Instance.IsHolding<FryerBasket>())
        {
            Debug.Log("📋 No fryer basket held!");
            return;
        }

        FryerBasket basket = PlayerHands.Instance.GetHeldItem<FryerBasket>();

        if (basket.currentFries == null)
        {
            Debug.LogWarning("⚠️ Basket is empty!");
            return;
        }

        UncookedFries uncookedFries = basket.currentFries as UncookedFries;

        if (uncookedFries == null)
        {
            Debug.LogError("❌ Fries in basket are not UncookedFries type!");
            return;
        }

        // Check if fries are ready (cooked or burnt)
        if (uncookedFries.currentState != CookingState.Cooked &&
            uncookedFries.currentState != CookingState.Burnt)
        {
            Debug.LogWarning("⚠️ Fries not ready to transfer! Current state: " + uncookedFries.currentState);
            return;
        }

        // Transfer fries!
        TransferFries(basket, uncookedFries);
    }

    void TransferFries(FryerBasket basket, UncookedFries uncookedFries)
    {
        if (cookedFriesPrefab == null)
        {
            Debug.LogError("❌ No cooked fries prefab assigned!");
            return;
        }

        bool isBurnt = uncookedFries.currentState == CookingState.Burnt;

        Debug.Log($"🍟 Transferring {(isBurnt ? "burnt" : "cooked")} fries from basket...");

        // Determine spawn position
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Create cooked fries prefab
        GameObject cookedObj = Instantiate(cookedFriesPrefab, spawnPos, spawnRot);
        CookedFries cookedFries = cookedObj.GetComponent<CookedFries>();

        if (cookedFries != null)
        {
            // Set burnt state
            cookedFries.isBurnt = isBurnt;
            cookedFries.UpdateVisual();
        }
        else
        {
            Debug.LogError("❌ Cooked fries prefab doesn't have CookedFries component!");
            Destroy(cookedObj);
            return;
        }

        // Remove fries from basket (destroy the UncookedFries)
        GameObject uncookedObj = uncookedFries.gameObject;
        basket.currentFries = null;
        Destroy(uncookedObj);

        Debug.Log($"✅ Removed uncooked fries from basket");

        // Drop the now-empty basket
        GameObject basketObj = PlayerHands.Instance.currentItem;
        PlayerHands.Instance.currentItem = null;
        basket.OnDrop();

        // Position basket next to station
        basketObj.transform.position = transform.position + Vector3.right * 0.5f;

        Debug.Log($"📍 Dropped empty basket at {basketObj.transform.position}");

        // Auto-pickup the cooked fries
        bool pickedUp = PlayerHands.Instance.TryPickup(cookedObj);

        if (pickedUp)
        {
            Debug.Log($"✅ Auto-equipped cooked fries to player hands!");
        }
        else
        {
            Debug.LogWarning("⚠️ Failed to auto-pickup cooked fries - hands might be full");
        }

        // Play transfer effect
        if (transferEffect != null)
        {
            transferEffect.Play();
        }

        Debug.Log($"🎉 Successfully transferred {(isBurnt ? "burnt" : "cooked")} fries!");
    }

    void OnDrawGizmos()
    {
        // Draw spawn point indicator
        if (spawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.1f);
        }
    }
}