using UnityEngine;


/// <summary>
/// Pan station - grab pans or put food in pans
/// </summary>
public class PanStation : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GameObject panPrefab;
    public Transform spawnPoint;

    [Header("Inventory")]
    public int panCount = 10;

    public string GetPromptText()
    {
        // If holding food, can put it in a pan
        if (PlayerHands.Instance.IsHolding<FoodItem>())
        {
            return "E to Put Food in Pan";
        }

        // If holding nothing, can grab a pan
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Grab Pan ({panCount})";
        }

        return "Pan Station";
    }

    public void Interact()
    {
        // Case 1: Holding food - create pan with food in it
        if (PlayerHands.Instance.IsHolding<FoodItem>())
        {
            if (panCount <= 0)
            {
                Debug.LogWarning("⚠️ No pans left!");
                return;
            }

            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();

            // Spawn pan
            GameObject panObj = Instantiate(panPrefab, spawnPoint.position, spawnPoint.rotation);
            CookingPan pan = panObj.GetComponent<CookingPan>();

            if (pan != null)
            {
                // Remove food from hands
                PlayerHands.Instance.currentItem = null;
                PlayerHands.Instance.currentItemObject = null;

                // Add food to pan
                pan.TryAddFood(food);

                // Pick up pan
                PlayerHands.Instance.TryPickup(panObj);

                panCount--;
                Debug.Log($"🍳 Put {food.GetItemName()} in pan!");
            }

            return;
        }

        // Case 2: Empty hands - grab a pan
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            if (panCount <= 0)
            {
                Debug.LogWarning("⚠️ No pans left!");
                return;
            }

            GameObject pan = Instantiate(panPrefab, spawnPoint.position, spawnPoint.rotation);

            if (PlayerHands.Instance.TryPickup(pan))
            {
                panCount--;
                Debug.Log($"🍳 Grabbed pan! ({panCount} remaining)");
            }
        }
    }
}