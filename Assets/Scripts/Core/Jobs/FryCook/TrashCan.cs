using UnityEngine;

/// <summary>
/// Trash can - destroys burnt/unwanted food
/// </summary>
public class TrashCan : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable held = PlayerHands.Instance.currentItem;
            if (held is FoodItem)
                return "E to Trash Food";
        }

        return "Trash Can";
    }

    public void Interact()
    {
        if (PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable held = PlayerHands.Instance.currentItem;

            if (held is FoodItem food)
            {
                Debug.Log($"🗑️ Trashed {food.GetItemName()}");
                Destroy(PlayerHands.Instance.currentItemObject);
                PlayerHands.Instance.currentItem = null;
                PlayerHands.Instance.currentItemObject = null;
            }
        }
    }
}