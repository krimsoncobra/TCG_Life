using UnityEngine;
using System.Collections;

/// <summary>
/// Fry Salting Minigame - Press A & D alternating to shake salt
/// Similar to burger flipping but for fries
/// </summary>
public class FrySaltingMinigame : MonoBehaviour
{
    public static FrySaltingMinigame Instance { get; private set; }

    [Header("Minigame Settings")]
    [Tooltip("How many shakes needed to complete")]
    public int shakesRequired = 10;

    [Tooltip("Max time between shakes before failure")]
    public float shakeTimeout = 0.8f;

    [Header("Current State")]
    public bool isActive = false;
    public UncookedFries currentFries;
    public int currentShakes = 0;
    public bool lastKeyWasA = false; // Track alternation
    public float timeSinceLastShake = 0f;

    [Header("Visual Feedback")]
    public GameObject saltShakerPrefab; // The salt shaker that appears
    private GameObject activeSaltShaker;

    [Header("Audio")]
    public AudioClip shakeSound;
    public AudioClip completeSound;
    public AudioClip failSound;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (shakeSound != null || completeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Track timeout
        timeSinceLastShake += Time.deltaTime;

        if (timeSinceLastShake >= shakeTimeout)
        {
            // Too slow! Failed
            FailMinigame();
            return;
        }

        // Check for A or D input
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnShakeInput(true); // true = A key
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            OnShakeInput(false); // false = D key
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  START/STOP MINIGAME
    // ═══════════════════════════════════════════════════════════════

    public void StartMinigame(UncookedFries fries)
    {
        if (isActive)
        {
            Debug.LogWarning("⚠️ Minigame already active!");
            return;
        }

        Debug.Log("🧂 Starting salting minigame!");

        isActive = true;
        currentFries = fries;
        currentShakes = 0;
        lastKeyWasA = false;
        timeSinceLastShake = 0f;

        // Spawn salt shaker visual
        if (saltShakerPrefab != null)
        {
            Vector3 spawnPos = fries.transform.position + Vector3.up * 1f;
            activeSaltShaker = Instantiate(saltShakerPrefab, spawnPos, Quaternion.identity);
        }

        // Show UI
        if (FrySaltingUI.Instance != null)
        {
            FrySaltingUI.Instance.ShowUI(shakesRequired);
        }

        Debug.Log($"🧂 Press A & D alternating! ({shakesRequired} shakes needed)");
    }

    void OnShakeInput(bool isAKey)
    {
        // Check alternation
        if (currentShakes > 0)
        {
            // Must alternate!
            if ((isAKey && lastKeyWasA) || (!isAKey && !lastKeyWasA))
            {
                // Same key pressed twice in a row = fail!
                Debug.LogWarning("❌ Must alternate A & D!");
                FailMinigame();
                return;
            }
        }

        // Valid shake!
        lastKeyWasA = isAKey;
        currentShakes++;
        timeSinceLastShake = 0f; // Reset timeout

        Debug.Log($"🧂 Shake {currentShakes}/{shakesRequired} ({(isAKey ? "A" : "D")})");

        // Play shake sound
        if (shakeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shakeSound);
        }

        // Shake animation
        if (activeSaltShaker != null)
        {
            // Tilt left for A, right for D
            float tiltAngle = isAKey ? -15f : 15f;
            activeSaltShaker.transform.rotation = Quaternion.Euler(0, 0, tiltAngle);

            // Spawn salt particles (optional)
            // ParticleSystem particles = activeSaltShaker.GetComponent<ParticleSystem>();
            // if (particles != null) particles.Play();
        }

        // Update UI
        if (FrySaltingUI.Instance != null)
        {
            FrySaltingUI.Instance.UpdateProgress(currentShakes, shakesRequired);
        }

        // Check if complete
        if (currentShakes >= shakesRequired)
        {
            CompleteMinigame();
        }
    }

    void CompleteMinigame()
    {
        Debug.Log("✅ Salting complete!");

        isActive = false;

        // Mark fries as salted (this improves quality/payment)
        if (currentFries != null)
        {
            currentFries.isSalted = true;
            Debug.Log("🧂 Fries are now salted!");
        }

        // Play success sound
        if (completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completeSound);
        }

        // Clean up
        Cleanup();

        // Hide UI
        if (FrySaltingUI.Instance != null)
        {
            FrySaltingUI.Instance.HideUI(true); // true = success
        }
    }

    void FailMinigame()
    {
        Debug.LogWarning("❌ Salting failed!");

        isActive = false;

        // Fries remain unsalted (lower quality/payment)
        if (currentFries != null)
        {
            currentFries.isSalted = false;
            Debug.Log("❌ Fries not salted (will pay less)");
        }

        // Play fail sound
        if (failSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(failSound);
        }

        // Clean up
        Cleanup();

        // Hide UI
        if (FrySaltingUI.Instance != null)
        {
            FrySaltingUI.Instance.HideUI(false); // false = failure
        }
    }

    void Cleanup()
    {
        // Remove salt shaker
        if (activeSaltShaker != null)
        {
            Destroy(activeSaltShaker);
            activeSaltShaker = null;
        }

        currentFries = null;
        currentShakes = 0;
        lastKeyWasA = false;
        timeSinceLastShake = 0f;
    }

    // ═══════════════════════════════════════════════════════════════
    //  PUBLIC HELPERS
    // ═══════════════════════════════════════════════════════════════

    public bool IsActive()
    {
        return isActive;
    }

    public void CancelMinigame()
    {
        if (isActive)
        {
            Debug.Log("⚠️ Salting cancelled");
            FailMinigame();
        }
    }
}