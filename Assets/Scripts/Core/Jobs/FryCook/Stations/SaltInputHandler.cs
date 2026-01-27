using UnityEngine;

/// <summary>
/// Input handler for salt shaking minigame
/// Press F while holding cooked fries to start salting
/// </summary>
public class SaltInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Key to press to start salting")]
    public KeyCode saltKey = KeyCode.F;

    [Header("Feedback")]
    public string promptText = "Press F to Salt Fries";

    [Header("Audio")]
    public AudioClip cannotSaltSound; // Optional: sound when trying to salt already-salted fries
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && cannotSaltSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Check for salt key press
        if (Input.GetKeyDown(saltKey))
        {
            TryStartSalting();
        }
    }

    void TryStartSalting()
    {
        // Must be holding something
        if (PlayerHands.Instance == null || !PlayerHands.Instance.IsHoldingSomething())
        {
            return;
        }

        // Must be holding fries
        if (!PlayerHands.Instance.IsHolding<UncookedFries>())
        {
            return;
        }

        UncookedFries fries = PlayerHands.Instance.GetHeldItem<UncookedFries>();

        if (fries == null)
        {
            return;
        }

        // Must be cooked (not raw, not burnt)
        if (fries.currentState != CookingState.Cooked)
        {
            Debug.Log("⚠️ Fries must be cooked first!");
            return;
        }

        // Already salted?
        if (fries.isSalted)
        {
            Debug.Log("⚠️ Fries already salted!");

            if (cannotSaltSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(cannotSaltSound);
            }

            return;
        }

        // Check if minigame is already active
        if (FrySaltingMinigame.Instance != null && FrySaltingMinigame.Instance.IsActive())
        {
            Debug.Log("⚠️ Already salting!");
            return;
        }

        // All checks passed - start minigame!
        if (FrySaltingMinigame.Instance != null)
        {
            Debug.Log("🧂 Starting salt minigame!");
            FrySaltingMinigame.Instance.StartMinigame(fries);
        }
        else
        {
            Debug.LogError("❌ FrySaltingMinigame.Instance is null! Make sure FrySaltingMinigame exists in scene.");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  OPTIONAL: SHOW PROMPT ON SCREEN
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Call this from your UI system to show "Press F to Salt" prompt
    /// </summary>
    public string GetPromptText()
    {
        if (PlayerHands.Instance == null || !PlayerHands.Instance.IsHoldingSomething())
        {
            return "";
        }

        if (!PlayerHands.Instance.IsHolding<UncookedFries>())
        {
            return "";
        }

        UncookedFries fries = PlayerHands.Instance.GetHeldItem<UncookedFries>();

        if (fries == null || fries.currentState != CookingState.Cooked)
        {
            return "";
        }

        if (fries.isSalted)
        {
            return "✅ Fries Already Salted!";
        }

        if (FrySaltingMinigame.Instance != null && FrySaltingMinigame.Instance.IsActive())
        {
            return ""; // Hide prompt during minigame
        }

        return promptText;
    }

    // ═══════════════════════════════════════════════════════════════
    //  OPTIONAL: ON-SCREEN GUI FOR TESTING
    // ═══════════════════════════════════════════════════════════════

    void OnGUI()
    {
        string prompt = GetPromptText();

        if (!string.IsNullOrEmpty(prompt))
        {
            // Show prompt in center-top of screen
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.yellow;

            // Add shadow for readability
            GUI.Label(new Rect(Screen.width / 2 - 199, 101, 400, 50), prompt, style);
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(Screen.width / 2 - 200, 100, 400, 50), prompt, style);
        }
    }
}