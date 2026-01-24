using UnityEngine;

/// <summary>
/// Pan station - grab pans, put food in pans, OR transfer food from plate to pan
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
        // Case 1: Holding plate with food → Transfer to pan
        if (PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            BurgerPlate plate = PlayerHands.Instance.GetHeldItem<BurgerPlate>();
            if (plate.layers.Count > 0)
            {
                GameObject topLayer = plate.layers[plate.layers.Count - 1];
                FoodItem food = topLayer.GetComponent<FoodItem>();

                if (food != null)
                {
                    return $"E to Transfer {food.GetItemName()} to Pan";
                }
            }
        }

        // Case 2: Holding food directly → Put in pan
        if (PlayerHands.Instance.IsHolding<FoodItem>())
        {
            return "E to Put Food in Pan";
        }

        // Case 3: Empty hands → Grab pan
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Grab Pan ({panCount})";
        }

        return "Pan Station";
    }

    public void Interact()
    {
        // Case 1: Holding plate with food - transfer top layer to pan
        if (PlayerHands.Instance.IsHolding<BurgerPlate>())
        {
            BurgerPlate plate = PlayerHands.Instance.GetHeldItem<BurgerPlate>();

            if (plate.layers.Count > 0)
            {
                if (panCount <= 0)
                {
                    Debug.LogWarning("⚠️ No pans left!");
                    return;
                }

                GameObject topLayer = plate.layers[plate.layers.Count - 1];
                FoodItem food = topLayer.GetComponent<FoodItem>();

                if (food != null)
                {
                    // Store references
                    GameObject plateObj = PlayerHands.Instance.currentItem;

                    // Spawn pan
                    GameObject panObj = Instantiate(panPrefab, spawnPoint.position, spawnPoint.rotation);
                    CookingPan pan = panObj.GetComponent<CookingPan>();

                    if (pan != null)
                    {
                        // Remove food from plate
                        GameObject foodObj = plate.RemoveTopLayer();

                        // Clear hands
                        PlayerHands.Instance.currentItem = null;

                        // Drop the plate properly
                        plate.OnDrop();
                        plateObj.transform.position = spawnPoint.position + Vector3.right * 0.5f;

                        // Make absolutely sure plate is pickupable
                        Collider plateCol = plateObj.GetComponent<Collider>();
                        if (plateCol != null)
                        {
                            plateCol.enabled = true;
                            plateCol.isTrigger = false;
                        }

                        Rigidbody plateRb = plateObj.GetComponent<Rigidbody>();
                        if (plateRb != null)
                        {
                            plateRb.isKinematic = false;
                            plateRb.useGravity = true;
                        }

                        // Ensure mesh is visible
                        Renderer[] renderers = plateObj.GetComponentsInChildren<Renderer>();
                        foreach (Renderer r in renderers)
                        {
                            r.enabled = true;
                        }

                        // Reset scale (in case it got weird)
                        plateObj.transform.localScale = Vector3.one;

                        // Ensure tag is set
                        if (!plateObj.CompareTag("Interactable"))
                        {
                            plateObj.tag = "Interactable";
                        }

                        Debug.Log($"📍 Dropped plate at {plateObj.transform.position}, visible: {renderers.Length > 0}");

                        // Reset food transform
                        foodObj.transform.SetParent(null);
                        foodObj.transform.localScale = Vector3.one;

                        // Add food to pan
                        pan.TryAddFood(food);

                        // Pick up pan
                        PlayerHands.Instance.TryPickup(panObj);

                        panCount--;
                        Debug.Log($"🍳 Transferred {food.GetItemName()} from plate to pan!");
                    }

                    return;
                }
            }
        }

        // Case 2: Holding food directly - create pan with food in it
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
                // Get the food object before clearing hands
                GameObject foodObj = PlayerHands.Instance.currentItem;

                // Remove food from hands (but don't destroy it)
                PlayerHands.Instance.currentItem = null;

                // Reset food transform before adding to pan
                foodObj.transform.SetParent(null);
                foodObj.transform.localScale = Vector3.one;

                // Add food to pan (this will parent it correctly)
                pan.TryAddFood(food);

                // Pick up pan
                PlayerHands.Instance.TryPickup(panObj);

                panCount--;
                Debug.Log($"🍳 Put {food.GetItemName()} in pan!");
            }

            return;
        }

        // Case 3: Empty hands - grab a pan
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