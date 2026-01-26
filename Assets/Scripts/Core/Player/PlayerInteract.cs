using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using System.Linq;

public class PlayerInteract : MonoBehaviour
{
    [Header("UI Prompt")]
    public CanvasGroup promptCanvasGroup;
    public TextMeshProUGUI promptText;

    [Header("Raycast Settings")]
    public float maxDistance = 4f;
    public LayerMask interactLayer;

    [Header("Swap Settings")]
    [Tooltip("Distance in front of player to drop swapped items")]
    public float swapDropDistance = 1.5f;
    public float swapDropHeight = 0.5f;

    [Header("Debug")]
    public bool showDebugRays = true;
    public bool showDebugLogs = false;

    private bool isPromptActive = false;
    private InputAction interactAction;
    private IInteractable currentInteractable;
    private GameObject currentLookedAtObject;

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

        // Perform raycast - now gets ALL hits (not just first)
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, interactLayer);

        // Sort hits by distance (closest first)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Debug logging
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            if (hits.Length > 0)
            {
                Debug.Log($"🎯 RAYCAST HIT {hits.Length} objects");
            }
        }

        // Find first valid interactable (skip player and child objects of held items)
        RaycastHit? validHit = null;
        foreach (RaycastHit hit in hits)
        {
            // Skip player objects
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player") ||
                hit.collider.CompareTag("Player"))
            {
                if (showDebugLogs && Time.frameCount % 60 == 0)
                    Debug.Log($"⏭️ Skipping player object: {hit.collider.name}");
                continue;
            }

            // Skip if this is a child of what we're holding
            if (PlayerHands.Instance != null &&
                PlayerHands.Instance.IsHoldingSomething() &&
                hit.collider.transform.IsChildOf(PlayerHands.Instance.currentItem.transform))
            {
                if (showDebugLogs && Time.frameCount % 60 == 0)
                    Debug.Log($"⏭️ Skipping child of held item: {hit.collider.name}");
                continue;
            }

            // Found a valid hit!
            validHit = hit;
            break;
        }

        if (validHit.HasValue)
        {
            RaycastHit hit = validHit.Value;

            // Draw green line to hit point
            if (showDebugRays)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }

            // Store the object we're looking at
            currentLookedAtObject = hit.collider.gameObject;

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

                    // Get prompt text - will show swap prompt if applicable
                    string promptText = GetContextualPromptText(interactable, hit.collider.gameObject);

                    if (!isPromptActive)
                    {
                        if (showDebugLogs)
                            Debug.Log($"🔔 Activating prompt for first time!");

                        ShowPrompt(promptText);
                        isPromptActive = true;
                    }
                    else
                    {
                        // Update prompt text dynamically
                        if (this.promptText != null)
                        {
                            this.promptText.text = promptText;
                        }
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
            currentLookedAtObject = null;
        }
    }

    string GetContextualPromptText(IInteractable interactable, GameObject targetObject)
    {
        // Check if we're holding something and looking at another holdable
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHoldingSomething())
        {
            IHoldable lookedAtHoldable = targetObject.GetComponent<IHoldable>();

            if (lookedAtHoldable != null)
            {
                // CRITICAL CHECK: Don't allow swapping with yourself or your child objects!
                GameObject currentHeldItem = PlayerHands.Instance.currentItem;

                // Check if target is the same object we're holding
                if (targetObject == currentHeldItem)
                {
                    if (showDebugLogs)
                        Debug.Log("⚠️ Target is the item we're holding - no swap");
                    return interactable.GetPromptText();
                }

                // Check if target is a child of what we're holding
                if (targetObject.transform.IsChildOf(currentHeldItem.transform))
                {
                    if (showDebugLogs)
                        Debug.Log("⚠️ Target is a child of held item - no swap");
                    return interactable.GetPromptText();
                }

                // Check if we're holding a child of the target
                if (currentHeldItem.transform.IsChildOf(targetObject.transform))
                {
                    if (showDebugLogs)
                        Debug.Log("⚠️ Held item is a child of target - no swap");
                    return interactable.GetPromptText();
                }

                // Valid swap target - show swap prompt
                IHoldable currentHoldable = currentHeldItem?.GetComponent<IHoldable>();
                string currentItemName = currentHoldable?.GetItemName() ?? "Item";
                string targetItemName = lookedAtHoldable.GetItemName();

                return $"E to Swap {currentItemName} for {targetItemName}";
            }
        }

        // Default to regular prompt
        return interactable.GetPromptText();
    }

    void ShowPrompt(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
            if (showDebugLogs)
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
        }
        else
        {
            Debug.LogError("❌ Prompt Canvas Group is NULL!");
        }
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

        // Check if this is a swap situation
        if (PlayerHands.Instance != null &&
            PlayerHands.Instance.IsHoldingSomething() &&
            currentLookedAtObject != null)
        {
            IHoldable lookedAtHoldable = currentLookedAtObject.GetComponent<IHoldable>();

            if (lookedAtHoldable != null && lookedAtHoldable.CanPickup())
            {
                GameObject currentHeldItem = PlayerHands.Instance.currentItem;

                // CRITICAL: Don't swap with yourself or child objects!
                bool isSameObject = currentLookedAtObject == currentHeldItem;
                bool isChildOfHeld = currentLookedAtObject.transform.IsChildOf(currentHeldItem.transform);
                bool isParentOfHeld = currentHeldItem.transform.IsChildOf(currentLookedAtObject.transform);

                if (isSameObject || isChildOfHeld || isParentOfHeld)
                {
                    if (showDebugLogs)
                        Debug.Log("⚠️ Cannot swap with self or child objects - running normal interaction");
                    // Run normal interaction instead
                    currentInteractable.Interact();
                    return;
                }

                // Valid swap - perform it
                PerformItemSwap(currentLookedAtObject);
                return;
            }
        }

        // Regular interaction
        currentInteractable.Interact();
    }

    void PerformItemSwap(GameObject newItem)
    {
        if (PlayerHands.Instance == null) return;

        // Get current item
        GameObject currentItem = PlayerHands.Instance.currentItem;
        IHoldable currentHoldable = currentItem?.GetComponent<IHoldable>();

        if (currentHoldable == null)
        {
            Debug.LogWarning("⚠️ Current item is not IHoldable!");
            return;
        }

        Debug.Log($"🔄 Swapping {currentHoldable.GetItemName()} for {newItem.name}");

        // Step 1: Drop current item in front of player
        Vector3 dropPosition = CalculateDropPosition();

        currentHoldable.OnDrop();
        currentItem.transform.position = dropPosition;

        // Make sure dropped item is interactable
        Collider col = currentItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        if (!currentItem.CompareTag("Interactable"))
        {
            currentItem.tag = "Interactable";
        }

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Step 2: Clear hands
        PlayerHands.Instance.currentItem = null;

        // Step 3: Pick up new item
        bool pickedUp = PlayerHands.Instance.TryPickup(newItem);

        if (pickedUp)
        {
            Debug.Log($"✅ Successfully swapped items!");
        }
        else
        {
            Debug.LogWarning("⚠️ Failed to pick up new item after dropping old one");
        }
    }

    Vector3 CalculateDropPosition()
    {
        // Drop in front of player
        Vector3 forwardPosition = transform.position + transform.forward * swapDropDistance;
        forwardPosition.y = transform.position.y + swapDropHeight;
        return forwardPosition;
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