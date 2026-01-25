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
    private Vector3 spawnPosition; // Store where to spawn result
    private Quaternion spawnRotation;
    private bool isPlaying = false;
    private int currentFlips = 0;
    private int perfectFlips = 0;
    private float currentSpeed;
    private bool movingRight = true;
    private RectTransform trackRect;
    private float trackHeight;
    private float trackWidth;

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

        // Use New Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CheckHit();
            Debug.Log("⌨️ SPACE pressed in minigame!");
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

        // Store pan position for spawning results later
        spawnPosition = currentPan.transform.position;
        spawnRotation = currentPan.transform.rotation;

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
        perfectFlips = 0;
        currentSpeed = indicatorSpeed;

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

            if (indicator.anchoredPosition.y < -trackHeight / 2 - 100)
            {
                indicator.anchoredPosition = new Vector2(0, trackHeight / 2 + 100);
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
                    indicator.anchoredPosition = new Vector2(trackWidth / 2, indicator.anchoredPosition.y);
                    movingRight = false;
                }
            }
            else
            {
                indicator.anchoredPosition -= new Vector2(movement, 0);

                if (indicator.anchoredPosition.x <= -trackWidth / 2)
                {
                    indicator.anchoredPosition = new Vector2(-trackWidth / 2, indicator.anchoredPosition.y);
                    movingRight = true;
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
        perfectFlips++;
        currentSpeed += speedIncrease;

        if (perfectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(perfectSound);
        }

        // Flash GOLD for perfect hit
        StartCoroutine(FlashElement(perfectLine, new Color(1f, 0.84f, 0f))); // Gold color
        StartCoroutine(FlashElement(indicator, new Color(1f, 0.84f, 0f))); // Gold color

        Debug.Log($"⭐ PERFECT! Flip {currentFlips}/{flipsRequired}");

        if (currentFood != null)
        {
            currentFood.flipBonus += perfectBonus;
        }

        CheckCompletion();
    }

    void OnGoodHit()
    {
        currentFlips++;
        currentSpeed += speedIncrease * 0.5f;

        if (goodSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(goodSound);
        }

        // Flash GREEN for good hit
        StartCoroutine(FlashElement(goodZone, Color.green));
        StartCoroutine(FlashElement(indicator, Color.green));

        Debug.Log($"✓ Good! Flip {currentFlips}/{flipsRequired}");

        if (currentFood != null)
        {
            currentFood.flipBonus += goodBonus;
        }

        CheckCompletion();
    }

    void OnMiss()
    {
        if (missSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(missSound);
        }

        StartCoroutine(FlashElement(indicator, Color.red));
        Debug.Log($"❌ Miss! Try again...");
    }

    void CheckCompletion()
    {
        UpdateUI();

        if (currentFlips >= flipsRequired)
        {
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
            $"🎉 PERFECT FLIP! Burger is ready! {perfectFlips} Perfect hits!" :
            $"❌ FAILED! Burger burned! Only {currentFlips}/{flipsRequired} flips!";

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

        // Spawn cooked burger on plate
        Debug.Log($"🍽️ Attempting to spawn at position: {spawnPosition}");

        if (cookedBurgerPlatePrefab != null)
        {
            Debug.Log($"✅ Prefab found! Instantiating...");
            GameObject cookedPlate = Instantiate(cookedBurgerPlatePrefab, spawnPosition, spawnRotation);
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

        // Spawn burnt burger in pan
        if (burntBurgerPanPrefab != null)
        {
            GameObject burntPan = Instantiate(burntBurgerPanPrefab, spawnPosition, spawnRotation);
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
            scoreText.text = $"Flips: {currentFlips}/{flipsRequired}";
        }

        if (comboText != null)
        {
            comboText.text = perfectFlips > 0 ? $"Perfect: {perfectFlips}" : "";
        }

        if (instructionText != null)
        {
            instructionText.text = "Press SPACE to match the spacebar!";
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