
using UnityEngine;

/// <summary>
/// Grill - cooks food placed on it (only accepts pans!)
/// </summary>
public class Grill : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform[] cookingSpots;  // Multiple spots for pans

    private CookingPan[] currentPans;

    void Awake()
    {
        currentPans = new CookingPan[cookingSpots.Length];
    }

    public string GetPromptText()
    {
        // Check if holding a pan
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable held = PlayerHands.Instance.currentItem;

            if (held is CookingPan)
            {
                // Find empty spot
                if (GetEmptySpotIndex() >= 0)
                    return "E to Place Pan on Grill";
            }
            else if (held is FoodItem)
            {
                return "Put Food in Pan First!";
            }
        }

        return "Grill";
    }

    public void Interact()
    {
        // Only accept pans!
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable held = PlayerHands.Instance.currentItem;

            if (held is CookingPan pan)
            {
                int spotIndex = GetEmptySpotIndex();

                if (spotIndex >= 0)
                {
                    PlayerHands.Instance.TryPlaceAt(cookingSpots[spotIndex]);
                    currentPans[spotIndex] = pan;
                    pan.isOnGrill = true;

                    if (pan.currentFood != null)
                    {
                        pan.currentFood.StartCooking();
                        Debug.Log($"🍳 Placed pan with {pan.currentFood.GetItemName()} on grill spot {spotIndex}");
                    }
                    else
                    {
                        Debug.Log($"🍳 Placed empty pan on grill spot {spotIndex}");
                    }
                }
            }
            else if (held is FoodItem)
            {
                Debug.LogWarning("⚠️ Can't place food directly on grill! Use a pan!");
            }
        }
    }

    int GetEmptySpotIndex()
    {
        for (int i = 0; i < currentPans.Length; i++)
        {
            if (currentPans[i] == null)
                return i;
        }
        return -1;
    }

    public void RemovePan(int spotIndex)
    {
        if (spotIndex >= 0 && spotIndex < currentPans.Length)
        {
            currentPans[spotIndex] = null;
        }
    }
}
