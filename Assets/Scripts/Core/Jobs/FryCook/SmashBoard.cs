using UnityEngine;

/// <summary>
/// Smash board - converts raw meat to burger patty
/// </summary>
public class SmashBoard : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform meatPosition;  // Where meat sits on board

    private RawMeat currentMeat;

    public string GetPromptText()
    {
        // If holding meat, can place it
        if (PlayerHands.Instance.IsHolding<RawMeat>())
            return "E to Place Meat";

        // If meat on board, can smash it
        if (currentMeat != null)
            return "E to Smash Meat";

        return "Place Meat Here";
    }

    public void Interact()
    {
        // Case 1: Holding meat, place it on board
        if (PlayerHands.Instance.IsHolding<RawMeat>())
        {
            RawMeat meat = PlayerHands.Instance.GetHeldItem<RawMeat>();
            PlayerHands.Instance.TryPlaceAt(meatPosition);
            currentMeat = meat;
            Debug.Log("📍 Placed meat on smash board");
            return;
        }

        // Case 2: Meat on board, smash it
        if (currentMeat != null)
        {
            // Smash meat into patty
            GameObject pattyObj = currentMeat.SmashIntoPatty();
            currentMeat = null;

            // Wait one frame for patty to fully spawn, then pick it up
            if (pattyObj != null)
            {
                StartCoroutine(PickupAfterDelay(pattyObj));
            }
        }
    }

    System.Collections.IEnumerator PickupAfterDelay(GameObject patty)
    {
        yield return new WaitForEndOfFrame();

        if (patty != null && PlayerHands.Instance != null)
        {
            bool pickedUp = PlayerHands.Instance.TryPickup(patty);

            if (pickedUp)
            {
                Debug.Log("✅ Auto-picked up patty!");
            }
            else
            {
                Debug.LogWarning("⚠️ Failed to auto-pickup patty - hands might be full");
            }
        }
    }
}
