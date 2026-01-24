using UnityEngine;

/// <summary>
/// Enhanced Trash Can - destroys food, plates, pans, and individual layers
/// Works layer-by-layer for both plates and pans
/// </summary>
public class TrashCan : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            return "Trash Can";
        }

        GameObject heldItem = PlayerHands.Instance.currentItem;

        // Case 1: Holding plate
        if (heldItem.GetComponent<BurgerPlate>() != null)
        {
            BurgerPlate plate = heldItem.GetComponent<BurgerPlate>();

            if (plate.layers.Count > 0)
            {
                GameObject topLayer = plate.layers[plate.layers.Count - 1];
                return $"E to Remove {GetLayerName(topLayer)}";
            }
            else
            {
                return "E to Trash Empty Plate";
            }
        }

        // Case 2: Holding pan
        if (heldItem.GetComponent<CookingPan>() != null)
        {
            CookingPan pan = heldItem.GetComponent<CookingPan>();

            if (pan.currentFood != null)
            {
                return $"E to Remove {pan.currentFood.GetItemName()}";
            }
            else
            {
                return "E to Trash Empty Pan";
            }
        }

        // Case 3: Holding food directly
        if (heldItem.GetComponent<FoodItem>() != null)
        {
            FoodItem food = heldItem.GetComponent<FoodItem>();
            return $"E to Trash {food.GetItemName()}";
        }

        // Case 4: Generic item
        return "E to Trash Item";
    }

    public void Interact()
    {
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            return;
        }

        GameObject heldItem = PlayerHands.Instance.currentItem;

        // Case 1: Trashing a burger plate
        if (heldItem.GetComponent<BurgerPlate>() != null)
        {
            BurgerPlate plate = heldItem.GetComponent<BurgerPlate>();

            // If plate has layers, remove top layer first
            if (plate.layers.Count > 0)
            {
                GameObject topLayer = plate.RemoveTopLayer();
                string layerName = GetLayerName(topLayer);

                Destroy(topLayer);
                Debug.Log($"🗑️ Removed and trashed: {layerName}");

                // Plate still in hand, just lighter now
                return;
            }
            else
            {
                // Empty plate - trash the whole thing
                Destroy(heldItem);
                PlayerHands.Instance.currentItem = null;
                Debug.Log("🗑️ Trashed empty plate");
                return;
            }
        }

        // Case 2: Trashing a cooking pan
        if (heldItem.GetComponent<CookingPan>() != null)
        {
            CookingPan pan = heldItem.GetComponent<CookingPan>();

            // If pan has food, remove it first
            if (pan.currentFood != null)
            {
                string foodName = pan.currentFood.GetItemName();
                GameObject foodObj = pan.currentFood.gameObject;

                pan.RemoveFood(); // Remove from pan

                Destroy(foodObj);
                Debug.Log($"🗑️ Removed and trashed: {foodName}");

                // Pan still in hand, now empty
                return;
            }
            else
            {
                // Empty pan - trash the whole thing
                Destroy(heldItem);
                PlayerHands.Instance.currentItem = null;
                Debug.Log("🗑️ Trashed empty pan");
                return;
            }
        }

        // Case 3: Trashing food item directly
        if (heldItem.GetComponent<FoodItem>() != null)
        {
            FoodItem food = heldItem.GetComponent<FoodItem>();
            string foodName = food.GetItemName();

            Destroy(heldItem);
            PlayerHands.Instance.currentItem = null;
            Debug.Log($"🗑️ Trashed {foodName}");
            return;
        }

        // Case 4: Generic trash
        Debug.Log($"🗑️ Trashed {heldItem.name}");
        Destroy(heldItem);
        PlayerHands.Instance.currentItem = null;
    }

    string GetLayerName(GameObject layer)
    {
        if (layer.GetComponent<BottomBun>() != null)
            return "Bottom Bun";
        if (layer.GetComponent<TopBun>() != null)
            return "Top Bun";
        if (layer.GetComponent<FoodItem>() != null)
            return layer.GetComponent<FoodItem>().GetItemName();

        return layer.name;
    }
}