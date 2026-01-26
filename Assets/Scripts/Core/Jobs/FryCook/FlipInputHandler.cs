using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dedicated handler for flip input - attach to Player
/// Checks for CookingPan on hit object AND children
/// </summary>
public class FlipInputHandler : MonoBehaviour
{
    [Header("Settings")]
    public float rayDistance = 4f;
    public LayerMask interactLayer;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Camera cam;
    private InputAction flipAction;
    private CookingPan lastDetectedPan;
    private float lastCheckTime;

    void Awake()
    {
        Debug.Log("═══════════════════════════════════════════════");
        Debug.Log("🎮 FlipInputHandler AWAKE");
        Debug.Log("═══════════════════════════════════════════════");

        // Setup F key input
        flipAction = new InputAction("Flip", InputActionType.Button);
        flipAction.AddBinding("<Keyboard>/f");
        flipAction.performed += ctx => TryFlip();

        Debug.Log("✅ F key input action configured");

        // Set up layer mask
        interactLayer = ~LayerMask.GetMask("Player");
    }

    void OnEnable()
    {
        flipAction?.Enable();
        Debug.Log("✅ FlipInputHandler ENABLED - F key listener active");
    }

    void OnDisable()
    {
        flipAction?.Disable();
        Debug.Log("⚠️ FlipInputHandler DISABLED");
    }

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("❌ FlipInputHandler: No Main Camera found!");
        }
        else
        {
            Debug.Log($"✅ FlipInputHandler: Camera found - {cam.name}");
        }
    }

    void Update()
    {
        // Continuously check what we're looking at
        CheckForFlippablePan();

        // ALSO check for F key manually as backup
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("🔑 F KEY DETECTED IN UPDATE (backup check)");
            TryFlip();
        }
    }

    void CheckForFlippablePan()
    {
        if (cam == null) return;

        // Raycast from screen center
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactLayer))
        {
            // IMPROVED: Check hit object AND its children for CookingPan
            CookingPan pan = hit.collider.GetComponent<CookingPan>();

            if (pan == null)
            {
                // Check children (pan might be child of grill)
                pan = hit.collider.GetComponentInChildren<CookingPan>();
            }

            if (pan != null)
            {
                lastDetectedPan = pan;

                // Debug log every second (not every frame)
                if (showDebugLogs && Time.time - lastCheckTime > 1f)
                {
                    bool canFlip = pan.CanFlip();
                    if (canFlip)
                    {
                        Debug.Log($"✅ Looking at flippable pan: {pan.name} - Press F to flip!");
                    }
                    else
                    {
                        // Show why we can't flip
                        string reason = "";
                        if (pan.currentFood == null)
                            reason = "No food";
                        else if (!pan.isOnGrill)
                            reason = "Not on grill";
                        else if (pan.currentFood.hasBeenFlipped)
                            reason = "Already flipped";
                        else if (pan.currentFood.currentState != CookingState.Cooking)
                            reason = $"Wrong state: {pan.currentFood.currentState}";
                        else if (PlayerHands.Instance != null && PlayerHands.Instance.IsHoldingSomething())
                            reason = "Hands not empty";

                        Debug.Log($"❌ Looking at pan but can't flip: {reason}");
                    }
                    lastCheckTime = Time.time;
                }
            }
            else
            {
                lastDetectedPan = null;
            }
        }
        else
        {
            lastDetectedPan = null;
        }
    }

    void TryFlip()
    {
        Debug.Log("═══════════════════════════════════════════════");
        Debug.Log("🔑 F KEY PRESSED! (TryFlip called)");
        Debug.Log($"⏰ Time: {Time.time:F2}");
        Debug.Log("═══════════════════════════════════════════════");

        if (lastDetectedPan == null)
        {
            Debug.Log("❌ lastDetectedPan is NULL - not looking at any pan");

            // Try manual raycast as fallback
            if (cam != null)
            {
                Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                Ray ray = cam.ScreenPointToRay(screenCenter);

                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactLayer))
                {
                    Debug.Log($"📍 Manual raycast hit: {hit.collider.name}");

                    // IMPROVED: Check both component and children
                    CookingPan pan = hit.collider.GetComponent<CookingPan>();
                    if (pan == null)
                    {
                        Debug.Log("   Checking children for CookingPan...");
                        pan = hit.collider.GetComponentInChildren<CookingPan>();
                    }

                    if (pan != null)
                    {
                        Debug.Log($"✅ Found pan via manual raycast: {pan.name}");
                        lastDetectedPan = pan;
                    }
                    else
                    {
                        Debug.Log("❌ No CookingPan found on hit object or its children");

                        // DEBUG: List all children
                        Transform[] children = hit.collider.GetComponentsInChildren<Transform>();
                        Debug.Log($"   Hit object has {children.Length} children:");
                        foreach (Transform child in children)
                        {
                            Component[] components = child.GetComponents<Component>();
                            string componentNames = string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name));
                            Debug.Log($"      - {child.name} (components: {componentNames})");
                        }
                    }
                }
                else
                {
                    Debug.Log("❌ Manual raycast missed - nothing in front of camera");
                }
            }

            if (lastDetectedPan == null)
            {
                Debug.Log("═══════════════════════════════════════════════");
                return;
            }
        }

        Debug.Log($"📍 Looking at pan: {lastDetectedPan.name}");

        // Detailed diagnostic
        bool hasFood = lastDetectedPan.currentFood != null;
        bool onGrill = lastDetectedPan.isOnGrill;
        bool hasBeenFlipped = hasFood && lastDetectedPan.currentFood.hasBeenFlipped;
        bool isCooking = hasFood && lastDetectedPan.currentFood.currentState == CookingState.Cooking;
        bool handsEmpty = PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething();

        Debug.Log($"📊 Pan Status:");
        Debug.Log($"   Has Food: {hasFood}");
        Debug.Log($"   On Grill: {onGrill}");
        Debug.Log($"   Already Flipped: {hasBeenFlipped}");
        Debug.Log($"   Is Cooking: {isCooking}");
        Debug.Log($"   Hands Empty: {handsEmpty}");

        if (hasFood)
        {
            Debug.Log($"   Food State: {lastDetectedPan.currentFood.currentState}");
            Debug.Log($"   Cook Progress: {lastDetectedPan.currentFood.GetCookProgress() * 100f:F1}%");
            Debug.Log($"   Food Object: {lastDetectedPan.currentFood.name}");
        }

        // Check if can flip
        bool canFlip = lastDetectedPan.CanFlip();
        Debug.Log($"🎯 CanFlip() Result: {canFlip}");

        if (canFlip)
        {
            Debug.Log("✅✅✅ ALL CONDITIONS MET - Starting flip minigame...");
            lastDetectedPan.TryStartFlipMinigame();
            Debug.Log("✅✅✅ TryStartFlipMinigame() called");
        }
        else
        {
            Debug.Log("❌ Cannot flip - conditions not met");

            // Provide helpful feedback
            if (!hasFood)
            {
                Debug.Log("   ❌ No food in pan");
            }
            else if (!onGrill)
            {
                Debug.Log("   ❌ Pan not on grill");
            }
            else if (hasBeenFlipped)
            {
                Debug.Log("   ❌ Already flipped this burger");
            }
            else if (!isCooking)
            {
                Debug.Log($"   ❌ Food not cooking (state: {lastDetectedPan.currentFood.currentState})");
            }
            else if (!handsEmpty)
            {
                Debug.Log("   ❌ Hands not empty");
            }
        }

        Debug.Log("═══════════════════════════════════════════════");
    }
}