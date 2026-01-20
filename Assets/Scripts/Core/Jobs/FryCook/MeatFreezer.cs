using UnityEngine;

/// <summary>
/// Meat freezer - spawns raw meat when interacted with
/// </summary>
public class MeatFreezer : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GameObject rawMeatPrefab;
    public Transform spawnPoint;  // Where meat appears when picked up

    [Header("Inventory")]
    public int meatCount = 999;  // Unlimited for now

    public string GetPromptText()
    {
        if (PlayerHands.Instance.IsHoldingSomething())
            return "Hands Full!";

        return $"E to Grab Meat ({meatCount})";
    }

    public void Interact()
    {
        // Can't grab if hands full
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            Debug.LogWarning("⚠️ Hands are full!");
            return;
        }

        if (meatCount <= 0)
        {
            Debug.LogWarning("⚠️ No meat left!");
            return;
        }

        // Spawn meat and pick it up
        GameObject meat = Instantiate(rawMeatPrefab, spawnPoint.position, spawnPoint.rotation);

        if (PlayerHands.Instance.TryPickup(meat))
        {
            meatCount--;
            Debug.Log($"🥩 Grabbed meat! ({meatCount} remaining)");
        }
    }
}
