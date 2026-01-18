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
    public float[] skills = { 0f, 0f, 0f }; // 0=Speed, 1=Str, 2=Int (current XP)
    public int[] levels = { 1, 1, 1 }; // Current levels
    public float baseXpToNext = 100f; // Scales: Lv2=150, Lv3=200, etc.
    public Slider[] skillSliders;
    public TextMeshProUGUI[] skillLabels;

    [Header("UI")]
    public CanvasGroup pauseCanvasGroup;
    public GameObject pausePanel;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad fix later – see Step 4
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
        UpdateMoneyUI();
        UpdateSkillUI();
    }

    // ENHANCED: XP Gain + Auto-Level
    public void AddXP(int skillIndex, float amount)
    {
        if (skillIndex < 0 || skillIndex > 2) return;

        skills[skillIndex] += amount;
        CheckLevelUp(skillIndex);
        UpdateSkillUI(); // Refresh bars/labels
    }

    void CheckLevelUp(int skillIndex)
    {
        float xpNeeded = baseXpToNext + (levels[skillIndex] - 1) * 50f; // Scales up
        while (skills[skillIndex] >= xpNeeded)
        {
            skills[skillIndex] -= xpNeeded; // Overflow to next bar
            levels[skillIndex]++;
            xpNeeded = baseXpToNext + (levels[skillIndex] - 1) * 50f; // Recalc next
            Debug.Log($"Level Up! {GetSkillName(skillIndex)} Lv{levels[skillIndex]}");
            // Optional: Particle effect or UI popup here
        }
    }

    string GetSkillName(int index) => index switch
    {
        0 => "Speed",
        1 => "Strength",
        2 => "Intelligence",
        _ => "Unknown"
    };

    void UpdateMoneyUI()
    {
        if (moneyText) moneyText.text = "$ " + money.ToString("N0");
    }

    void UpdateSkillUI()
    {
        for (int i = 0; i < 3; i++)
        {
            float xpNeeded = baseXpToNext + (levels[i] - 1) * 50f;
            if (skillSliders[i]) skillSliders[i].value = skills[i] / xpNeeded;
            if (skillLabels[i])
                skillLabels[i].text = $"{GetSkillName(i)}: Lv {levels[i]} ({(int)skills[i]:F0}/{xpNeeded:F0})";
        }
    }

    public void AddMoney(int amount) { money += amount; }

    void TogglePause()
    {
        isPaused = !isPaused;
        pauseCanvasGroup.alpha = isPaused ? 1f : 0f;
        pauseCanvasGroup.interactable = isPaused;
        pauseCanvasGroup.blocksRaycasts = isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }
}