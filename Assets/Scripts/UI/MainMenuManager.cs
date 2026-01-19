using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public CanvasGroup mainMenuPanel;
    public CanvasGroup settingsPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button collectionButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton; // For settings panel

    [Header("Settings (Optional)")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    [Header("Scene Settings")]
    public string gameSceneName = "TownHub"; // Your main game scene

    [Header("Animation")]
    public float fadeDuration = 0.5f;
    public float buttonAnimDelay = 0.1f;

    void Start()
    {
        // Setup button listeners
        playButton.onClick.AddListener(OnPlayClicked);
        collectionButton.onClick.AddListener(OnCollectionClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Setup settings (if you have them)
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);

        // Initial state
        ShowMainMenu();

        // Animate menu entrance
        AnimateMenuEntrance();
    }

    // ═══════════════════════════════════════════════════════════════
    //  BUTTON CALLBACKS
    // ═══════════════════════════════════════════════════════════════

    void OnPlayClicked()
    {
        Debug.Log("Play clicked - Loading game...");

        // Fade out and load game scene
        mainMenuPanel.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }

    void OnCollectionClicked()
    {
        Debug.Log("Collection clicked");
        // TODO: Open collection viewer or load collection scene
        // For now, could load a separate scene or show a panel
    }

    void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
        ShowSettings();
    }

    void OnQuitClicked()
    {
        Debug.Log("Quit clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnBackClicked()
    {
        ShowMainMenu();
    }

    // ═══════════════════════════════════════════════════════════════
    //  SETTINGS CALLBACKS
    // ═══════════════════════════════════════════════════════════════

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        // TODO: Save to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // ═══════════════════════════════════════════════════════════════
    //  PANEL MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
        HidePanel(settingsPanel);
    }

    void ShowSettings()
    {
        HidePanel(mainMenuPanel);
        ShowPanel(settingsPanel);

        // Load saved settings
        if (volumeSlider != null)
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    void ShowPanel(CanvasGroup panel)
    {
        if (panel == null) return;

        panel.DOFade(1f, fadeDuration);
        panel.interactable = true;
        panel.blocksRaycasts = true;
    }

    void HidePanel(CanvasGroup panel)
    {
        if (panel == null) return;

        panel.DOFade(0f, fadeDuration);
        panel.interactable = false;
        panel.blocksRaycasts = false;
    }

    // ═══════════════════════════════════════════════════════════════
    //  ANIMATIONS
    // ═══════════════════════════════════════════════════════════════

    void AnimateMenuEntrance()
    {
        // Fade in main panel
        mainMenuPanel.alpha = 0f;
        mainMenuPanel.DOFade(1f, fadeDuration);

        // Animate buttons with stagger effect
        Button[] buttons = { playButton, collectionButton, settingsButton, quitButton };

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            // Start buttons off-screen or scaled down
            Transform btnTransform = buttons[i].transform;
            Vector3 originalScale = btnTransform.localScale;
            btnTransform.localScale = Vector3.zero;

            // Animate in with delay
            float delay = i * buttonAnimDelay;
            btnTransform.DOScale(originalScale, fadeDuration)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  BUTTON HOVER EFFECTS (Optional - Add to buttons in inspector)
    // ═══════════════════════════════════════════════════════════════

    public void OnButtonHoverEnter(Transform button)
    {
        button.DOScale(1.05f, 0.2f).SetEase(Ease.OutQuad);
    }

    public void OnButtonHoverExit(Transform button)
    {
        button.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
    }
}