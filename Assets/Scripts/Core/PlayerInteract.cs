using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening; // If using DOTween for fade
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("UI Prompt")]
    public CanvasGroup promptCanvasGroup; // Drag your InteractCanvas here
    public TextMeshProUGUI promptText;   // Drag your "E to ..." text here

    [Header("Raycast Settings")]
    public float maxDistance = 4f;
    public LayerMask interactLayer = ~LayerMask.GetMask("Player"); // Ignore Player layer

    private bool isPromptActive = false;
    private InputAction interactAction;

    void Awake()
    {
        // Setup Input Action (new Input System)
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.performed += ctx => TryInteract();
    }

    void OnEnable() => interactAction.Enable();
    void OnDisable() => interactAction.Disable();

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // DEBUG: Log ray start/end
        Debug.Log($"Raycast from camera: Range={maxDistance}, Hit={Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactLayer)}");

        if (Physics.Raycast(ray, maxDistance, interactLayer))
        {
            Debug.Log($"Hit object: {hit.collider.name}, Tag: {hit.collider.tag}");

            if (hit.collider.CompareTag("Interactable"))
            {
                Debug.Log("INTERACTABLE FOUND! Showing prompt...");
                if (!isPromptActive)
                {
                    ShowPrompt(hit.collider.name);
                    isPromptActive = true;
                }
                return;
            }
        }

        if (isPromptActive)
        {
            HidePrompt();
            isPromptActive = false;
        }
    }

    void ShowPrompt(string targetName)
    {
        string cleanName = targetName.Replace("(Clone)", "").Replace("Door", "").Trim();
        promptText.text = $"E to Enter {cleanName}";
        promptCanvasGroup.DOFade(1f, 0.3f); // Fade in
    }

    void HidePrompt()
    {
        promptCanvasGroup.DOFade(0f, 0.3f); // Fade out
    }

    void TryInteract()
    {
        if (!isPromptActive) return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactLayer))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                string doorName = hit.collider.name.Replace("(Clone)", "").Trim();
                Debug.Log($"Interacting with door: {doorName}");

                // Scene Transition
                switch (doorName)
                {
                    case "FryCookDoor":
                        SceneManager.LoadScene("KitchenScene");
                        break;
                    case "ShopDoor":
                        SceneManager.LoadScene("ShopScene");
                        break;
                    default:
                        Debug.Log($"No scene mapped for door: {doorName}");
                        break;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;

        // Draw the exact raycast line from camera center forward
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
    }
}