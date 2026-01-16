using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Only if you're using TextMeshPro

public class DoorTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string sceneToLoad = "RestaurantScene"; // ← Change to your exact scene name!
    [SerializeField] private GameObject interactionPrompt;           // Drag your UI Text here

    private bool playerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Optional: nice little sound/animation/effect here later

            // Load the scene!
            SceneManager.LoadScene(sceneToLoad);
            // Alternative: SceneManager.LoadSceneAsync(sceneToLoad); for loading screen later
        }
    }
}