using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Ticket Window - MULTIPLE ORDERS VERSION
/// Players can take multiple orders and work on them simultaneously
/// </summary>
public class TicketWindow : MonoBehaviour, IInteractable
{
    [Header("Player Money")]
    public static float playerMoney = 0f;

    [Header("Multiple Orders Settings")]
    [Tooltip("Maximum number of active orders player can have at once")]
    public int maxActiveOrders = 3;

    [Tooltip("List of all active orders player is working on")]
    public List<CustomerOrder> activeOrders = new List<CustomerOrder>();

    [Header("Order Generation")]
    [Range(1, 3)]
    public int minBurgers = 1;
    [Range(1, 3)]
    public int maxBurgers = 2;

    [Range(1, 3)]
    public int minFries = 1;
    [Range(1, 3)]
    public int maxFries = 2;

    [Range(0f, 1f)]
    public float burgerOnlyChance = 0.3f;
    [Range(0f, 1f)]
    public float friesOnlyChance = 0.2f;

    public float orderTimeLimit = 180f; // 3 minutes

    [Header("UI References")]
    [Tooltip("The order ticket panel GameObjects (not components)")]
    public List<GameObject> orderTicketPanels = new List<GameObject>();

    [Header("Payment Feedback")]
    public TextMeshProUGUI paymentFeedbackText;
    public float feedbackDuration = 2f;
    public float floatHeight = 50f;

    [Header("Audio")]
    public AudioClip cashRegisterSound;
    public AudioClip wrongOrderSound;
    public AudioClip takeOrderSound;
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

    void Update()
    {
        // Update all active order timers
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            activeOrders[i].UpdateTimer(Time.deltaTime);

            // Check if expired
            if (activeOrders[i].isExpired)
            {
                Debug.Log($"❌ Order {i + 1} expired! Customer left.");

                // Hide corresponding ticket panel
                if (i < orderTicketPanels.Count && orderTicketPanels[i] != null)
                {
                    HideTicketPanel(orderTicketPanels[i]);
                }

                activeOrders.RemoveAt(i);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        if (PlayerHands.Instance == null) return "Ticket Window";

        // Priority 1: Player holding tray with food -> serve order
        if (PlayerHands.Instance.IsHolding<ServingTray>())
        {
            ServingTray tray = PlayerHands.Instance.GetHeldItem<ServingTray>();

            if (tray.GetBurgerCount() == 0 && tray.GetFriesCount() == 0)
            {
                return "Empty Tray - Add Food First!";
            }

            // Find matching order
            CustomerOrder matchingOrder = FindMatchingOrder(tray);
            if (matchingOrder != null)
            {
                return $"E to Serve Order (${matchingOrder.GetTotalPayment():F2})";
            }
            else if (activeOrders.Count > 0)
            {
                return "Wrong Order! Check Tickets";
            }
            else
            {
                return "No Active Orders!";
            }
        }

        // Priority 2: Empty hands -> take new order
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            if (activeOrders.Count < maxActiveOrders)
            {
                return $"E to Take Order ({activeOrders.Count}/{maxActiveOrders})";
            }
            else
            {
                return $"Max Orders! ({activeOrders.Count}/{maxActiveOrders})";
            }
        }

        return "Ticket Window";
    }

