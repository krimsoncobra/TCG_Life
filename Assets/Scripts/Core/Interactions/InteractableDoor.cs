using UnityEngine;

public class InteractableDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private string doorName = "Kitchen";
    [SerializeField] private string sceneToLoad = "KitchenScene";

    [Header("Optional: Loading Screen")]
    [SerializeField] private bool showLoadingDelay = false;
    [SerializeField] private float loadingDelay = 1f;

    public string GetPromptText()
    {
        return $"E to Enter {doorName}";
    }

    public void Interact()
    {
        Debug.Log($"🚪 Opening door to {doorName}...");

        // Use GameManager to load scene (preserves data)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            // Fallback if no GameManager exists
            Debug.LogWarning("⚠️ No GameManager found! Data won't persist. Loading scene directly...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}