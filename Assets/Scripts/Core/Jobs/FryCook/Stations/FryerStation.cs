using UnityEngine;

/// <summary>
/// Fryer station - cooks fries placed in baskets
/// Similar to Grill but for FryerBaskets
/// </summary>
public class FryerStation : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform[] fryerSpots; // Multiple spots for baskets

    private FryerBasket[] currentBaskets;

    void Awake()
    {
        currentBaskets = new FryerBasket[fryerSpots.Length];
    }

    public string GetPromptText()
    {
        // Check if holding a basket
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable heldItem = PlayerHands.Instance.currentItem?.GetComponent<IHoldable>();

            if (heldItem is FryerBasket)
            {
                // Find empty spot
                if (GetEmptySpotIndex() >= 0)
                    return "E to Place Basket in Fryer";
                else
                    return "Fryer Full!";
            }
            else if (heldItem is UncookedFries)
            {
                return "Put Fries in Basket First!";
            }
        }

        // If not holding anything, check if there are baskets to pick up
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            FryerBasket nearestBasket = FindNearestBasket();
            if (nearestBasket != null)
            {
                return "E to Pick Up Basket";
            }
        }

        return "Fryer Station";
    }

    public void Interact()
    {
        // Priority 1: If not holding anything, try to pick up a basket from fryer
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            FryerBasket nearestBasket = FindNearestBasket();

            if (nearestBasket != null)
            {
                // Find which spot it's in
                int spotIndex = FindBasketSpotIndex(nearestBasket);

                if (spotIndex >= 0)
                {
                    // Remove from spot tracking
                    currentBaskets[spotIndex] = null;
                    nearestBasket.isInFryer = false;

                    // Pick it up
                    if (PlayerHands.Instance.TryPickup(nearestBasket.gameObject))
                    {
                        Debug.Log($"✅ Picked up basket from fryer spot {spotIndex}");
                    }
                }

                return;
            }
        }

        // Priority 2: Place basket in fryer
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable heldItem = PlayerHands.Instance.currentItem?.GetComponent<IHoldable>();

            if (heldItem is FryerBasket basket)
            {
                int spotIndex = GetEmptySpotIndex();

                if (spotIndex >= 0)
                {
                    PlayerHands.Instance.TryPlaceAt(fryerSpots[spotIndex]);
                    currentBaskets[spotIndex] = basket;
                    basket.isInFryer = true;

                    if (basket.currentFries != null)
                    {
                        basket.currentFries.StartCooking();
                        Debug.Log($"🍟 Placed basket with fries in fryer spot {spotIndex}");
                    }
                    else
                    {
                        Debug.Log($"🍟 Placed empty basket in fryer spot {spotIndex}");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Fryer is full!");
                }
            }
            else if (heldItem is UncookedFries)
            {
                Debug.LogWarning("⚠️ Can't place fries directly in fryer! Use a basket!");
            }
        }
    }

    int GetEmptySpotIndex()
    {
        for (int i = 0; i < currentBaskets.Length; i++)
        {
            if (currentBaskets[i] == null)
                return i;
        }
        return -1;
    }

    FryerBasket FindNearestBasket()
    {
        // Find closest basket to player
        float closestDist = float.MaxValue;
        FryerBasket closest = null;

        foreach (FryerBasket basket in currentBaskets)
        {
            if (basket != null)
            {
                float dist = Vector3.Distance(basket.transform.position, PlayerHands.Instance.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = basket;
                }
            }
        }

        return closest;
    }

    int FindBasketSpotIndex(FryerBasket basket)
    {
        for (int i = 0; i < currentBaskets.Length; i++)
        {
            if (currentBaskets[i] == basket)
                return i;
        }
        return -1;
    }

    public void RemoveBasket(int spotIndex)
    {
        if (spotIndex >= 0 && spotIndex < currentBaskets.Length)
        {
            currentBaskets[spotIndex] = null;
        }
    }
}