    public void Interact()
    {
        if (PlayerHands.Instance == null) return;

        // Case 1: Holding tray -> try to serve order
        if (PlayerHands.Instance.IsHolding<ServingTray>())
        {
            ServeTray();
            return;
        }

        // Case 2: Empty hands -> take new order
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            TakeNewOrder();
            return;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  TAKE ORDER
    // ═══════════════════════════════════════════════════════════════

    void TakeNewOrder()
    {
        if (activeOrders.Count >= maxActiveOrders)
        {
            Debug.LogWarning($"⚠️ Already have max orders ({maxActiveOrders})!");
            return;
        }

        // Generate new order
        CustomerOrder newOrder = GenerateOrder();
        newOrder.Activate();

        activeOrders.Add(newOrder);

        // Show on next available ticket panel
        int orderIndex = activeOrders.Count - 1;
        if (orderIndex < orderTicketPanels.Count && orderTicketPanels[orderIndex] != null)
        {
            ShowTicketPanel(orderTicketPanels[orderIndex], newOrder);
        }
        else
        {
            Debug.LogWarning($"⚠️ No ticket panel slot #{orderIndex + 1}! Assign more panels in Inspector.");
        }

        // Play sound
        if (takeOrderSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(takeOrderSound);
        }

        Debug.Log($"📝 Took order #{activeOrders.Count}: {newOrder.GetOrderDescription()} (${newOrder.GetTotalPayment():F2})");
    }

    CustomerOrder GenerateOrder()
    {
        float roll = Random.value;
        int burgers = 0;
        int fries = 0;

        if (roll < burgerOnlyChance)
        {
            burgers = Random.Range(minBurgers, maxBurgers + 1);
            fries = 0;
        }
        else if (roll < burgerOnlyChance + friesOnlyChance)
        {
            burgers = 0;
            fries = Random.Range(minFries, maxFries + 1);
        }
        else
        {
            burgers = Random.Range(minBurgers, maxBurgers + 1);
            fries = Random.Range(minFries, maxFries + 1);
        }

        return new CustomerOrder(burgers, fries, orderTimeLimit);
    }

    // ═══════════════════════════════════════════════════════════════
    //  SERVE ORDER
    // ═══════════════════════════════════════════════════════════════

    void ServeTray()
    {
        ServingTray tray = PlayerHands.Instance.GetHeldItem<ServingTray>();

        if (tray == null)
        {
            Debug.LogWarning("Not holding a tray!");
            return;
        }

        if (activeOrders.Count == 0)
        {
            Debug.LogWarning("No active orders!");
            ShowFeedback("No Active Orders!", Color.red);
            return;
        }

        int trayBurgers = tray.GetBurgerCount();
        int trayFries = tray.GetFriesCount();

        // Find matching order
        CustomerOrder matchingOrder = FindMatchingOrder(tray);
        int orderIndex = matchingOrder != null ? activeOrders.IndexOf(matchingOrder) : -1;

        if (matchingOrder != null)
        {
            // PERFECT ORDER!
            float timeTaken = matchingOrder.timeLimit - matchingOrder.timeRemaining;
            float basePayment = matchingOrder.GetTotalPayment();
            float tipPercentage = CalculateTip(matchingOrder.GetTimeProgress());
            float tipAmount = basePayment * tipPercentage;
            float totalPayment = basePayment + tipAmount;

            Debug.Log($"✅ PERFECT ORDER #{orderIndex + 1}! Total: ${totalPayment:F2}");

            GivePayment(totalPayment);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecordOrderCompleted(totalPayment, tipAmount, trayBurgers, trayFries, timeTaken);
            }

            ShowFeedback($"Perfect Order #{orderIndex + 1}!\n+${totalPayment:F2}\n(+${tipAmount:F2} tip!)", Color.green);

            if (cashRegisterSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(cashRegisterSound);
            }

            // Hide ticket panel
            if (orderIndex < orderTicketPanels.Count && orderTicketPanels[orderIndex] != null)
            {
                HideTicketPanel(orderTicketPanels[orderIndex]);
            }

            // Remove order
            activeOrders.RemoveAt(orderIndex);
        }
        else
        {
            // WRONG ORDER - Accept with base payment only
            float basePayment = (trayBurgers * 1.00f) + (trayFries * 0.50f);

            Debug.LogWarning($"❌ WRONG ORDER! Base payment only: ${basePayment:F2}");

            GivePayment(basePayment);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RecordOrderFailed(trayBurgers, trayFries);
            }

            ShowFeedback($"Wrong Order!\nBase Pay: ${basePayment:F2}\n(No Tip)", new Color(1f, 0.5f, 0f));

            if (wrongOrderSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(wrongOrderSound);
            }

            // Don't remove any orders - player needs to fix it!
        }

        // Clear tray
        tray.ClearTray();

