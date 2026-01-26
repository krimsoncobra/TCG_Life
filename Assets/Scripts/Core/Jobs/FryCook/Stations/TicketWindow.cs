using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Ticket Window - Accept completed burgers and pay the player
/// Weighted random tip system: 90% common, 5% uncommon, 2.5% rare, 1% very rare, 0.5% jackpot
/// </summary>
public class TicketWindow : MonoBehaviour, IInteractable
{
    [Header("Player Money")]
    [Tooltip("Static money storage - accessible from anywhere")]
    public static float playerMoney = 0f;

    [Header("Payment Feedback")]
    [Tooltip("Optional: UI text to show payment amount")]
    public TextMeshProUGUI paymentFeedbackText;

    [Tooltip("How long to show payment feedback")]
    public float feedbackDuration = 2f;

    [Header("Audio")]
    public AudioClip cashRegisterSound;
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

        // Calculate payment with weighted random tips
        float payment = CalculatePayment();

        // Give payment to player (you'll integrate with your currency system)
        GivePayment(payment);

        // Show feedback
        ShowPaymentFeedback(payment);

        // Play sound
        if (cashRegisterSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cashRegisterSound);
        }

        // Remove the served burger
        GameObject plateObject = PlayerHands.Instance.currentItem;
        PlayerHands.Instance.currentItem = null;
        Destroy(plateObject);

        Debug.Log($"💰 Burger served! Payment: ${payment:F2}");
    }

    float CalculatePayment()
    {
        // Weighted random system
        float roll = Random.Range(0f, 100f);

        if (roll < 90f) // 90% chance - Common
        {
            float amount = Random.Range(0.50f, 2.00f);
            Debug.Log($"💵 Common tip: ${amount:F2}");
            return amount;
        }
        else if (roll < 95f) // 5% chance - Uncommon
        {
            float amount = Random.Range(3f, 10f);
            Debug.Log($"👍 Uncommon tip: ${amount:F2}");
            return amount;
        }
        else if (roll < 97.5f) // 2.5% chance - Rare
        {
            float amount = Random.Range(25f, 50f);
            Debug.Log($"✨ RARE tip! ${amount:F2}");
            return amount;
        }
        else if (roll < 98.5f) // 1% chance - Very Rare
        {
            float amount = Random.Range(100f, 250f);
            Debug.Log($"🌟 VERY RARE tip!! ${amount:F2}");
            return amount;
        }
        else // 0.5% chance - Jackpot!
        {
            float amount = Random.Range(500f, 1000f);
            Debug.Log($"💎💎💎 JACKPOT!!! ${amount:F2} 💎💎💎");
            return amount;
        }
    }

    void GivePayment(float amount)
    {
        // Add to static player money
        playerMoney += amount;

        Debug.Log($"💰 Player earned: ${amount:F2} | Total: ${playerMoney:F2}");

        // If you have your own currency system, add it here too:
        // PlayerStats.Instance.AddMoney(amount);
        // CurrencyManager.Instance.AddCurrency(amount);
        // PlayerPrefs.SetFloat("Money", PlayerPrefs.GetFloat("Money", 0) + amount);
    }

    void ShowPaymentFeedback(float amount)
    {
        if (paymentFeedbackText != null)
        {
            // Determine rarity message
            string message = "";
            if (amount >= 500f)
            {
                message = "💎 JACKPOT! 💎";
            }
            else if (amount >= 100f)
            {
                message = "🌟 HUGE TIP! 🌟";
            }
            else if (amount >= 25f)
            {
                message = "✨ BIG TIP! ✨";
            }
            else if (amount >= 3f)
            {
                message = "👍 Nice Tip!";
            }
            else
            {
                message = "Thanks!";
            }

            paymentFeedbackText.text = $"{message}\n+${amount:F2}";
            StartCoroutine(ShowFeedbackTemporarily());
        }
    }

    IEnumerator ShowFeedbackTemporarily()
    {
        if (paymentFeedbackText != null)
        {
            paymentFeedbackText.gameObject.SetActive(true);
            yield return new WaitForSeconds(feedbackDuration);
            paymentFeedbackText.gameObject.SetActive(false);
        }
    }
}