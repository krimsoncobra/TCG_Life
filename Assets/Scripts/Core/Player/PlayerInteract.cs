using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("UI Prompt")]
    public CanvasGroup promptCanvasGroup;
    public TextMeshProUGUI promptText;

    [Header("Raycast Settings")]
    public float maxDistance = 4f;

    private LayerMask interactLayer;
    private bool isPromptActive = false;
    private InputAction interactAction;
    private IInteractable currentInteractable; // Cache for performance

    void Awake()
    {
        interactLayer = ~LayerMask.GetMask("Player");

        // Setup Input Action
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

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactLayer))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                // Get interactable component (doors, items, NPCs, etc)
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    currentInteractable = interactable;

                    if (!isPromptActive)
                    {
                        ShowPrompt(interactable.GetPromptText());
                        isPromptActive = true;
                    }
                    return;
                }
            }
        }

        // Nothing in range
        if (isPromptActive)
        {
            HidePrompt();
            isPromptActive = false;
            currentInteractable = null;
        }
    }

    void ShowPrompt(string text)
    {
        promptText.text = text;
        promptCanvasGroup.DOFade(1f, 0.2f);
    }

    void HidePrompt()
    {
        promptCanvasGroup.DOFade(0f, 0.2f);
    }

    void TryInteract()
    {
        if (!isPromptActive || currentInteractable == null) return;

        currentInteractable.Interact();
        Debug.Log($"Interacted with: {currentInteractable}");
    }

    void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
    }
}
