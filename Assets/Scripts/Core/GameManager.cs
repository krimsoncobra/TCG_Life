using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent GameManager that survives scene changes
/// Stores player money, time, and job performance stats
/// Only one instance exists across all scenes
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Persistent Data")]
    public float playerMoney = 0f;
    public float gameTime = 0f; // Total time played (in seconds)
    public float dayNightCycle = 0f; // Day/night cycle progress (0-1)

    [Header("Player Stats (Optional)")]
    public int playerLevel = 1;
    public float playerXP = 0f;

    [Header("Job Performance Stats")]
    public FryCookStats fryCookStats = new FryCookStats();

    [Header("Scene Management")]
    public string currentSceneName;
    public string previousSceneName;

    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This is the magic line!
            Debug.Log("✅ GameManager created and persisted across scenes");
        }
        else
        {
            // Destroy duplicate GameManagers
            Debug.Log("⚠️ Duplicate GameManager detected, destroying...");
            Destroy(gameObject);
            return;
        }

        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Update()
    {
        // Track total game time
        gameTime += Time.deltaTime;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        previousSceneName = currentSceneName;
        currentSceneName = scene.name;

        Debug.Log($"📍 Scene loaded: {currentSceneName} (previous: {previousSceneName})");
        Debug.Log($"💰 Player Money: ${playerMoney:F2}");
        Debug.Log($"⏰ Game Time: {gameTime:F1}s");

        // Sync data with scene-specific systems
        SyncWithScene();
    }

    /// <summary>
    /// Sync GameManager data with scene-specific systems
    /// </summary>
    void SyncWithScene()
    {
        // Sync money with TicketWindow
        TicketWindow.playerMoney = playerMoney;
        Debug.Log($"💰 Synced money to TicketWindow: ${playerMoney:F2}");

        // Sync time with DayNightManager (if it exists in scene)
        DayNightManager dayNightManager = FindFirstObjectByType<DayNightManager>();
        if (dayNightManager != null)
        {
            dayNightManager.SetCycleTime(dayNightCycle);
            Debug.Log($"⏰ Synced day/night cycle: {dayNightCycle:F2}");
        }

        // Sync with PlayerStats (if it exists)
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.money = (int)playerMoney;
            Debug.Log($"💰 Synced money to PlayerStats: ${playerMoney:F2}");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  MONEY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    public void AddMoney(float amount)
    {
        playerMoney += amount;
        TicketWindow.playerMoney = playerMoney; // Keep TicketWindow in sync
        Debug.Log($"💰 Added ${amount:F2} | Total: ${playerMoney:F2}");
    }

    public bool SpendMoney(float amount)
    {
        if (playerMoney >= amount)
        {
            playerMoney -= amount;
            TicketWindow.playerMoney = playerMoney; // Keep TicketWindow in sync
            Debug.Log($"💳 Spent ${amount:F2} | Remaining: ${playerMoney:F2}");
            return true;
        }

        Debug.LogWarning($"⚠️ Not enough money! Need ${amount:F2}, have ${playerMoney:F2}");
        return false;
    }

    public void SetMoney(float amount)
    {
        playerMoney = amount;
        TicketWindow.playerMoney = playerMoney;
    }

    // ═══════════════════════════════════════════════════════════════
    //  TIME MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    public void UpdateDayNightCycle(float cycleTime)
    {
        dayNightCycle = cycleTime;
    }

    public float GetGameTimeInMinutes()
    {
        return gameTime / 60f;
    }

    public float GetGameTimeInHours()
    {
        return gameTime / 3600f;
    }

    // ═══════════════════════════════════════════════════════════════
    //  FRY COOK STATS (NEW!)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Record a completed order
    /// </summary>
    public void RecordOrderCompleted(float payment, float tipAmount, int burgers, int fries, float timeToComplete)
    {
        fryCookStats.RecordOrderCompleted(payment, tipAmount, burgers, fries, timeToComplete);
    }

    /// <summary>
    /// Record a failed order
    /// </summary>
    public void RecordOrderFailed(int burgers, int fries)
    {
        fryCookStats.RecordOrderFailed(burgers, fries);
    }

    /// <summary>
    /// Record burnt food
    /// </summary>
    public void RecordBurntFood(bool isBurger)
    {
        fryCookStats.RecordBurntFood(isBurger);
    }

    /// <summary>
    /// Get fry cook stats
    /// </summary>
    public FryCookStats GetFryCookStats()
    {
        return fryCookStats;
    }

    // ═══════════════════════════════════════════════════════════════
    //  SCENE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    public void LoadScene(string sceneName)
    {
        Debug.Log($"🚪 Loading scene: {sceneName}");

        // Save current state before changing scenes
        SaveBeforeSceneChange();

        SceneManager.LoadScene(sceneName);
    }

    void SaveBeforeSceneChange()
    {
        // Capture latest money from TicketWindow
        playerMoney = TicketWindow.playerMoney;

        // Capture latest day/night cycle
        DayNightManager dayNightManager = FindFirstObjectByType<DayNightManager>();
        if (dayNightManager != null)
        {
            dayNightCycle = dayNightManager.GetCycleTime();
        }

        Debug.Log($"💾 Saved state before scene change - Money: ${playerMoney:F2}, Time: {dayNightCycle:F2}");
    }

    // ═══════════════════════════════════════════════════════════════
    //  SAVE/LOAD (Optional - for future)
    // ═══════════════════════════════════════════════════════════════

    public void SaveGame()
    {
        PlayerPrefs.SetFloat("PlayerMoney", playerMoney);
        PlayerPrefs.SetFloat("GameTime", gameTime);
        PlayerPrefs.SetFloat("DayNightCycle", dayNightCycle);
        PlayerPrefs.SetInt("PlayerLevel", playerLevel);
        PlayerPrefs.SetFloat("PlayerXP", playerXP);

        // Save fry cook stats
        PlayerPrefs.SetInt("FryCook_OrdersCompleted", fryCookStats.ordersCompleted);
        PlayerPrefs.SetInt("FryCook_OrdersFailed", fryCookStats.ordersFailed);
        PlayerPrefs.SetInt("FryCook_PerfectOrders", fryCookStats.perfectOrders);
        PlayerPrefs.SetFloat("FryCook_TotalEarned", fryCookStats.totalEarned);
        PlayerPrefs.SetFloat("FryCook_TotalTips", fryCookStats.totalTipsEarned);
        PlayerPrefs.SetInt("FryCook_BestStreak", fryCookStats.bestStreak);

        PlayerPrefs.Save();

        Debug.Log("💾 Game saved!");
    }

    public void LoadGame()
    {
        playerMoney = PlayerPrefs.GetFloat("PlayerMoney", 0f);
        gameTime = PlayerPrefs.GetFloat("GameTime", 0f);
        dayNightCycle = PlayerPrefs.GetFloat("DayNightCycle", 0f);
        playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        playerXP = PlayerPrefs.GetFloat("PlayerXP", 0f);

        // Load fry cook stats
        fryCookStats.ordersCompleted = PlayerPrefs.GetInt("FryCook_OrdersCompleted", 0);
        fryCookStats.ordersFailed = PlayerPrefs.GetInt("FryCook_OrdersFailed", 0);
        fryCookStats.perfectOrders = PlayerPrefs.GetInt("FryCook_PerfectOrders", 0);
        fryCookStats.totalEarned = PlayerPrefs.GetFloat("FryCook_TotalEarned", 0f);
        fryCookStats.totalTipsEarned = PlayerPrefs.GetFloat("FryCook_TotalTips", 0f);
        fryCookStats.bestStreak = PlayerPrefs.GetInt("FryCook_BestStreak", 0);

        // Sync with current scene
        SyncWithScene();

        Debug.Log($"💾 Game loaded! Money: ${playerMoney:F2}");
    }

    public void ResetGame()
    {
        playerMoney = 0f;
        gameTime = 0f;
        dayNightCycle = 0f;
        playerLevel = 1;
        playerXP = 0f;
        fryCookStats.Reset();

        PlayerPrefs.DeleteAll();
        Debug.Log("🔄 Game data reset!");
    }

    // ═══════════════════════════════════════════════════════════════
    //  DEBUG UTILITIES
    // ═══════════════════════════════════════════════════════════════

    [ContextMenu("Add $100")]
    public void Debug_Add100()
    {
        AddMoney(100f);
    }

    [ContextMenu("Print Current State")]
    public void Debug_PrintState()
    {
        Debug.Log("═══════════════════════════════════════════════");
        Debug.Log("📊 GAMEMANAGER STATE");
        Debug.Log("═══════════════════════════════════════════════");
        Debug.Log($"💰 Money: ${playerMoney:F2}");
        Debug.Log($"⏰ Game Time: {GetGameTimeInMinutes():F1} minutes");
        Debug.Log($"🌙 Day/Night Cycle: {dayNightCycle:F2}");
        Debug.Log($"📍 Current Scene: {currentSceneName}");
        Debug.Log($"📍 Previous Scene: {previousSceneName}");
        Debug.Log("═══════════════════════════════════════════════");
    }

    [ContextMenu("Print Fry Cook Stats")]
    public void Debug_PrintFryCookStats()
    {
        Debug.Log(fryCookStats.GetStatsSummary());
    }

    [ContextMenu("Reset Fry Cook Stats")]
    public void Debug_ResetFryCookStats()
    {
        fryCookStats.Reset();
    }
}