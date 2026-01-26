using UnityEngine;
using TMPro;

/// <summary>
/// HUD Money Display - shows player's current money in top-left corner
/// Updates automatically when money changes
/// </summary>
public class MoneyDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("The TextMeshProUGUI component to display money")]
    public TextMeshProUGUI moneyText;

    [Header("Format Settings")]
    [Tooltip("Prefix before the amount (e.g., '$', 'Money: ')")]
    public string prefix = "$";

    [Tooltip("Number of decimal places to show")]
    public int decimalPlaces = 2;

    [Header("Optional: Reference to Currency System")]
    [Tooltip("If you have a PlayerStats or CurrencyManager, you can reference it here")]
    public MonoBehaviour currencyManager;

    private float lastKnownMoney = 0f;

    void Start()
    {
        if (moneyText == null)
        {
            moneyText = GetComponent<TextMeshProUGUI>();
        }

        if (moneyText == null)
        {
            Debug.LogError("❌ MoneyDisplay: No TextMeshProUGUI component found!");
        }

        UpdateDisplay();
    }

    void Update()
    {
        // Check if money has changed and update display
        float currentMoney = GetCurrentMoney();

        if (currentMoney != lastKnownMoney)
        {
            lastKnownMoney = currentMoney;
            UpdateDisplay();
        }
    }

    float GetCurrentMoney()
    {
        // Get money from TicketWindow's static variable
        // You can replace this with your own currency system

        // Option 1: Use TicketWindow's static money (simple)
        return TicketWindow.playerMoney;

        // Option 2: If you have PlayerStats
        // return PlayerStats.Instance.money;

        // Option 3: If you have PlayerPrefs
        // return PlayerPrefs.GetFloat("Money", 0);

        // Option 4: Custom currency manager
        // if (currencyManager != null)
        // {
        //     // Cast and get money from your system
        // }
    }

    void UpdateDisplay()
    {
        if (moneyText != null)
        {
            float money = GetCurrentMoney();
            string formattedMoney = money.ToString($"F{decimalPlaces}");
            moneyText.text = $"{prefix}{formattedMoney}";
        }
    }

    /// <summary>
    /// Call this to force an immediate update (useful after earning money)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateDisplay();
    }
}