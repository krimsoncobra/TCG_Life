using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Gears of War style timing minigame for flipping burgers
/// </summary>
public class BurgerFlipMinigame : MonoBehaviour
{
    public static BurgerFlipMinigame Instance;

    [Header("UI References")]
    public CanvasGroup minigamePanel;
    public Image sliderBar;
    public Image sliderFill;
    public RectTransform goldenZone;
    public RectTransform cursor;
    public TextMeshProUGUI resultText;

    [Header("Settings")]
    public float sliderSpeed = 200f;      // Pixels per second
    public float goldenZoneSize = 50f;    // Width of golden zone
    public Vector2 goldenZonePosition = new Vector2(0.7f, 0f); // 70% across bar

    [Header("Colors")]
    public Color normalColor = Color.gray;
    public Color goldenColor = Color.yellow;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    private bool isActive = false;
    private float cursorPosition = 0f;
    private float barWidth;
    private FoodItem targetFood;
    private InputAction flipAction;

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

        // Setup flip input (Spacebar)
        flipAction = new InputAction("Flip", InputActionType.Button);
        flipAction.AddBinding("<Keyboard>/space");
        flipAction.performed += ctx => TryFlip();
    }

    void OnEnable() => flipAction.Enable();
    void OnDisable() => flipAction.Disable();

    void Start()
    {
        HideMinigame();

        // Get bar width
        if (sliderBar != null)
        {
            barWidth = sliderBar.rectTransform.rect.width;
        }

        // Setup golden zone
        if (goldenZone != null)
        {
            goldenZone.sizeDelta = new Vector2(goldenZoneSize, goldenZone.sizeDelta.y);
            goldenZone.anchoredPosition = new Vector2(goldenZonePosition.x * barWidth - barWidth / 2f, 0f);
            goldenZone.GetComponent<Image>().color = goldenColor;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Move cursor
        cursorPosition += sliderSpeed * Time.deltaTime;

        // Loop cursor
        if (cursorPosition > barWidth)
        {
            cursorPosition = 0f;
        }

        // Update cursor visual
        if (cursor != null)
        {
            cursor.anchoredPosition = new Vector2(cursorPosition - barWidth / 2f, 0f);
        }

        // Update fill color based on position
        UpdateFillColor();
    }

    // ═══════════════════════════════════════════════════════════════
    //  MINIGAME CONTROL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Start the flip minigame for a specific food item
    /// </summary>
    public void StartMinigame(FoodItem food)
    {
        targetFood = food;
        isActive = true;
        cursorPosition = 0f;

        // Show UI
        if (minigamePanel != null)
        {
            minigamePanel.alpha = 1f;
            minigamePanel.interactable = true;
            minigamePanel.blocksRaycasts = true;
        }

        if (resultText != null)
            resultText.text = "Press SPACE to Flip!";

        // Pause game slightly for timing
        Time.timeScale = 0.5f;

        Debug.Log("🎮 Flip minigame started!");
    }

    void HideMinigame()
    {
        isActive = false;

        if (minigamePanel != null)
        {
            minigamePanel.alpha = 0f;
            minigamePanel.interactable = false;
            minigamePanel.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
    }

    void TryFlip()
    {
        if (!isActive) return;

        // Check if cursor is in golden zone
        bool inGoldenZone = IsInGoldenZone();

        if (inGoldenZone)
        {
            // PERFECT FLIP!
            if (resultText != null)
                resultText.text = "PERFECT FLIP! ✨";

            if (targetFood != null)
            {
                targetFood.ApplyFlipBonus();
            }

            if (sliderFill != null)
                sliderFill.color = successColor;

            Debug.Log("✨ PERFECT FLIP!");
        }
        else
        {
            // Normal flip (no bonus)
            if (resultText != null)
                resultText.text = "Flipped";

            if (sliderFill != null)
                sliderFill.color = failColor;

            Debug.Log("📍 Normal flip");
        }

        // End minigame after brief delay
        Invoke("HideMinigame", 0.5f);
    }

    bool IsInGoldenZone()
    {
        if (goldenZone == null) return false;

        float goldenStart = goldenZone.anchoredPosition.x + barWidth / 2f - goldenZoneSize / 2f;
        float goldenEnd = goldenStart + goldenZoneSize;

        return cursorPosition >= goldenStart && cursorPosition <= goldenEnd;
    }

    void UpdateFillColor()
    {
        if (sliderFill == null) return;

        if (IsInGoldenZone())
        {
            sliderFill.color = goldenColor;
        }
        else
        {
            sliderFill.color = normalColor;
        }
    }
}