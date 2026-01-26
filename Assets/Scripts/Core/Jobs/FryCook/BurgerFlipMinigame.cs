using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Guitar Hero style burger flip minigame - GameObject version
/// Drag GameObjects with CinemachineCamera components
/// Tracks perfect hits for tip bonuses
/// </summary>
public class BurgerFlipMinigame : MonoBehaviour
{
    public static BurgerFlipMinigame Instance { get; private set; }

    [Header("UI References")]
    public GameObject minigameCanvas;
    public RectTransform indicator;
    public RectTransform perfectLine;
    public RectTransform goodZone;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI instructionText;

    [Header("Cinemachine Cameras (drag GameObjects here)")]
    [Tooltip("Drag the GameObject with your player's Cinemachine Camera")]
    public GameObject playerCameraObject;
    [Tooltip("Drag the GameObject with minigame Cinemachine Camera")]
    public GameObject minigameCameraObject;

    [Header("Minigame Settings")]
    public float indicatorSpeed = 400f;
    public float speedIncrease = 75f;
    public int flipsRequired = 4;
    public bool verticalMovement = true;

    [Header("Hit Detection")]
    public float perfectTolerance = 15f;
    public float goodTolerance = 60f;

    [Header("Scoring")]
    public float perfectBonus = 2f;
    public float goodBonus = 1f;

    [Header("Outcome Prefabs")]
    [Tooltip("Prefab of cooked burger on plate (spawned on success)")]
    public GameObject cookedBurgerPlatePrefab;
    [Tooltip("Prefab of burnt burger in pan (spawned on failure)")]
    public GameObject burntBurgerPanPrefab;

    [Header("Audio")]
    public AudioClip perfectSound;
    public AudioClip goodSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    // Internal references
    private CinemachineCamera playerCamera;
    private CinemachineCamera minigameCamera;

    private FoodItem currentFood;
    private CookingPan currentPan;
    private Grill currentGrill; // Store reference to the grill
    private Vector3 spawnPosition; // Store where to spawn result
    private Quaternion spawnRotation;
    private bool isPlaying = false;
    private int currentFlips = 0; // Successful flips
    private int totalAttempts = 0; // Total attempts (hits + misses)
    private int perfectFlips = 0;
    public int lastPerfectHits = 0; // Track perfect hits from last game for tip bonus
    private float currentSpeed;
    private bool movingRight = true;
    private RectTransform trackRect;
    private float trackHeight;
    private float trackWidth;

    // Input locking to prevent spam
    private bool canPressSpace = true;
    private bool hasAttemptedThisCycle = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Get CinemachineCamera components from GameObjects
        if (playerCameraObject != null)
        {
            playerCamera = playerCameraObject.GetComponent<CinemachineCamera>();
            if (playerCamera == null)
            {
                Debug.LogError("❌ Player camera GameObject doesn't have CinemachineCamera component!");
            }
        }

        if (minigameCameraObject != null)
        {
            minigameCamera = minigameCameraObject.GetComponent<CinemachineCamera>();
            if (minigameCamera == null)
            {
                Debug.LogError("❌ Minigame camera GameObject doesn't have CinemachineCamera component!");
            }
        }

        if (minigameCanvas != null && indicator != null)
        {
            trackRect = indicator.parent.GetComponent<RectTransform>();
            if (trackRect != null)
            {
                trackHeight = trackRect.rect.height;
                trackWidth = trackRect.rect.width;
            }
            minigameCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        MoveIndicator();

        // Use New Input System with input lock
        if (canPressSpace && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("⌨️ SPACE pressed in minigame!");
            hasAttemptedThisCycle = true;
            canPressSpace = false; // Lock input until indicator resets
            CheckHit();
        }
    }

    public void StartMinigame(FoodItem food)
    {
        if (food == null)
        {
            Debug.LogError("❌ StartMinigame: food is NULL!");
            return;
        }

        if (isPlaying)
        {
            Debug.LogWarning("⚠️ Minigame already playing!");
            return;
        }

        Debug.Log("🎸 StartMinigame called! Starting in 0.1s...");

        // Small delay to ensure clean transition
        StartCoroutine(StartMinigameDelayed(food));
    }

