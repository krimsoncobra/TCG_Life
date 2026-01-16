using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // For new Input System

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance; // Singleton

    [Header("Money")]
    public int money = 100;
    public TextMeshProUGUI moneyText;

    [Header("Skills")]
    public float[] skills = { 0f, 0f, 0f }; // 0=Speed, 1=Str, 2=Int
    public float xpToNextLevel = 100f;
    public Slider[] skillSliders;
    public TextMeshProUGUI[] skillLabels;

    [Header("UI")]
    public CanvasGroup pauseCanvasGroup;
    public GameObject pausePanel; // Or use SetActive

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject); // Persists scenes
    }

    void Start()
    {
        UpdateMoneyUI();
        UpdateSkillUI();
    }

    void Update()
    {
        // ESC Pause (prototype – Input System later)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Auto-update every frame (simple for proto)
        UpdateMoneyUI();
        UpdateSkillUI();
    }

    void UpdateMoneyUI()
    {
        if (moneyText) moneyText.text = "$ " + money.ToString("N0");
    }

    void UpdateSkillUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (skillSliders[i]) skillSliders[i].value = skills[i] / xpToNextLevel;
            if (skillLabels[i])
            {
                int level = 1 + (int)(skills[i] / xpToNextLevel);
                skillLabels[i].text = new string[] { "Speed", "Strength", "Intelligence" }[i] +
                                     ": Lv " + level + " (" + (int)skills[i] + "/" + (int)xpToNextLevel + ")";
            }
        }
    }

    public void AddMoney(int amount) { money += amount; }
    public void AddXP(int skillIndex, float amount) { skills[skillIndex] += amount; }

    void TogglePause()
    {
        isPaused = !isPaused;
        pauseCanvasGroup.alpha = isPaused ? 1f : 0f;
        pauseCanvasGroup.interactable = isPaused;
        pauseCanvasGroup.blocksRaycasts = isPaused;
        Time.timeScale = isPaused ? 0f : 1f; // Pause game
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }
}