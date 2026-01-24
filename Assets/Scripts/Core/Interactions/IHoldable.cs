using UnityEngine;

/// <summary>
/// Interface for any object that can be picked up and held by the player
/// Implement this on items like pans, food, tools, etc.
/// </summary>
public interface IHoldable
{
    /// <summary>
    /// Check if the item can currently be picked up
    /// </summary>
    bool CanPickup();

    /// <summary>
    /// Called when the player picks up this item
    /// </summary>
    /// <param name="handPosition">The transform of the player's hand</param>
    void OnPickup(Transform handPosition);

    /// <summary>
    /// Called when the player drops this item
    /// </summary>
    void OnDrop();

    /// <summary>
    /// Called when the player places this item at a specific location (e.g. on a grill)
    /// </summary>
    /// <param name="targetPosition">Where to place the item</param>
    void OnPlaceAt(Transform targetPosition);

    /// <summary>
    /// Get the name of this item (for UI display)
    /// </summary>
    string GetItemName();
}