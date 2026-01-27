using UnityEngine;

/// <summary>
/// Basket station - grab baskets, put fries in baskets
/// Similar to PanStation
/// </summary>
public class BasketStation : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GameObject basketPrefab;
    public Transform spawnPoint;

    [Header("Inventory")]
    public int basketCount = 10;

    public string GetPromptText()
    {
        // Case 1: Holding fries → Put in basket
        if (PlayerHands.Instance.IsHolding<UncookedFries>())
        {
            return "E to Put Fries in Basket";
        }

        // Case 2: Empty hands → Grab basket
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Grab Basket ({basketCount})";
        }

        return "Basket Station";
    }

    public void Interact()
    {
        // Case 1: Holding fries - create basket with fries in it
        if (PlayerHands.Instance.IsHolding<UncookedFries>())
        {
            if (basketCount <= 0)
            {
                Debug.LogWarning("⚠️ No baskets left!");
                return;
            }

            UncookedFries fries = PlayerHands.Instance.GetHeldItem<UncookedFries>();

            // Spawn basket
            GameObject basketObj = Instantiate(basketPrefab, spawnPoint.position, spawnPoint.rotation);
            FryerBasket basket = basketObj.GetComponent<FryerBasket>();

            if (basket != null)
            {
                // Get the fries object before clearing hands
                GameObject friesObj = PlayerHands.Instance.currentItem;

                // Remove fries from hands (but don't destroy it)
                PlayerHands.Instance.currentItem = null;

                // Reset fries transform before adding to basket
                friesObj.transform.SetParent(null);
                friesObj.transform.localScale = Vector3.one;

                // Add fries to basket (this will parent it correctly)
                basket.TryAddFries(fries);

                // Pick up basket
                PlayerHands.Instance.TryPickup(basketObj);

                basketCount--;
                Debug.Log($"🧺 Put {fries.GetItemName()} in basket!");
            }

            return;
        }

        // Case 2: Empty hands - grab a basket
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            if (basketCount <= 0)
            {
                Debug.LogWarning("⚠️ No baskets left!");
                return;
            }

            GameObject basket = Instantiate(basketPrefab, spawnPoint.position, spawnPoint.rotation);

            if (PlayerHands.Instance.TryPickup(basket))
            {
                basketCount--;
                Debug.Log($"🧺 Grabbed basket! ({basketCount} remaining)");
            }
        }
    }
}