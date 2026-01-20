using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// World-space cooking progress bar that appears above cooking food
/// </summary>
public class CookingProgressBar : MonoBehaviour
{
    [Header("References")]
    public CookingPan pan;
    public Canvas worldCanvas;
    public Image fillImage;
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 0.5f, 0);
    public Color cookingColor = Color.yellow;
    public Color cookedColor = Color.green;
    public Color burningColor = Color.orange;
    public Color burntColor = Color.red;

    void Start()
    {
        // Create world canvas if not assigned
        if (worldCanvas == null)
        {
            CreateWorldCanvas();
        }

        // Hide initially
        if (worldCanvas != null)
            worldCanvas.enabled = false;
    }

    void Update()
    {
        if (pan == null || pan.currentFood == null)
        {
            // Hide if no food
            if (worldCanvas != null)
                worldCanvas.enabled = false;
            return;
        }

        // Only show when on grill
        if (!pan.isOnGrill)
        {
            if (worldCanvas != null)
                worldCanvas.enabled = false;
            return;
        }

        // Show and update
        if (worldCanvas != null)
            worldCanvas.enabled = true;

        UpdateProgress();
        UpdatePosition();
    }

    void CreateWorldCanvas()
    {
        // Create canvas GameObject
        GameObject canvasObj = new GameObject("CookingProgressCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offset;

        // Add Canvas component
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;

        // Add Canvas Scaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // Set size
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 0.5f);

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Create fill image
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform);
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = cookingColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-10, -10); // Padding

        // Create status text
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.SetParent(canvasObj.transform);
        statusText = textObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Cooking...";
        statusText.fontSize = 24;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.sizeDelta = new Vector2(0, 0.3f);
        textRect.anchoredPosition = new Vector2(0, 0.2f);
    }

    void UpdateProgress()
    {
        if (pan.currentFood == null || fillImage == null) return;

        FoodItem food = pan.currentFood;

        switch (food.currentState)
        {
            case CookingState.Cooking:
            case CookingState.Raw:
            case CookingState.Prepared:
                fillImage.fillAmount = food.GetCookProgress();
                fillImage.color = cookingColor;
                if (statusText != null)
                    statusText.text = $"Cooking... {(food.GetCookProgress() * 100):F0}%";
                break;

            case CookingState.Cooked:
                float burnProgress = food.GetBurnProgress();
                fillImage.fillAmount = burnProgress;

                if (burnProgress < 0.5f)
                {
                    fillImage.color = cookedColor;
                    if (statusText != null)
                        statusText.text = "✓ Cooked!";
                }
                else
                {
                    fillImage.color = burningColor;
                    if (statusText != null)
                        statusText.text = $"⚠️ Burning! {(burnProgress * 100):F0}%";
                }
                break;

            case CookingState.Burnt:
                fillImage.fillAmount = 1f;
                fillImage.color = burntColor;
                if (statusText != null)
                    statusText.text = "✗ BURNT!";
                break;
        }
    }

    void UpdatePosition()
    {
        if (worldCanvas == null) return;

        // Make canvas face camera
        if (Camera.main != null)
        {
            worldCanvas.transform.LookAt(Camera.main.transform);
            worldCanvas.transform.Rotate(0, 180, 0); // Flip to face player
        }
    }
}