    IEnumerator StartMinigameDelayed(FoodItem food)
    {
        yield return new WaitForSeconds(0.1f);

        currentFood = food;

        // Hide the flip prompt immediately
        if (FlipPromptUI.Instance != null)
        {
            FlipPromptUI.Instance.HidePrompt();
            Debug.Log("🔽 Hid flip prompt");
        }

        // Get reference to the pan
        currentPan = food.GetComponentInParent<CookingPan>();
        if (currentPan == null)
        {
            Debug.LogError("❌ Could not find CookingPan parent!");
            yield break;
        }

        // Get reference to the grill
        currentGrill = currentPan.GetComponentInParent<Grill>();
        if (currentGrill == null)
        {
            Debug.LogWarning("⚠️ Could not find Grill parent! Will spawn at pan position.");
        }

        // Store spawn position (prefer grill position over pan position)
        if (currentGrill != null)
        {
            // Spawn in front of grill
            spawnPosition = currentGrill.transform.position + currentGrill.transform.forward * 1.5f;
            spawnPosition.y = currentGrill.transform.position.y + 1f;
            spawnRotation = currentGrill.transform.rotation;
            Debug.Log($"📍 Will spawn in front of grill at: {spawnPosition}");
        }
        else
        {
            // Fallback: spawn at pan position
            spawnPosition = currentPan.transform.position;
            spawnRotation = currentPan.transform.rotation;
            Debug.Log($"📍 Will spawn at pan position: {spawnPosition}");
        }

        Debug.Log($"🗑️ Despawning pan and burger, will spawn result at {spawnPosition}");

        // Remove food from pan first
        currentPan.RemoveFood();

        // Destroy pan and food immediately
        Destroy(currentPan.gameObject);
        Destroy(currentFood.gameObject);

        // Clear references so they don't cause issues
        currentFood = null;
        currentPan = null;

        isPlaying = true;
        currentFlips = 0;
        totalAttempts = 0; // RESET attempts counter!
        perfectFlips = 0; // RESET perfect hits counter
        currentSpeed = indicatorSpeed;

        // Reset input locks
        canPressSpace = true;
        hasAttemptedThisCycle = false;

        Debug.Log("🔄 Reset all minigame counters and locks");

        if (verticalMovement)
        {
            indicator.anchoredPosition = new Vector2(0, trackHeight / 2 + 100);
        }
        else
        {
            indicator.anchoredPosition = new Vector2(-trackWidth / 2, indicator.anchoredPosition.y);
        }

        // Disable player controls
        DisablePlayerControls();

        SwitchToMinigameCamera();

        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(true);
            Debug.Log("🎨 Showed minigame canvas");
        }
        else
        {
            Debug.LogError("❌ Minigame canvas is NULL!");
        }

