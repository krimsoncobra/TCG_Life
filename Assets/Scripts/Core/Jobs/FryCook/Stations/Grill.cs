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
        // Check if looking at a pan on the grill (raycast hit the pan, not the grill)
        // The interaction system will handle this automatically if the pan has a collider

        // Check if holding a pan
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable heldItem = PlayerHands.Instance.currentItem?.GetComponent<IHoldable>();

            if (heldItem is CookingPan)
            {
                // Find empty spot
                if (GetEmptySpotIndex() >= 0)
                    return "E to Place Pan on Grill";
                else
                    return "Grill Full!";
            }
            else if (heldItem is FoodItem)
            {
                return "Put Food in Pan First!";
            }
        }

        // If not holding anything, check if there are pans to pick up
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            // Try to find a pan on the grill
            CookingPan nearestPan = FindNearestPan();
            if (nearestPan != null)
            {
                return "E to Pick Up Pan";
            }
        }

        return "Grill";
    }

    public void Interact()
    {
        // Priority 1: If not holding anything, try to pick up a pan from grill
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            CookingPan nearestPan = FindNearestPan();

            if (nearestPan != null)
            {
                // Find which spot it's in
                int spotIndex = FindPanSpotIndex(nearestPan);

                if (spotIndex >= 0)
                {
                    // Remove from spot tracking
                    currentPans[spotIndex] = null;
                    nearestPan.isOnGrill = false;

                    // Pick it up
                    if (PlayerHands.Instance.TryPickup(nearestPan.gameObject))
                    {
                        Debug.Log($"✅ Picked up pan from grill spot {spotIndex}");
                    }
                }

                return;
            }
        }

        // Priority 2: Place pan on grill
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable heldItem = PlayerHands.Instance.currentItem?.GetComponent<IHoldable>();

            if (heldItem is CookingPan pan)
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
                else
                {
                    Debug.LogWarning("⚠️ Grill is full!");
                }
            }
            else if (heldItem is FoodItem)
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

    CookingPan FindNearestPan()
    {
        // Find closest pan to player
        float closestDist = float.MaxValue;
        CookingPan closest = null;

        foreach (CookingPan pan in currentPans)
        {
            if (pan != null)
            {
                float dist = Vector3.Distance(pan.transform.position, PlayerHands.Instance.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = pan;
                }
            }
        }

        return closest;
    }

    int FindPanSpotIndex(CookingPan pan)
    {
        for (int i = 0; i < currentPans.Length; i++)
        {
            if (currentPans[i] == pan)
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