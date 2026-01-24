using UnityEngine;

/// <summary>
/// Plate Station - grab plates or put food on plates
/// EXACTLY MIMICS PanStation logic - simple and working!
/// </summary>
public class PlateStation : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GameObject platePrefab;
    public Transform spawnPoint;

    [Header("Inventory")]
    public int plateCount = 20;

    public string GetPromptText()
    {
        // Case 1: Holding food directly
        if (PlayerHands.Instance.IsHolding<FoodItem>())
        {
            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();
            return $"E to Put {food.GetItemName()} on Plate";
        }

        // Case 2: Holding pan with food
        if (PlayerHands.Instance.IsHolding<CookingPan>())
        {
            CookingPan pan = PlayerHands.Instance.GetHeldItem<CookingPan>();
            if (pan.currentFood != null)
            {
                return $"E to Transfer {pan.currentFood.GetItemName()} to Plate";
            }
        }

        // Case 3: Empty hands
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            return $"E to Grab Plate ({plateCount})";
        }

        return "Plate Station";
    }

    public void Interact()
    {
        // Case 1: Holding pan with food → transfer to plate
        if (PlayerHands.Instance.IsHolding<CookingPan>())
        {
            CookingPan pan = PlayerHands.Instance.GetHeldItem<CookingPan>();

            if (pan.currentFood != null && plateCount > 0)
            {
                GameObject plateObj = Instantiate(platePrefab, spawnPoint.position, spawnPoint.rotation);

                // Freeze physics IMMEDIATELY
                Rigidbody plateRb = plateObj.GetComponent<Rigidbody>();
                if (plateRb != null)
                {
                    plateRb.isKinematic = true;
                    plateRb.useGravity = false;
                    plateRb.linearVelocity = Vector3.zero;
                    plateRb.angularVelocity = Vector3.zero;
                }
                Collider plateCol = plateObj.GetComponent<Collider>();
                if (plateCol != null)
                {
                    plateCol.isTrigger = true;
                }

                BurgerPlate plate = plateObj.GetComponent<BurgerPlate>();

                if (plate != null)
                {
                    FoodItem removedFood = pan.RemoveFood();
                    GameObject foodObj = removedFood.gameObject;

                    if (foodObj != null)
                    {
                        foodObj.transform.localScale = Vector3.one;
                        plate.TryAddLayer(foodObj);

                        // Drop empty pan
                        IHoldable panHoldable = pan as IHoldable;
                        if (panHoldable != null)
                        {
                            panHoldable.OnDrop();
                        }

                        PlayerHands.Instance.currentItem = null;
                        PlayerHands.Instance.TryPickup(plateObj);

                        plateCount--;
                        Debug.Log($"🍽️ Transferred {foodObj.name} to plate!");
                    }
                }
                return;
            }
        }

        // Case 2: Holding food directly → put on plate
        if (PlayerHands.Instance.IsHolding<FoodItem>() && plateCount > 0)
        {
            FoodItem food = PlayerHands.Instance.GetHeldItem<FoodItem>();

            GameObject plateObj = Instantiate(platePrefab, spawnPoint.position, spawnPoint.rotation);

            // Freeze physics IMMEDIATELY
            Rigidbody plateRb = plateObj.GetComponent<Rigidbody>();
            if (plateRb != null)
            {
                plateRb.isKinematic = true;
                plateRb.useGravity = false;
                plateRb.linearVelocity = Vector3.zero;
                plateRb.angularVelocity = Vector3.zero;
            }
            Collider plateCol = plateObj.GetComponent<Collider>();
            if (plateCol != null)
            {
                plateCol.isTrigger = true;
            }

            BurgerPlate plate = plateObj.GetComponent<BurgerPlate>();

            if (plate != null)
            {
                GameObject foodObj = PlayerHands.Instance.currentItem;
                PlayerHands.Instance.currentItem = null;

                foodObj.transform.SetParent(null);
                foodObj.transform.localScale = Vector3.one;

                plate.TryAddLayer(foodObj);
                PlayerHands.Instance.TryPickup(plateObj);

                plateCount--;
                Debug.Log($"🍽️ Put {food.GetItemName()} on plate!");
            }
            return;
        }

        // Case 3: Empty hands → grab empty plate
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            if (plateCount <= 0)
            {
                Debug.LogWarning("⚠️ No plates left!");
                return;
            }

            GameObject plateObj = Instantiate(platePrefab, spawnPoint.position, spawnPoint.rotation);

            // Freeze physics IMMEDIATELY
            Rigidbody plateRb = plateObj.GetComponent<Rigidbody>();
            if (plateRb != null)
            {
                plateRb.isKinematic = true;
                plateRb.useGravity = false;
                plateRb.linearVelocity = Vector3.zero;
                plateRb.angularVelocity = Vector3.zero;
            }
            Collider plateCol = plateObj.GetComponent<Collider>();
            if (plateCol != null)
            {
                plateCol.isTrigger = true;
            }

            if (PlayerHands.Instance.TryPickup(plateObj))
            {
                plateCount--;
                Debug.Log($"🍽️ Grabbed empty plate! ({plateCount} remaining)");
            }
        }
    }
}