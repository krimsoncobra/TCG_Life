using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "Meat";
    [SerializeField] private int value = 10;

    public string GetPromptText()
    {
        return $"E to Pick Up {itemName}";
    }

    public void Interact()
    {
        Debug.Log($"Picked up {itemName} worth ${value}");
        // TODO: Add to inventory system later
        Destroy(gameObject);
    }
}