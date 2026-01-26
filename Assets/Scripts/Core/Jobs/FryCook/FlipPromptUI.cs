using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Shows "Press F to Flip!" prompt when burger is ready to flip
/// Now with cook progress indicator
/// </summary>
public class FlipPromptUI : MonoBehaviour
{
    public static FlipPromptUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;
    public Image progressBar; // Optional: Add a progress bar

    [Header("Animation")]
    public float pulseSpeed = 2f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    private bool isShowing = false;
    private float pulseTimer = 0f;

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

        HidePromptImmediate();
    }

    void Start()
    {
        HidePromptImmediate();
    }

    void HidePromptImmediate()
    {
        isShowing = false;

        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
            Debug.Log("✅ FlipPromptUI: Prompt panel hidden");
        }
        else
        {
            Debug.LogWarning("⚠️ FlipPromptUI: Prompt panel not assigned!");
        }
    }

    void Update()
    {
        if (isShowing && promptPanel != null)
        {
            // Pulse animation
            pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(pulseTimer) + 1f) / 2f);
            promptPanel.transform.localScale = Vector3.one * scale;
        }
    }

    public void ShowPrompt()
    {
        if (!isShowing)
        {
            isShowing = true;
            if (promptPanel != null)
            {
                promptPanel.SetActive(true);
                Debug.Log("✅ FlipPromptUI: Showing prompt");
            }
            else
            {
                Debug.LogError("❌ FlipPromptUI: Cannot show - promptPanel is NULL!");
            }

            if (promptText != null)
            {
                promptText.text = "Press F to Flip!";
            }
            pulseTimer = 0f;
        }
    }

    public void HidePrompt()
    {
        if (isShowing)
        {
            isShowing = false;
            if (promptPanel != null)
            {
                promptPanel.SetActive(false);
                Debug.Log("🔽 FlipPromptUI: Hiding prompt");
            }
        }
    }

    /// <summary>
    /// Update prompt text dynamically (e.g., with cook progress)
    /// </summary>
    public void UpdatePromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
    }

    /// <summary>
    /// Update progress bar if you have one
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = progress;
        }
    }
}