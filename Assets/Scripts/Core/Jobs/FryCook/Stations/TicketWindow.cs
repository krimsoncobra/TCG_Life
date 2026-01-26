using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Ticket Window - Accept completed burgers and pay the player
/// $1.00 base pay + weighted random tips for good burgers
/// $0.50 flat payment for burnt burgers (no tip)
/// </summary>
public class TicketWindow : MonoBehaviour, IInteractable
{
    [Header("Player Money")]
    [Tooltip("Static money storage - accessible from anywhere")]
    public static float playerMoney = 0f;

    [Header("Payment Settings")]
    [Tooltip("Base payment for good burgers")]
    public float basePayment = 1.00f;

    [Tooltip("Payment for burnt burgers (no tip)")]
    public float burntBurgerPayment = 0.50f;

    [Header("Payment Feedback")]
    [Tooltip("UI text to show payment amount (floating green text)")]
    public TextMeshProUGUI paymentFeedbackText;

    [Tooltip("How long to show payment feedback")]
    public float feedbackDuration = 2f;

    [Tooltip("How high the text floats up")]
    public float floatHeight = 50f;

    [Header("Audio")]
    public AudioClip cashRegisterSound;
    public AudioClip disappointedSound; // Optional: sad sound for burnt burger
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (paymentFeedbackText != null)
        {
            paymentFeedbackText.gameObject.SetActive(false);
        }
    }

    public string GetPromptText()
    {
        if (PlayerHands.Instance != null && PlayerHands.Instance.IsHoldingSomething())
        {
            BurgerPlate plate = PlayerHands.Instance.currentItem?.GetComponent<BurgerPlate>();

            if (plate != null && plate.layers.Count > 0)
            {
                // Check if burger is burnt
                bool isBurnt = IsBurgerBurnt(plate);

                if (isBurnt)
                {
                    return "E to Serve Burnt Burger ($0.50)";
                }

                return "E to Serve Burger (Get Paid!)";
            }
        }

        return "Ticket Window";
    }

    public void Interact()
    {
        if (PlayerHands.Instance == null || !PlayerHands.Instance.IsHoldingSomething())
        {
            Debug.Log("📋 No order to serve!");
            return;
        }

        BurgerPlate plate = PlayerHands.Instance.currentItem?.GetComponent<BurgerPlate>();

        if (plate == null || plate.layers.Count == 0)
        {
            Debug.Log("📋 Can't serve empty plate!");
            return;
        }

        // Check if burger is burnt
        bool isBurnt = IsBurgerBurnt(plate);

        float totalPayment;
        float tip = 0f;

        if (isBurnt)
        {
            // Burnt burger: fixed $0.50, no tip
            totalPayment = burntBurgerPayment;
            Debug.Log($"🔥 Burnt burger served! Fixed payment: ${totalPayment:F2} (no tip)");

            // Play disappointed sound if available
            if (disappointedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(disappointedSound);
            }
        }
        else
        {
            // Good burger: $1 base + random tip (boosted by perfect hits)
            int perfectHits = GetPerfectHits(plate);
            tip = CalculateTip(perfectHits);
            totalPayment = basePayment + tip;

            if (perfectHits > 0)
            {
                Debug.Log($"💰 Good burger with {perfectHits} perfect hits! Base: ${basePayment:F2} + Tip: ${tip:F2} = Total: ${totalPayment:F2}");
            }
            else
            {
                Debug.Log($"💰 Good burger served! Base: ${basePayment:F2} + Tip: ${tip:F2} = Total: ${totalPayment:F2}");
            }

            // Play cash register sound
            if (cashRegisterSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(cashRegisterSound);
            }
        }

        // Give payment to player
        GivePayment(totalPayment);

        // Show visual feedback
        ShowPaymentFeedback(totalPayment, tip, isBurnt);

        // Remove the served burger
        GameObject plateObject = PlayerHands.Instance.currentItem;
        PlayerHands.Instance.currentItem = null;
        Destroy(plateObject);
    }

    /// <summary>
    /// Check if burger has any burnt food items
    /// </summary>
    bool IsBurgerBurnt(BurgerPlate plate)
    {
        if (plate == null || plate.layers.Count == 0)
            return false;

        // Check each layer for burnt food
        foreach (GameObject layer in plate.layers)
        {
            FoodItem food = layer.GetComponent<FoodItem>();

            if (food != null && food.currentState == CookingState.Burnt)
            {
                return true; // Found burnt food
            }
        }

        return false; // No burnt food found
    }

    /// <summary>
    /// Get perfect hits from burger (stored in patty's flipBonus field)
    /// </summary>
    int GetPerfectHits(BurgerPlate plate)
    {
        if (plate == null || plate.layers.Count == 0)
            return 0;

        // Check each layer for patty with perfect hit data
        foreach (GameObject layer in plate.layers)
        {
            FoodItem food = layer.GetComponent<FoodItem>();

            if (food != null && food.currentState == CookingState.Cooked)
            {
                // flipBonus field stores perfect hit count
                int perfectHits = Mathf.RoundToInt(food.flipBonus);
                Debug.Log($"⭐ Found {perfectHits} perfect hits stored in patty");
                return perfectHits;
            }
        }

        return 0; // No perfect hits (manually built burger)
    }

    /// <summary>
    /// Calculate tip with bonus from perfect hits
    /// Perfect hits shift odds toward better tips:
    /// 1 perfect = +1% rare chance
    /// 2 perfects = +5% rare chance  
    /// 3 perfects = +10% rare chance
    /// 4 perfects = +15% rare chance
    /// </summary>
    float CalculateTip(int perfectHits = 0)
    {
        // Calculate bonus based on perfect hits
        float rarityBonus = 0f;
        if (perfectHits == 1)
            rarityBonus = 1f;
        else if (perfectHits == 2)
            rarityBonus = 5f;
        else if (perfectHits == 3)
            rarityBonus = 10f;
        else if (perfectHits >= 4)
            rarityBonus = 15f;

        // Weighted random tip system with rarity bonus
        float roll = Random.Range(0f, 100f);

        Debug.Log($"🎲 Tip roll: {roll:F1} (bonus: +{rarityBonus}% for {perfectHits} perfects)");

        // Apply bonus by REDUCING the threshold for rare tiers
        // This effectively increases chance of better tips

        if (roll < (90f - rarityBonus)) // Common tier (shrinks with perfects)
        {
            return Random.Range(0.00f, 1.00f);
        }
        else if (roll < (95f - (rarityBonus * 0.6f))) // Uncommon (slightly affected)
        {
            return Random.Range(2f, 9f);
        }
        else if (roll < (97.5f - (rarityBonus * 0.3f))) // Rare (moderately affected)
        {
            return Random.Range(24f, 49f);
        }
        else if (roll < (98.5f - (rarityBonus * 0.2f))) // Very Rare (slightly affected)
        {
            return Random.Range(99f, 249f);
        }
        else // Jackpot (always 1.5% + bonus)
        {
            return Random.Range(499f, 999f);
        }
    }

    void GivePayment(float amount)
    {
        // Add to static player money
        playerMoney += amount;

        // SYNC WITH GAMEMANAGER
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMoney(playerMoney);
        }

        Debug.Log($"💰 Player earned: ${amount:F2} | Total: ${playerMoney:F2}");

        // If you have PlayerStats or other currency system, add here:
        // PlayerStats.Instance.AddMoney((int)amount);
    }

    void ShowPaymentFeedback(float totalAmount, float tipAmount, bool isBurnt)
    {
        if (paymentFeedbackText == null) return;

        string message = "";
        Color textColor = Color.green;

        if (isBurnt)
        {
            // Burnt burger feedback
            message = "Burnt Burger...";
            textColor = new Color(0.6f, 0.3f, 0f); // Brown/burnt color
            paymentFeedbackText.text = $"{message}\n+${totalAmount:F2}";
        }
        else
        {
            // Good burger feedback - determine rarity based on TIP
            if (tipAmount >= 499f)
            {
                message = "JACKPOT TIP!";
                textColor = new Color(1f, 0.84f, 0f); // Gold
            }
            else if (tipAmount >= 99f)
            {
                message = "HUGE TIP!";
                textColor = new Color(1f, 0.5f, 0f); // Orange
            }
            else if (tipAmount >= 24f)
            {
                message = "BIG TIP!";
                textColor = new Color(0.5f, 1f, 0.5f); // Light green
            }
            else if (tipAmount >= 2f)
            {
                message = "Nice Tip!";
                textColor = Color.green;
            }
            else
            {
                message = "Thanks!";
                textColor = Color.green;
            }

            // Format: Show base + tip = total
            paymentFeedbackText.text = $"{message}\n+${totalAmount:F2}";
            if (tipAmount > 0)
            {
                paymentFeedbackText.text += $"\n(${basePayment:F2} + ${tipAmount:F2} tip)";
            }
        }

        paymentFeedbackText.color = textColor;

        StartCoroutine(AnimatePaymentFeedback());
    }

    IEnumerator AnimatePaymentFeedback()
    {
        if (paymentFeedbackText == null) yield break;

        GameObject textObj = paymentFeedbackText.gameObject;
        RectTransform rectTransform = paymentFeedbackText.GetComponent<RectTransform>();

        // Store original position
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, floatHeight);

        // Show and animate
        textObj.SetActive(true);
        paymentFeedbackText.alpha = 0f;

        // Fade in and float up
        Sequence sequence = DOTween.Sequence();
        sequence.Append(paymentFeedbackText.DOFade(1f, 0.3f));
        sequence.Join(rectTransform.DOAnchorPos(endPos, feedbackDuration).SetEase(Ease.OutQuad));
        sequence.Append(paymentFeedbackText.DOFade(0f, 0.5f));
        sequence.OnComplete(() =>
        {
            textObj.SetActive(false);
            rectTransform.anchoredPosition = startPos; // Reset position
        });

        yield return new WaitForSeconds(feedbackDuration + 0.8f);
    }
}