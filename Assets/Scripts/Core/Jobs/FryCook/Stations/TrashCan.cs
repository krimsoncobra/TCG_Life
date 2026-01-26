using UnityEngine;

/// <summary>
/// Trash can - destroys burnt/unwanted food
/// Also handles burnt pan disposal, spawning a clean pan
/// </summary>
public class TrashCan : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable heldItem = PlayerHands.Instance.currentItem?.GetComponent<IHoldable>();

            // Check for burnt pan disposal
            BurntPanDisposal burntPan = PlayerHands.Instance.currentItem?.GetComponent<BurntPanDisposal>();
            if (burntPan != null)
            {
                return "E to Trash Burnt Burger (Get Clean Pan)";
            }

            if (heldItem is FoodItem)
            {
                return "E to Trash Food";
            }

            // Check for any cooking pan
            if (heldItem is CookingPan)
            {
                return "E to Trash Pan";
            }
        }

        return "Trash Can";
    }

    public void Interact()
    {
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable heldItem = PlayerHands.Instance.currentItem?.GetComponent<IHoldable>();

            // Check for burnt pan disposal first
            BurntPanDisposal burntPan = PlayerHands.Instance.currentItem?.GetComponent<BurntPanDisposal>();
            if (burntPan != null)
            {
                Debug.Log($"🗑️ Trashing burnt pan - will spawn clean pan");

                // Clear from player hands first
                GameObject burntPanObject = PlayerHands.Instance.currentItem;
                PlayerHands.Instance.currentItem = null;

                // Call the disposal which will spawn clean pan
                burntPan.OnDisposed();
                return;
            }

            // Handle regular food items
            if (heldItem is FoodItem food)
            {
                Debug.Log($"🗑️ Trashed {food.GetItemName()}");
                Destroy(PlayerHands.Instance.currentItem);
                PlayerHands.Instance.currentItem = null;
                return;
            }

            // Handle regular cooking pans (non-burnt)
            if (heldItem is CookingPan)
            {
                Debug.Log($"🗑️ Trashed cooking pan");
                Destroy(PlayerHands.Instance.currentItem);
                PlayerHands.Instance.currentItem = null;
                return;
            }
        }
    }
}