        UpdateUI();
        Debug.Log($"🎸 Flip minigame started! Hit SPACE {flipsRequired} times!");
    }

    void DisablePlayerControls()
    {
        // Find player GameObject (usually tagged "Player")
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Disable all MonoBehaviour components on player that might handle input
            MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();

            foreach (var component in components)
            {
                if (component == null) continue;

                string typeName = component.GetType().Name;

                // Disable common player controller types
                if (typeName.Contains("PlayerController") ||
                    typeName.Contains("ThirdPersonController") ||
                    typeName.Contains("FirstPersonController") ||
                    typeName.Contains("PlayerMovement") ||
                    typeName.Contains("PlayerInput"))
                {
                    component.enabled = false;
                    Debug.Log($"🎮 Disabled {typeName}");
                }
            }

            // Also disable CharacterController
            CharacterController charController = player.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
                Debug.Log("🎮 Disabled CharacterController");
            }
        }

        // Disable new Input System PlayerInput if present
        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
            Debug.Log("🎮 Deactivated PlayerInput");
        }

        // Keep cursor locked and hidden - we don't need mouse for this minigame
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("🎮 Player controls disabled");
    }

    void EnablePlayerControls()
    {
        try
        {
            Debug.Log("🎮 Re-enabling player controls...");

            // Find player GameObject
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                // Re-enable all components we disabled
                MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();

                foreach (var component in components)
                {
                    if (component == null) continue;

                    string typeName = component.GetType().Name;

                    if (typeName.Contains("PlayerController") ||
                        typeName.Contains("ThirdPersonController") ||
                        typeName.Contains("FirstPersonController") ||
                        typeName.Contains("PlayerMovement") ||
                        typeName.Contains("PlayerInput"))
                    {
                        component.enabled = true;
                        Debug.Log($"🎮 Enabled {typeName}");
                    }
                }

                // Re-enable CharacterController
                CharacterController charController = player.GetComponent<CharacterController>();
                if (charController != null)
                {
                    charController.enabled = true;
                    Debug.Log("🎮 Enabled CharacterController");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find Player GameObject!");
            }

            // Re-enable PlayerInput
            var playerInput = FindFirstObjectByType<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.ActivateInput();
                Debug.Log("🎮 Activated PlayerInput");
            }

            // Restore cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("✅ Player controls enabled successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error enabling player controls: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");

            // Emergency fallback - just make sure cursor is correct
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void SwitchToMinigameCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
            Debug.Log("📷 Disabled player camera");
        }
        else
        {
            Debug.LogWarning("⚠️ Player camera not found!");
        }

        if (minigameCamera != null)
        {
            minigameCamera.enabled = true;
            Debug.Log("📷 Enabled minigame camera");
        }
        else
        {
            Debug.LogError("❌ Minigame camera not found!");
        }
    }

    void SwitchToPlayerCamera()
    {
        if (minigameCamera != null)
        {
            minigameCamera.enabled = false;
            Debug.Log("📷 Disabled minigame camera");
        }

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            Debug.Log("📷 Enabled player camera");
        }
    }

    void MoveIndicator()
    {
        if (verticalMovement)
        {
            float movement = currentSpeed * Time.deltaTime;
            indicator.anchoredPosition -= new Vector2(0, movement);

            // Check if indicator has passed the bottom (missed the window)
            if (indicator.anchoredPosition.y < -trackHeight / 2 - 100)
            {
                // If player didn't attempt during this cycle, count as miss
                if (!hasAttemptedThisCycle)
                {
                    Debug.Log($"❌ Auto-miss! Player didn't press during cycle");
                    OnMiss(); // This will increment totalAttempts and check completion
                }

                // Reset for next cycle
                indicator.anchoredPosition = new Vector2(0, trackHeight / 2 + 100);
                canPressSpace = true;
                hasAttemptedThisCycle = false;
            }
        }
        else
        {
            float movement = currentSpeed * Time.deltaTime;

            if (movingRight)
            {
                indicator.anchoredPosition += new Vector2(movement, 0);

                if (indicator.anchoredPosition.x >= trackWidth / 2)
                {
                    // Reached end - check if player attempted
                    if (!hasAttemptedThisCycle)
                    {
                        Debug.Log($"❌ Auto-miss! Player didn't press during cycle");
                        OnMiss(); // This will increment totalAttempts and check completion
                    }

                    indicator.anchoredPosition = new Vector2(trackWidth / 2, indicator.anchoredPosition.y);
                    movingRight = false;
                    canPressSpace = true;
                    hasAttemptedThisCycle = false;
                }
            }
            else
            {
                indicator.anchoredPosition -= new Vector2(movement, 0);

                if (indicator.anchoredPosition.x <= -trackWidth / 2)
                {
                    // Reached end - check if player attempted
                    if (!hasAttemptedThisCycle)
                    {
                        Debug.Log($"❌ Auto-miss! Player didn't press during cycle");
                        OnMiss(); // This will increment totalAttempts and check completion
                    }

                    indicator.anchoredPosition = new Vector2(-trackWidth / 2, indicator.anchoredPosition.y);
                    movingRight = true;
                    canPressSpace = true;
                    hasAttemptedThisCycle = false;
                }
            }
        }
    }

    void CheckHit()
    {
        if (verticalMovement)
        {
            float indicatorCenterY = indicator.anchoredPosition.y;
            float perfectLineY = perfectLine.anchoredPosition.y;

            float indicatorHalfHeight = indicator.rect.height / 2;
            float indicatorTopEdge = indicatorCenterY + indicatorHalfHeight;
            float indicatorBottomEdge = indicatorCenterY - indicatorHalfHeight;

            float goodZoneHalfHeight = goodZone.rect.height / 2;
            float goodZoneTopEdge = goodZone.anchoredPosition.y + goodZoneHalfHeight;
            float goodZoneBottomEdge = goodZone.anchoredPosition.y - goodZoneHalfHeight;

            float distanceFromPerfect = Mathf.Abs(indicatorCenterY - perfectLineY);

            if (distanceFromPerfect <= perfectTolerance)
            {
                OnPerfectHit();
            }
            else if (IsOverlapping(indicatorBottomEdge, indicatorTopEdge, goodZoneBottomEdge, goodZoneTopEdge))
            {
                OnGoodHit();
            }
            else
            {
                OnMiss();
            }
        }
        else
        {
            float indicatorCenterX = indicator.anchoredPosition.x;
            float perfectLineX = perfectLine.anchoredPosition.x;

            float indicatorHalfWidth = indicator.rect.width / 2;
            float indicatorLeftEdge = indicatorCenterX - indicatorHalfWidth;
            float indicatorRightEdge = indicatorCenterX + indicatorHalfWidth;

            float goodZoneHalfWidth = goodZone.rect.width / 2;
            float goodZoneLeftEdge = goodZone.anchoredPosition.x - goodZoneHalfWidth;
            float goodZoneRightEdge = goodZone.anchoredPosition.x + goodZoneHalfWidth;

            float distanceFromPerfect = Mathf.Abs(indicatorCenterX - perfectLineX);

            if (distanceFromPerfect <= perfectTolerance)
            {
                OnPerfectHit();
            }
            else if (IsOverlapping(indicatorLeftEdge, indicatorRightEdge, goodZoneLeftEdge, goodZoneRightEdge))
            {
                OnGoodHit();
            }
            else
            {
                OnMiss();
            }
        }
    }

    bool IsOverlapping(float a1, float a2, float b1, float b2)
    {
        return !(a2 < b1 || b2 < a1);
    }

    void OnPerfectHit()
    {
        currentFlips++;
        totalAttempts++; // Count this attempt
        perfectFlips++; // Count perfect hit
        currentSpeed += speedIncrease;

        if (perfectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(perfectSound);
        }

        // Flash GOLD for perfect hit
        StartCoroutine(FlashElement(perfectLine, new Color(1f, 0.84f, 0f)));
        StartCoroutine(FlashElement(indicator, new Color(1f, 0.84f, 0f)));

        Debug.Log($"⭐ PERFECT! Hits: {currentFlips} | Attempts: {totalAttempts}/{flipsRequired} | Perfect Hits: {perfectFlips}");

        if (currentFood != null)
        {
            currentFood.flipBonus += perfectBonus;
        }

        CheckCompletion();
    }

    void OnGoodHit()
    {
        currentFlips++;
        totalAttempts++; // Count this attempt
        currentSpeed += speedIncrease * 0.5f;

        if (goodSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(goodSound);
        }

        // Flash GREEN for good hit
        StartCoroutine(FlashElement(goodZone, Color.green));
        StartCoroutine(FlashElement(indicator, Color.green));

        Debug.Log($"✓ Good! Hits: {currentFlips} | Attempts: {totalAttempts}/{flipsRequired}");

        if (currentFood != null)
        {
            currentFood.flipBonus += goodBonus;
        }

        CheckCompletion();
    }

    void OnMiss()
    {
        totalAttempts++; // Count this attempt

        if (missSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(missSound);
        }

        StartCoroutine(FlashElement(indicator, Color.red));
        Debug.Log($"❌ MISS! Hits: {currentFlips} | Attempts: {totalAttempts}/{flipsRequired}");

        CheckCompletion();
    }

    void CheckCompletion()
    {
        UpdateUI();

        // End game after 4 attempts (regardless of hits/misses)
        if (totalAttempts >= flipsRequired)
        {
            Debug.Log($"🎮 Game Over! {currentFlips} successful hits out of {totalAttempts} attempts");
            EndMinigame(true);
        }
    }

    void EndMinigame(bool success)
    {
        isPlaying = false;

        float successRate = (float)currentFlips / (float)flipsRequired;

        // Calculate success based on flips completed
        bool isSuccessful = successRate >= 0.75f; // Need 75% or more (3/4 flips)

        Debug.Log($"🎮 Minigame ended! Success rate: {successRate:P0} ({currentFlips}/{flipsRequired} flips)");

        // SAVE PERFECT HITS for TicketWindow to use
        lastPerfectHits = perfectFlips;
        Debug.Log($"⭐ Perfect hits this game: {lastPerfectHits}");

        if (isSuccessful)
        {
            // SUCCESS: Burger is perfectly cooked, put it on a plate
            Debug.Log($"✅ SUCCESS! {currentFlips}/{flipsRequired} flips - Starting SuccessOutcome coroutine...");
            StartCoroutine(SuccessOutcome());
        }
        else
        {
            // FAILURE: Burger burns in pan
            Debug.Log($"❌ FAILED! Only {currentFlips}/{flipsRequired} flips - Starting FailureOutcome coroutine...");
            StartCoroutine(FailureOutcome());
        }

        string resultText = isSuccessful ?
            $"PERFECT FLIP! Burger is ready! {perfectFlips} Perfect hits!" :
            $"FAILED! Burger burned! Only {currentFlips}/{flipsRequired} flips!";

        Debug.Log(resultText);

        if (instructionText != null)
        {
            instructionText.text = resultText;
        }
    }

    IEnumerator SuccessOutcome()
    {
        Debug.Log("🎉 SuccessOutcome started - waiting 2 seconds...");

        // Wait 2 seconds to show result
        yield return new WaitForSeconds(2f);

        Debug.Log("⏰ 2 seconds elapsed, closing minigame...");

        // Close minigame UI
        SwitchToPlayerCamera();
        Debug.Log("📷 Switched to player camera");

        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
            Debug.Log("🎨 Hidden minigame canvas");
        }

        Debug.Log("🎮 About to enable player controls...");
        EnablePlayerControls();
        Debug.Log("✅ Player controls enabled");

        // Spawn cooked burger on plate IN FRONT OF GRILL
        Debug.Log($"🍽️ Spawning cooked burger at: {spawnPosition}");

        if (cookedBurgerPlatePrefab != null)
        {
            Debug.Log($"✅ Prefab found! Instantiating...");
            GameObject cookedPlate = Instantiate(cookedBurgerPlatePrefab, spawnPosition, spawnRotation);

            // Ensure it has BurgerPlate component (IInteractable)
            BurgerPlate plateComponent = cookedPlate.GetComponent<BurgerPlate>();
            if (plateComponent == null)
            {
                plateComponent = cookedPlate.AddComponent<BurgerPlate>();
                Debug.Log("➕ Added BurgerPlate component");
            }

            // CRITICAL: Add burger layers so TicketWindow recognizes it as sellable
            if (plateComponent.layers.Count == 0)
            {
                Debug.Log("📝 Setting up burger layers for complete burger...");

                // Create dummy GameObjects for the burger parts
                // Bottom Bun
                GameObject bottomBun = new GameObject("BottomBun");
                bottomBun.transform.SetParent(plateComponent.stackPosition);
                bottomBun.AddComponent<BottomBun>();
                bottomBun.tag = "Untagged"; // NOT interactable
                plateComponent.layers.Add(bottomBun);

                // Cooked Patty WITH PERFECT HITS STORED
                GameObject patty = new GameObject("CookedPatty");
                patty.transform.SetParent(plateComponent.stackPosition);
                FoodItem foodItem = patty.AddComponent<FoodItem>();
                foodItem.currentState = CookingState.Cooked;
                foodItem.foodName = "Cooked Patty";
                patty.tag = "Untagged"; // NOT interactable

                // STORE PERFECT HITS IN THE PATTY
                foodItem.flipBonus = lastPerfectHits; // Store perfect hit count
                Debug.Log($"⭐ Stored {lastPerfectHits} perfect hits in patty");

                plateComponent.layers.Add(patty);

                // Top Bun
                GameObject topBun = new GameObject("TopBun");
                topBun.transform.SetParent(plateComponent.stackPosition);
                topBun.AddComponent<TopBun>();
                topBun.tag = "Untagged"; // NOT interactable
                plateComponent.layers.Add(topBun);

                Debug.Log($"✅ Added {plateComponent.layers.Count} layers to burger plate (children not interactable)");
            }

            // Ensure it's tagged as Interactable
            if (!cookedPlate.CompareTag("Interactable"))
            {
                cookedPlate.tag = "Interactable";
                Debug.Log("➕ Tagged as Interactable");
            }

            // Add physics if not already present
            Rigidbody rb = cookedPlate.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = cookedPlate.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                Debug.Log("➕ Added Rigidbody to spawned plate");
            }

            Collider col = cookedPlate.GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider boxCol = cookedPlate.AddComponent<BoxCollider>();
                Debug.Log("➕ Added BoxCollider to spawned plate");
            }

            Debug.Log($"🍽️ Spawned cooked burger on plate: {cookedPlate.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Cooked Burger Plate Prefab not assigned! Assign in Inspector.");
        }

        Debug.Log("✅ Success! Perfectly cooked burger on plate!");
    }

    IEnumerator FailureOutcome()
    {
        // Wait 2 seconds to show result
        yield return new WaitForSeconds(2f);

        // Close minigame UI
        SwitchToPlayerCamera();
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
        }
        EnablePlayerControls();

        // Spawn burnt burger in pan IN FRONT OF GRILL
        Debug.Log($"🔥 Spawning burnt burger at: {spawnPosition}");

        if (burntBurgerPanPrefab != null)
        {
            GameObject burntPan = Instantiate(burntBurgerPanPrefab, spawnPosition, spawnRotation);

            // Ensure it has CookingPan component (IInteractable)
            CookingPan panComponent = burntPan.GetComponent<CookingPan>();
            if (panComponent == null)
            {
                panComponent = burntPan.AddComponent<CookingPan>();
                Debug.Log("➕ Added CookingPan component");
            }

            // Ensure it's tagged as Interactable
            if (!burntPan.CompareTag("Interactable"))
            {
                burntPan.tag = "Interactable";
                Debug.Log("➕ Tagged as Interactable");
            }

            // Add physics if not already present
            if (burntPan.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = burntPan.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                Debug.Log("➕ Added Rigidbody to burnt pan");
            }

            if (burntPan.GetComponent<Collider>() == null)
            {
                BoxCollider col = burntPan.AddComponent<BoxCollider>();
                Debug.Log("➕ Added BoxCollider to burnt pan");
            }

            Debug.Log($"🔥 Spawned burnt burger in pan at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("⚠️ Burnt Burger Pan Prefab not assigned! Assign in Inspector.");
        }

        Debug.Log("💀 Failure! Burnt burger in pan!");
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Attempt {totalAttempts}/{flipsRequired} | Hits: {currentFlips}";
        }

        if (comboText != null)
        {
            // Move perfect text to separate line to avoid overlap
            if (perfectFlips > 0)
            {
                comboText.text = $"Perfect: {perfectFlips}";
            }
            else
            {
                comboText.text = "";
            }
        }

        if (instructionText != null)
        {
            // Keep instruction separate from other text
            instructionText.text = "Press SPACE!";
        }
    }

    IEnumerator FlashElement(RectTransform element, Color color)
    {
        Image img = element.GetComponent<Image>();
        if (img == null) yield break;

        Color original = img.color;
        img.color = color;

        yield return new WaitForSeconds(0.15f);

        img.color = original;
    }
}