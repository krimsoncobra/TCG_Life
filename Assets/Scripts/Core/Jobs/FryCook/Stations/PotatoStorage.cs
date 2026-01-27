using UnityEngine;

/// <summary>
/// Potato storage - spawns raw potatoes when interacted with
/// Similar to MeatFreezer
/// </summary>
public class PotatoStorage : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GameObject rawPotatoPrefab;
    public Transform spawnPoint;

    [Header("Inventory")]
    public int potatoCount = 999; // Unlimited for now

    public string GetPromptText()
    {
        if (PlayerHands.Instance.IsHoldingSomething())
            return "Hands Full!";

        return $"E to Grab Potato ({potatoCount})";
    }

    public void Interact()
    {
        // Can't grab if hands full
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            Debug.LogWarning("⚠️ Hands are full!");
            return;
        }

        if (potatoCount <= 0)
        {
            Debug.LogWarning("⚠️ No potatoes left!");
            return;
        }

        // Spawn potato and pick it up
        GameObject potato = Instantiate(rawPotatoPrefab, spawnPoint.position, spawnPoint.rotation);

        if (PlayerHands.Instance.TryPickup(potato))
        {
            potatoCount--;
            Debug.Log($"🥔 Grabbed potato! ({potatoCount} remaining)");
        }
    }
}