using UnityEngine;

/// <summary>
/// Cutting station - cuts potatoes into fries
/// Similar to SmashBoard
/// </summary>
public class CuttingStation : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform cuttingPosition; // Where potato sits on board

    private Potato currentPotato;

    public string GetPromptText()
    {
        // If holding potato, can place it
        if (PlayerHands.Instance.IsHolding<Potato>())
            return "E to Place Potato";

        // If potato on board, can cut it
        if (currentPotato != null)
            return "E to Cut Potato into Fries";

        return "Place Potato Here";
    }

    public void Interact()
    {
        // Case 1: Holding potato, place it on board
        if (PlayerHands.Instance.IsHolding<Potato>())
        {
            Potato potato = PlayerHands.Instance.GetHeldItem<Potato>();
            PlayerHands.Instance.TryPlaceAt(cuttingPosition);
            currentPotato = potato;
            Debug.Log("📍 Placed potato on cutting station");
            return;
        }

        // Case 2: Potato on board, cut it
        if (currentPotato != null)
        {
            // Cut potato into fries
            GameObject friesObj = currentPotato.CutIntoFries();
            currentPotato = null;

            // Wait one frame for fries to fully spawn, then pick it up
            if (friesObj != null)
            {
                StartCoroutine(PickupAfterDelay(friesObj));
            }
        }
    }

    System.Collections.IEnumerator PickupAfterDelay(GameObject fries)
    {
        yield return new WaitForEndOfFrame();

        if (fries != null && PlayerHands.Instance != null)
        {
            bool pickedUp = PlayerHands.Instance.TryPickup(fries);

            if (pickedUp)
            {
                Debug.Log("✅ Auto-picked up fries!");
            }
            else
            {
                Debug.LogWarning("⚠️ Failed to auto-pickup fries - hands might be full");
            }
        }
    }
}