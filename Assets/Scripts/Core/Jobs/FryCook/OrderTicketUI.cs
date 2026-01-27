using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Order Ticket UI - Displays in bottom-left corner
/// Shows burger/fries icons with quantities and timer
/// </summary>
public class OrderTicketUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Main panel that slides in/out")]
    public GameObject ticketPanel;

    [Tooltip("Burger icon image")]
    public Image burgerIcon;

    [Tooltip("Text showing burger quantity (e.g., 'x2')")]
    public TextMeshProUGUI burgerQuantityText;

    [Tooltip("Fries icon image")]
    public Image friesIcon;

    [Tooltip("Text showing fries quantity (e.g., 'x3')")]
    public TextMeshProUGUI friesQuantityText;

    [Tooltip("Timer text (e.g., '2:30')")]
    public TextMeshProUGUI timerText;

    [Tooltip("Timer progress bar")]
    public Image timerBar;

    [Tooltip("Total payment text (e.g., '$2.50')")]
    public TextMeshProUGUI paymentText;

    [Header("Animation")]
    public float slideInDuration = 0.5f;
    public float slideOutDuration = 0.3f;
    public Vector2 hiddenPosition = new Vector2(-400, 0); // Off screen left
    public Vector2 shownPosition = new Vector2(20, 20); // Bottom-left corner

    [Header("Colors")]
    public Color normalTimerColor = Color.green;
    public Color warningTimerColor = Color.yellow;
    public Color urgentTimerColor = Color.red;

    [Header("Icon Settings")]
    public Color activeIconColor = Color.white;
    public Color inactiveIconColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // Grayed out

    private CustomerOrder currentOrder;
    private bool isShowing = false;
    private RectTransform panelRect;

    void Awake()
    {
        if (ticketPanel != null)
        {
            panelRect = ticketPanel.GetComponent<RectTransform>();

            // Start hidden
            ticketPanel.SetActive(false);
            if (panelRect != null)
            {
                panelRect.anchoredPosition = hiddenPosition;
            }
        }
    }

    void Update()
    {
        // Update timer every frame if ticket is showing
        if (isShowing && currentOrder != null)
        {
            UpdateTimerDisplay();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  SHOW/HIDE TICKET
    // ═══════════════════════════════════════════════════════════════

    public void ShowTicket(CustomerOrder order)
    {
        if (order == null)
        {
            Debug.LogError("Cannot show ticket - order is null!");
            return;
        }

        currentOrder = order;
        isShowing = true;

        // Set up UI elements
        UpdateOrderDisplay();

        // Activate panel
        if (ticketPanel != null)
        {
            ticketPanel.SetActive(true);
        }

        // Animate slide in
        if (panelRect != null)
        {
            panelRect.anchoredPosition = hiddenPosition;
            panelRect.DOAnchorPos(shownPosition, slideInDuration).SetEase(Ease.OutBack);
        }

        Debug.Log($"📋 Showing ticket: {order.GetOrderDescription()}");
    }

    public void HideTicket()
    {
        isShowing = false;
        currentOrder = null;

        // Animate slide out
        if (panelRect != null)
        {
            panelRect.DOAnchorPos(hiddenPosition, slideOutDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (ticketPanel != null)
                    {
                        ticketPanel.SetActive(false);
                    }
                });
        }

        Debug.Log("📋 Hiding ticket");
    }

    // ═══════════════════════════════════════════════════════════════
    //  UPDATE DISPLAY
    // ═══════════════════════════════════════════════════════════════

    void UpdateOrderDisplay()
    {
        if (currentOrder == null) return;

        // Update burger display
        if (currentOrder.burgersWanted > 0)
        {
            // Show burger icon
            if (burgerIcon != null)
            {
                burgerIcon.enabled = true;
                burgerIcon.color = activeIconColor;
            }

            // Show quantity
            if (burgerQuantityText != null)
            {
                burgerQuantityText.text = $"x{currentOrder.burgersWanted}";
                burgerQuantityText.gameObject.SetActive(true);
            }
        }
        else
        {
            // Hide burger icon
            if (burgerIcon != null)
            {
                burgerIcon.enabled = false;
            }

            if (burgerQuantityText != null)
            {
                burgerQuantityText.gameObject.SetActive(false);
            }
        }

        // Update fries display
        if (currentOrder.friesWanted > 0)
        {
            // Show fries icon
            if (friesIcon != null)
            {
                friesIcon.enabled = true;
                friesIcon.color = activeIconColor;
            }

            // Show quantity
            if (friesQuantityText != null)
            {
                friesQuantityText.text = $"x{currentOrder.friesWanted}";
                friesQuantityText.gameObject.SetActive(true);
            }
        }
        else
        {
            // Hide fries icon
            if (friesIcon != null)
            {
                friesIcon.enabled = false;
            }

            if (friesQuantityText != null)
            {
                friesQuantityText.gameObject.SetActive(false);
            }
        }

        // Update payment text
        if (paymentText != null)
        {
            paymentText.text = $"${currentOrder.GetTotalPayment():F2}";
        }

        // Initial timer update
        UpdateTimerDisplay();
    }

    void UpdateTimerDisplay()
    {
        if (currentOrder == null) return;

        // Update timer text
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentOrder.timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(currentOrder.timeRemaining % 60f);
            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }

        // Update timer bar
        if (timerBar != null)
        {
            float progress = currentOrder.GetTimeProgress();
            timerBar.fillAmount = progress;

            // Change color based on time remaining
            if (progress > 0.5f)
                timerBar.color = normalTimerColor;
            else if (progress > 0.25f)
                timerBar.color = warningTimerColor;
            else
                timerBar.color = urgentTimerColor;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  PUBLIC HELPERS
    // ═══════════════════════════════════════════════════════════════

    public bool IsShowing()
    {
        return isShowing;
    }

    public CustomerOrder GetCurrentOrder()
    {
        return currentOrder;
    }
}