using UnityEngine;

/// <summary>
/// Defines a customer's order with quantities
/// Can request 1-3 burgers and/or 1-3 fries
/// </summary>
[System.Serializable]
public class CustomerOrder
{
    [Header("Order Details")]
    public string orderID; // Unique ID for this order

    [Header("Quantities")]
    [Range(0, 3)]
    public int burgersWanted = 0; // 0-3 burgers

    [Range(0, 3)]
    public int friesWanted = 0; // 0-3 fries orders

    [Header("Payment")]
    public float burgerPayment = 1.00f; // Per burger
    public float friesPayment = 0.50f; // Per fries

    [Header("Time Limit")]
    public float timeLimit = 180f; // 3 minutes default
    public float timeRemaining;

    [Header("Status")]
    public bool isCompleted = false;
    public bool isExpired = false;
    public bool isActive = false; // Is currently being worked on

    public CustomerOrder(int burgers, int fries, float timeLimit = 180f)
    {
        this.orderID = System.Guid.NewGuid().ToString();
        this.burgersWanted = Mathf.Clamp(burgers, 0, 3);
        this.friesWanted = Mathf.Clamp(fries, 0, 3);
        this.timeLimit = timeLimit;
        this.timeRemaining = timeLimit;
        this.isActive = false; // Starts inactive until player takes it
    }

    /// <summary>
    /// Get total payment for this order
    /// </summary>
    public float GetTotalPayment()
    {
        return (burgersWanted * burgerPayment) + (friesWanted * friesPayment);
    }

    /// <summary>
    /// Get order description for UI
    /// </summary>
    public string GetOrderDescription()
    {
        if (burgersWanted > 0 && friesWanted > 0)
            return $"{burgersWanted}x Burger + {friesWanted}x Fries";
        else if (burgersWanted > 0)
            return $"{burgersWanted}x Burger{(burgersWanted > 1 ? "s" : "")}";
        else if (friesWanted > 0)
            return $"{friesWanted}x Fries";
        else
            return "Empty Order";
    }

    /// <summary>
    /// Check if order matches what's on the tray
    /// </summary>
    public bool MatchesTray(ServingTray tray)
    {
        if (tray == null) return false;

        int trayBurgers = tray.GetBurgerCount();
        int trayFries = tray.GetFriesCount();

        // Must match exactly what was ordered
        return (burgersWanted == trayBurgers) && (friesWanted == trayFries);
    }

    /// <summary>
    /// Update timer (call in Update)
    /// </summary>
    public void UpdateTimer(float deltaTime)
    {
        if (isCompleted || isExpired || !isActive) return;

        timeRemaining -= deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            isExpired = true;
            Debug.Log($"⏰ Order {orderID} expired!");
        }
    }

    /// <summary>
    /// Get time remaining as percentage (0-1)
    /// </summary>
    public float GetTimeProgress()
    {
        return Mathf.Clamp01(timeRemaining / timeLimit);
    }

    /// <summary>
    /// Activate order (start timer)
    /// </summary>
    public void Activate()
    {
        isActive = true;
        Debug.Log($"📋 Order activated: {GetOrderDescription()}");
    }

    /// <summary>
    /// Mark order as completed
    /// </summary>
    public void Complete()
    {
        isCompleted = true;
        Debug.Log($"✅ Order {orderID} completed! Payment: ${GetTotalPayment():F2}");
    }
}