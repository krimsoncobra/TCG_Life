using UnityEngine;

/// <summary>
/// Tracks fry cook job performance stats
/// Stores in GameManager for persistence
/// </summary>
[System.Serializable]
public class FryCookStats
{
    [Header("Order Statistics")]
    public int ordersCompleted = 0;
    public int ordersFailed = 0;
    public int perfectOrders = 0; // Orders completed correctly

    [Header("Earnings")]
    public float totalEarned = 0f;
    public float totalTipsEarned = 0f;

    [Header("Performance Metrics")]
    public float averageOrderTime = 0f; // Average time to complete order
    public int currentStreak = 0; // Perfect orders in a row
    public int bestStreak = 0; // Best streak ever

    [Header("Item Statistics")]
    public int burgersServed = 0;
    public int friesServed = 0;
    public int burgersBurnt = 0;
    public int friesBurnt = 0;

    // ═══════════════════════════════════════════════════════════════
    //  RECORDING METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Record a completed order
    /// </summary>
    public void RecordOrderCompleted(float payment, float tipAmount, int burgers, int fries, float timeToComplete)
    {
        ordersCompleted++;
        totalEarned += payment;
        totalTipsEarned += tipAmount;
        burgersServed += burgers;
        friesServed += fries;

        // Update average order time
        if (ordersCompleted == 1)
            averageOrderTime = timeToComplete;
        else
            averageOrderTime = ((averageOrderTime * (ordersCompleted - 1)) + timeToComplete) / ordersCompleted;

        // Check if it was perfect (got a tip)
        if (tipAmount > 0)
        {
            perfectOrders++;
            currentStreak++;

            if (currentStreak > bestStreak)
                bestStreak = currentStreak;
        }
        else
        {
            currentStreak = 0; // Break streak
        }

        Debug.Log($"📊 Order completed! Total: {ordersCompleted}, Perfect: {perfectOrders}, Streak: {currentStreak}");
    }

    /// <summary>
    /// Record a failed order (wrong items)
    /// </summary>
    public void RecordOrderFailed(int burgers, int fries)
    {
        ordersFailed++;
        currentStreak = 0; // Break streak

        // Still count items served (even if wrong)
        burgersServed += burgers;
        friesServed += fries;

        Debug.Log($"❌ Order failed! Total failures: {ordersFailed}");
    }

    /// <summary>
    /// Record burnt food
    /// </summary>
    public void RecordBurntFood(bool isBurger)
    {
        if (isBurger)
            burgersBurnt++;
        else
            friesBurnt++;
    }

    // ═══════════════════════════════════════════════════════════════
    //  CALCULATED STATS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get success rate (0-100%)
    /// </summary>
    public float GetSuccessRate()
    {
        int totalOrders = ordersCompleted + ordersFailed;
        if (totalOrders == 0) return 0f;

        return (float)ordersCompleted / totalOrders * 100f;
    }

    /// <summary>
    /// Get perfection rate (0-100%)
    /// </summary>
    public float GetPerfectionRate()
    {
        if (ordersCompleted == 0) return 0f;

        return (float)perfectOrders / ordersCompleted * 100f;
    }

    /// <summary>
    /// Get performance grade (S, A, B, C, D, F)
    /// </summary>
    public string GetPerformanceGrade()
    {
        float perfectionRate = GetPerfectionRate();

        if (perfectionRate >= 95f) return "S";
        if (perfectionRate >= 85f) return "A";
        if (perfectionRate >= 75f) return "B";
        if (perfectionRate >= 65f) return "C";
        if (perfectionRate >= 50f) return "D";
        return "F";
    }

    /// <summary>
    /// Get average tips per order
    /// </summary>
    public float GetAverageTips()
    {
        if (ordersCompleted == 0) return 0f;
        return totalTipsEarned / ordersCompleted;
    }

    // ═══════════════════════════════════════════════════════════════
    //  DISPLAY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get formatted stats summary
    /// </summary>
    public string GetStatsSummary()
    {
        return $"═══════════════════════════════════\n" +
               $"FRY COOK PERFORMANCE\n" +
               $"═══════════════════════════════════\n" +
               $"Orders Completed: {ordersCompleted}\n" +
               $"Orders Failed: {ordersFailed}\n" +
               $"Success Rate: {GetSuccessRate():F1}%\n" +
               $"Perfect Orders: {perfectOrders} ({GetPerfectionRate():F1}%)\n" +
               $"Current Streak: {currentStreak}\n" +
               $"Best Streak: {bestStreak}\n" +
               $"Grade: {GetPerformanceGrade()}\n" +
               $"─────────────────────────────────\n" +
               $"Total Earned: ${totalEarned:F2}\n" +
               $"Total Tips: ${totalTipsEarned:F2}\n" +
               $"Avg Tips/Order: ${GetAverageTips():F2}\n" +
               $"Avg Order Time: {averageOrderTime:F1}s\n" +
               $"─────────────────────────────────\n" +
               $"Burgers Served: {burgersServed}\n" +
               $"Fries Served: {friesServed}\n" +
               $"Burgers Burnt: {burgersBurnt}\n" +
               $"Fries Burnt: {friesBurnt}\n" +
               $"═══════════════════════════════════";
    }

    /// <summary>
    /// Reset all stats
    /// </summary>
    public void Reset()
    {
        ordersCompleted = 0;
        ordersFailed = 0;
        perfectOrders = 0;
        totalEarned = 0f;
        totalTipsEarned = 0f;
        averageOrderTime = 0f;
        currentStreak = 0;
        bestStreak = 0;
        burgersServed = 0;
        friesServed = 0;
        burgersBurnt = 0;
        friesBurnt = 0;

        Debug.Log("🔄 Fry cook stats reset!");
    }
}