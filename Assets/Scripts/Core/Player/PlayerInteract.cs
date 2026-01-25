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
    public LayerMask interactLayer;

    [Header("Debug")]
    public bool showDebugRays = true;
    public bool showDebugLogs = false;

    private bool isPromptActive = false;
    private InputAction interactAction;
    private IInteractable currentInteractable;

    void Awake()
    {
        // Set up layer mask - interact with everything EXCEPT Player layer
        interactLayer = ~LayerMask.GetMask("Player");

        // Setup Input Action
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.performed += ctx => TryInteract();
    }

    void Start()
    {
        // Verify camera exists
        if (Camera.main == null)
        {
            Debug.LogError("⚠️ PlayerInteract: No camera tagged 'MainCamera' found! Interaction won't work!");
        }
        else
        {
            Debug.Log($"✓ PlayerInteract: Found camera '{Camera.main.name}'");
        }

        // Hide prompt initially
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }
    }

    void OnEnable() => interactAction.Enable();
    void OnDisable() => interactAction.Disable();

    void Update()
    {
        CheckForInteractable();

        // Check for F key press (flip minigame for CookingPan)
        // Use the already-detected interactable instead of a fresh raycast
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (currentInteractable != null)
            {
                // Try to get CookingPan from the current interactable
                CookingPan pan = (currentInteractable as MonoBehaviour)?.GetComponent<CookingPan>();

                if (pan != null)
                {
                    Debug.Log($"🔑 F pressed while looking at {pan.gameObject.name}");

                    if (pan.CanFlip())
                    {
                        Debug.Log("✅ CanFlip returned true, starting minigame...");
                        pan.TryStartFlipMinigame();
                    }
                    else
                    {
                        Debug.Log("❌ CanFlip returned false - check conditions");
                    }
                }
                else
                {
                    Debug.Log("🔑 F pressed but not looking at a CookingPan");
                }
            }
            else
            {
                Debug.Log("🔑 F pressed but no interactable detected");
            }
        }
    }

    void CheckForInteractable()
    {
        // Always get fresh camera reference (for Cinemachine compatibility)
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("⚠️ PlayerInteract: No camera tagged 'MainCamera'!");
            return;
        }

        // Create ray from screen center using CURRENT camera
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        // Debug visualization
        if (showDebugRays)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan);
        }

        // Perform raycast
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactLayer);

        // Debug logging
        if (showDebugLogs && Time.frameCount % 60 == 0) // Only log every 60 frames to avoid spam
        {
            if (hitSomething)
            {
                Debug.Log($"🎯 RAYCAST HIT: {hit.collider.name} | Tag: {hit.collider.tag} | Dist: {hit.distance:F2}m");
            }
        }

        if (hitSomething)
        {
            // Draw green line to hit point
            if (showDebugRays)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }

            // Check if it's interactable
            if (hit.collider.CompareTag("Interactable"))
            {
                if (showDebugLogs)
                    Debug.Log($"🎯 Hit interactable: {hit.collider.name}");

                // Try to get IInteractable component
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    currentInteractable = interactable;

                    if (!isPromptActive)
                    {
                        if (showDebugLogs)
                            Debug.Log($"🔔 Activating prompt for first time!");

                        ShowPrompt(interactable.GetPromptText());
                        isPromptActive = true;
                    }
                    return;
                }
                else
                {
                    Debug.LogWarning($"⚠️ {hit.collider.name} is tagged 'Interactable' but has no IInteractable component!");
                }
            }
        }

        // Nothing in range - hide prompt
        if (isPromptActive)
        {
            HidePrompt();
            isPromptActive = false;
            currentInteractable = null;
        }
    }

    void ShowPrompt(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
            Debug.Log($"✅ Set prompt text to: {text}");
        }
        else
        {
            Debug.LogError("❌ Prompt Text is NULL!");
        }

        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.DOFade(1f, 0.2f);
            promptCanvasGroup.interactable = true;
            promptCanvasGroup.blocksRaycasts = true;
            Debug.Log($"✅ Fading prompt to Alpha 1 (currently: {promptCanvasGroup.alpha})");
        }
        else
        {
            Debug.LogError("❌ Prompt Canvas Group is NULL!");
        }

        if (showDebugLogs)
            Debug.Log($"📝 Showing prompt: {text}");
    }

    void HidePrompt()
    {
        if (promptCanvasGroup != null)
            promptCanvasGroup.DOFade(0f, 0.2f);
    }

    void TryInteract()
    {
        if (!isPromptActive || currentInteractable == null) return;

        if (showDebugLogs)
            Debug.Log($"🔧 Interacting with: {currentInteractable}");

        currentInteractable.Interact();
    }

    // Draw gizmo in Scene view to show raycast range
    void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);

        // Draw sphere at max distance
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(ray.origin + ray.direction * maxDistance, 0.2f);
    }
}