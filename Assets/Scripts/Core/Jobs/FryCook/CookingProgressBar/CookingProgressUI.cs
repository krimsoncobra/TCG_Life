using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Screen-space UI manager for cooking progress
/// Tracks all active cooking pans and displays their progress
/// NO WORLDSPACE ISSUES!
/// </summary>
public class CookingProgressUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The panel that contains all progress bars")]
    public Transform progressContainer;

    [Tooltip("Prefab for a single progress bar entry")]
    public GameObject progressBarPrefab;

    [Header("Settings")]
    [Tooltip("Show progress bars at top of screen or follow pan position")]
    public DisplayMode displayMode = DisplayMode.TopOfScreen;

    [Tooltip("Vertical spacing between multiple progress bars")]
    public float barSpacing = 40f;

    [Header("Optional - For World Position Mode")]
    [Tooltip("Offset above the pan in world space (only used in FollowPan mode)")]
    public Vector3 worldOffset = new Vector3(0, 0.5f, 0);

    public enum DisplayMode
    {
        TopOfScreen,    // Stack bars at top of screen
        FollowPan       // Position each bar above its pan in screen space
    }

    // Track all active cooking pans
    private Dictionary<CookingPan, CookingProgressBarUI> activeBars = new Dictionary<CookingPan, CookingProgressBarUI>();

    // Singleton for easy access
    public static CookingProgressUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        UpdateAllBars();
    }

    /// <summary>
    /// Register a pan that's cooking - call this when pan is placed on grill
    /// </summary>
    public void RegisterCookingPan(CookingPan pan)
    {
        if (pan == null || activeBars.ContainsKey(pan)) return;

        // Create a new progress bar for this pan
        GameObject barObj = Instantiate(progressBarPrefab, progressContainer);
        CookingProgressBarUI barUI = barObj.GetComponent<CookingProgressBarUI>();

        if (barUI != null)
        {
            barUI.Initialize(pan);
            activeBars.Add(pan, barUI);

            Debug.Log($"Registered cooking pan: {pan.name}");
        }
    }

    /// <summary>
    /// Unregister a pan - call this when pan is picked up or cooking stops
    /// </summary>
    public void UnregisterCookingPan(CookingPan pan)
    {
        if (pan == null || !activeBars.ContainsKey(pan)) return;

        CookingProgressBarUI barUI = activeBars[pan];
        if (barUI != null)
        {
            Destroy(barUI.gameObject);
        }

        activeBars.Remove(pan);
        Debug.Log($"Unregistered cooking pan: {pan.name}");
    }

    void UpdateAllBars()
    {
        // Clean up any null entries
        List<CookingPan> toRemove = new List<CookingPan>();
        foreach (var kvp in activeBars)
        {
            if (kvp.Key == null || kvp.Value == null || !kvp.Key.isOnGrill)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var pan in toRemove)
        {
            UnregisterCookingPan(pan);
        }

        // Update positions based on display mode
        if (displayMode == DisplayMode.TopOfScreen)
        {
            UpdateTopOfScreenLayout();
        }
        else if (displayMode == DisplayMode.FollowPan)
        {
            UpdateFollowPanLayout();
        }

        // Update each bar's content
        foreach (var kvp in activeBars)
        {
            kvp.Value.UpdateDisplay();
        }
    }

    void UpdateTopOfScreenLayout()
    {
        // Stack bars vertically at top of screen
        int index = 0;
        foreach (var barUI in activeBars.Values)
        {
            RectTransform rect = barUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Position from top, stacked vertically
                rect.anchoredPosition = new Vector2(0, -20 - (index * barSpacing));
            }
            index++;
        }
    }

    void UpdateFollowPanLayout()
    {
        // Position each bar above its corresponding pan in screen space
        foreach (var kvp in activeBars)
        {
            CookingPan pan = kvp.Key;
            CookingProgressBarUI barUI = kvp.Value;

            if (pan != null && Camera.main != null)
            {
                // Get world position above the pan
                Vector3 worldPos = pan.transform.position + worldOffset;

                // Convert to screen position
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                // Check if behind camera
                if (screenPos.z > 0)
                {
                    // Convert screen position to canvas position
                    RectTransform rect = barUI.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            progressContainer as RectTransform,
                            screenPos,
                            null,
                            out Vector2 localPos
                        );

                        rect.anchoredPosition = localPos;
                        barUI.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // Pan is behind camera, hide bar
                    barUI.gameObject.SetActive(false);
                }
            }
        }
    }
}
