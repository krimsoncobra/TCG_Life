using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual progress bar UI element
/// One instance per cooking pan OR fryer basket
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
    private FryerBasket assignedBasket;

    public void InitializeForPan(CookingPan pan)
    {
        assignedPan = pan;
        assignedBasket = null;
    }

    public void InitializeForBasket(FryerBasket basket)
    {
        assignedBasket = basket;
        assignedPan = null;
    }

    public void UpdateDisplay()
    {
        FoodItem food = null;

        // Get food from either pan or basket
        if (assignedPan != null && assignedPan.currentFood != null)
        {
            food = assignedPan.currentFood;
        }
        else if (assignedBasket != null && assignedBasket.currentFries != null)
        {
            food = assignedBasket.currentFries;
        }

        if (food == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

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