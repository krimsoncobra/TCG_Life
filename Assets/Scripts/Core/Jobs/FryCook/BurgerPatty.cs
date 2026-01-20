using UnityEngine;

/// <summary>
/// Burger patty (extends FoodItem for cooking logic)
/// </summary>
public class BurgerPatty : FoodItem
{
    [Header("Pickup Settings")]
    [Tooltip("Offset position when held in hand")]
    public Vector3 handOffset = Vector3.zero;

    [Tooltip("Rotation when held in hand")]
    public Vector3 handRotation = Vector3.zero;

    [Tooltip("Scale when held (1,1,1 = no change)")]
    public Vector3 handScale = Vector3.one;

    void Start()
    {
        foodName = "Burger Patty";
        currentState = CookingState.Prepared; // Ready to cook
        cookTime = 5f;
        burnTime = 3f;
        flipBonus = 3f;
    }

    // Override pickup to use custom offsets
    public new void OnPickup(Transform handPosition)
    {
        base.OnPickup(handPosition);

        // Apply custom positioning
        transform.localPosition = handOffset;
        transform.localRotation = Quaternion.Euler(handRotation);
    }
}