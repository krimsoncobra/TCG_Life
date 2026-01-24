using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual progress bar UI element
/// One instance per cooking pan
/// Attach this to the ProgressBarEntry prefab
/// </summary>
public class CookingProgressBarUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image fillImage;
    public TextMeshProUGUI statusText;
    public Image backgroundImage;

    [Header("Colors")]
    public Color cookingColor = new Color(1f, 0.8f, 0f);
    public Color cookedColor = new Color(0f, 1f, 0f);
    public Color burningColor = new Color(1f, 0.5f, 0f);
    public Color burntColor = new Color(1f, 0f, 0f);

    private CookingPan assignedPan;

    public void Initialize(CookingPan pan)
    {
        assignedPan = pan;
    }

    public void UpdateDisplay()
    {
        if (assignedPan == null || assignedPan.currentFood == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        FoodItem food = assignedPan.currentFood;

        switch (food.currentState)
        {
            case CookingState.Cooking:
            case CookingState.Raw:
            case CookingState.Prepared:
                float cookProgress = food.GetCookProgress();
                if (fillImage != null)
                {
                    fillImage.fillAmount = cookProgress;
                    fillImage.color = cookingColor;
                }
                if (statusText != null)
                {
                    statusText.text = $"Cooking {(cookProgress * 100):F0}%";
                }
                break;

            case CookingState.Cooked:
                float burnProgress = food.GetBurnProgress();
                if (fillImage != null)
                {
                    fillImage.fillAmount = burnProgress;
                    fillImage.color = burnProgress < 0.5f ? cookedColor : burningColor;
                }
                if (statusText != null)
                {
                    if (burnProgress < 0.5f)
                    {
                        statusText.text = "Ready!";
                    }
                    else
                    {
                        statusText.text = $"Burning! {(burnProgress * 100):F0}%";
                    }
                }
                break;

            case CookingState.Burnt:
                if (fillImage != null)
                {
                    fillImage.fillAmount = 1f;
                    fillImage.color = burntColor;
                }
                if (statusText != null)
                {
                    statusText.text = "BURNT!";
                }
                break;
        }
    }
}