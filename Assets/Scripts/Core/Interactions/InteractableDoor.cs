using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractableDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private string doorName = "Kitchen";
    [SerializeField] private string sceneToLoad = "KitchenScene";

    public string GetPromptText()
    {
        return $"E to Enter {doorName}";
    }

    public void Interact()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}