        // Drop tray
        GameObject trayObj = PlayerHands.Instance.currentItem;
        PlayerHands.Instance.currentItem = null;
        tray.OnDrop();
        trayObj.transform.position = transform.position + Vector3.right * 0.5f;
    }

    CustomerOrder FindMatchingOrder(ServingTray tray)
    {
        foreach (CustomerOrder order in activeOrders)
        {
            if (order.MatchesTray(tray))
            {
                return order;
            }
        }
        return null;
    }

    float CalculateTip(float timeProgress)
    {
        if (timeProgress >= 0.9f) return 0.5f;
        else if (timeProgress >= 0.75f) return 0.35f;
        else if (timeProgress >= 0.5f) return 0.20f;
        else if (timeProgress >= 0.25f) return 0.10f;
        else return 0f;
    }

    void GivePayment(float amount)
    {
        playerMoney += amount;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMoney(playerMoney);
        }

        Debug.Log($"💰 Player earned: ${amount:F2} | Total: ${playerMoney:F2}");
    }

    // ═══════════════════════════════════════════════════════════════
    //  FEEDBACK UI
    // ═══════════════════════════════════════════════════════════════

    void ShowFeedback(string message, Color color)
    {
        if (paymentFeedbackText == null) return;

        paymentFeedbackText.text = message;
        paymentFeedbackText.color = color;

        StartCoroutine(AnimateFeedback());
    }

    IEnumerator AnimateFeedback()
    {
        if (paymentFeedbackText == null) yield break;

        GameObject textObj = paymentFeedbackText.gameObject;
        RectTransform rectTransform = paymentFeedbackText.GetComponent<RectTransform>();

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, floatHeight);

        textObj.SetActive(true);
        paymentFeedbackText.alpha = 0f;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(paymentFeedbackText.DOFade(1f, 0.3f));
        sequence.Join(rectTransform.DOAnchorPos(endPos, feedbackDuration).SetEase(Ease.OutQuad));
        sequence.Append(paymentFeedbackText.DOFade(0f, 0.5f));
        sequence.OnComplete(() =>
        {
            textObj.SetActive(false);
            rectTransform.anchoredPosition = startPos;
        });

        yield return new WaitForSeconds(feedbackDuration + 0.8f);
    }

    // ═══════════════════════════════════════════════════════════════
    //  PANEL DISPLAY HELPERS
    // ═══════════════════════════════════════════════════════════════

    void ShowTicketPanel(GameObject panel, CustomerOrder order)
    {
        if (panel == null) return;

        // Activate panel
        panel.SetActive(true);

        // Update text elements
        UpdatePanelDisplay(panel, order);

        Debug.Log($"📋 Showing panel: {panel.name} for order: {order.GetOrderDescription()}");
    }

    void HideTicketPanel(GameObject panel)
    {
        if (panel == null) return;

        panel.SetActive(false);

        Debug.Log($"📋 Hiding panel: {panel.name}");
    }

    void UpdatePanelDisplay(GameObject panel, CustomerOrder order)
    {
        if (panel == null || order == null) return;

        // Find and update burger quantity
        TextMeshProUGUI burgerText = panel.transform.Find("BurgerQuantityText")?.GetComponent<TextMeshProUGUI>();
        if (burgerText != null)
        {
            if (order.burgersWanted > 0)
            {
                burgerText.text = $"x{order.burgersWanted}";
                burgerText.gameObject.SetActive(true);
            }
            else
            {
                burgerText.gameObject.SetActive(false);
            }
        }

        // Find and update fries quantity
        TextMeshProUGUI friesText = panel.transform.Find("FriesQuantityText")?.GetComponent<TextMeshProUGUI>();
        if (friesText != null)
        {
            if (order.friesWanted > 0)
            {
                friesText.text = $"x{order.friesWanted}";
                friesText.gameObject.SetActive(true);
            }
            else
            {
                friesText.gameObject.SetActive(false);
            }
        }

        // Find and update payment
        TextMeshProUGUI paymentText = panel.transform.Find("PaymentText")?.GetComponent<TextMeshProUGUI>();
        if (paymentText != null)
        {
            paymentText.text = $"${order.GetTotalPayment():F2}";
        }

        // Find and update burger icon
        Image burgerIcon = panel.transform.Find("BurgerImage")?.GetComponent<Image>();
        if (burgerIcon != null)
        {
            burgerIcon.enabled = order.burgersWanted > 0;
        }

        // Find and update fries icon
        Image friesIcon = panel.transform.Find("FriesImage")?.GetComponent<Image>();
        if (friesIcon != null)
        {
            friesIcon.enabled = order.friesWanted > 0;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  DEBUG
    // ═══════════════════════════════════════════════════════════════

    [ContextMenu("Clear All Orders")]
    public void Debug_ClearOrders()
    {
        foreach (var panel in orderTicketPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        activeOrders.Clear();
        Debug.Log("Cleared all orders");
    }
}