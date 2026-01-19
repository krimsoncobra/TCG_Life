using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Money")]
    public int money = 100;
    public TextMeshProUGUI moneyText;

    [Header("Skills")]
    public float[] skills = { 0f, 0f, 0f }; // 0=Speed, 1=Power, 2=Intelligence
    public int[] levels = { 1, 1, 1 };
    public float baseXpToNext = 100f;
    public Slider[] skillSliders;
    public TextMeshProUGUI[] skillLabels;

    [Header("UI")]
    public CanvasGroup pauseCanvasGroup;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateMoneyUI();
        UpdateSkillUI();
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    // ═══════════════════════════════════════════════════════════════
    //  XP & LEVELING SYSTEM
    // ═══════════════════════════════════════════════════════════════

    public void AddXP(int skillIndex, float amount)
    {
        if (skillIndex < 0 || skillIndex > 2) return;

        skills[skillIndex] += amount;
        CheckLevelUp(skillIndex);
        UpdateSkillUI();
    }

    void CheckLevelUp(int skillIndex)
    {
        float xpNeeded = GetXPNeeded(skillIndex);

        while (skills[skillIndex] >= xpNeeded)
        {
            skills[skillIndex] -= xpNeeded;
            levels[skillIndex]++;
            xpNeeded = GetXPNeeded(skillIndex);

            Debug.Log($"🎉 Level Up! {GetSkillName(skillIndex)} → Lv {levels[skillIndex]}");
            // TODO: Trigger level up VFX/SFX here
        }
    }

    float GetXPNeeded(int skillIndex)
    {
        return baseXpToNext + (levels[skillIndex] - 1) * 50f;
    }

    string GetSkillName(int index) => index switch
    {
        0 => "Speed",
        1 => "Power",
        2 => "Intelligence",
        _ => "Unknown"
    };

    // ═══════════════════════════════════════════════════════════════
    //  MONEY SYSTEM
    // ═══════════════════════════════════════════════════════════════

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoneyUI();
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateMoneyUI();
            return true;
        }

        Debug.LogWarning("Not enough money!");
        return false;
    }

    // ═══════════════════════════════════════════════════════════════
    //  UI UPDATES (Only called when values change)
    // ═══════════════════════════════════════════════════════════════

    void UpdateMoneyUI()
    {
        if (moneyText) moneyText.text = "$" + money.ToString("N0");
    }

    void UpdateSkillUI()
    {
        for (int i = 0; i < 3; i++)
        {
            float xpNeeded = GetXPNeeded(i);
            float progress = skills[i] / xpNeeded;

            if (skillSliders[i])
                skillSliders[i].value = progress;

            if (skillLabels[i])
                skillLabels[i].text = $"{GetSkillName(i)}: Lv {levels[i]} ({skills[i]:F0}/{xpNeeded:F0})";
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  PAUSE MENU
    // ═══════════════════════════════════════════════════════════════

    void TogglePause()
    {
        isPaused = !isPaused;

        pauseCanvasGroup.alpha = isPaused ? 1f : 0f;
        pauseCanvasGroup.interactable = isPaused;
        pauseCanvasGroup.blocksRaycasts = isPaused;

        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void Resume()
    {
        if (isPaused) TogglePause();
    }
}