using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Screen-space UI manager for cooking progress
/// Tracks all active cooking pans AND fryer baskets and displays their progress
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
    private Dictionary<CookingPan, CookingProgressBarUI> activePanBars = new Dictionary<CookingPan, CookingProgressBarUI>();

    // Track all active fryer baskets
    private Dictionary<FryerBasket, CookingProgressBarUI> activeBasketBars = new Dictionary<FryerBasket, CookingProgressBarUI>();

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

    // ═══════════════════════════════════════════════════════════════
    //  COOKING PAN METHODS (Original)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Register a pan that's cooking - call this when pan is placed on grill
    /// </summary>
    public void RegisterCookingPan(CookingPan pan)
    {
        if (pan == null || activePanBars.ContainsKey(pan)) return;

        // Create a new progress bar for this pan
        GameObject barObj = Instantiate(progressBarPrefab, progressContainer);
        CookingProgressBarUI barUI = barObj.GetComponent<CookingProgressBarUI>();

        if (barUI != null)
        {
            barUI.InitializeForPan(pan);
            activePanBars.Add(pan, barUI);

            Debug.Log($"✅ Registered cooking pan: {pan.name}");
        }
    }

    /// <summary>
    /// Unregister a pan - call this when pan is picked up or cooking stops
    /// </summary>
    public void UnregisterCookingPan(CookingPan pan)
    {
        if (pan == null || !activePanBars.ContainsKey(pan)) return;

        CookingProgressBarUI barUI = activePanBars[pan];
        if (barUI != null)
        {
            Destroy(barUI.gameObject);
        }

        activePanBars.Remove(pan);
        Debug.Log($"🔽 Unregistered cooking pan: {pan.name}");
    }

    // ═══════════════════════════════════════════════════════════════
    //  FRYER BASKET METHODS (NEW!)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Register a fryer basket that's cooking - call this when basket is placed in fryer
    /// </summary>
    public void RegisterFryerBasket(FryerBasket basket)
    {
        if (basket == null || activeBasketBars.ContainsKey(basket)) return;

        // Create a new progress bar for this basket
        GameObject barObj = Instantiate(progressBarPrefab, progressContainer);
        CookingProgressBarUI barUI = barObj.GetComponent<CookingProgressBarUI>();

        if (barUI != null)
        {
            barUI.InitializeForBasket(basket);
            activeBasketBars.Add(basket, barUI);

            Debug.Log($"✅ Registered fryer basket: {basket.name}");
        }
    }

    /// <summary>
    /// Unregister a basket - call this when basket is picked up or cooking stops
    /// </summary>
    public void UnregisterFryerBasket(FryerBasket basket)
    {
        if (basket == null || !activeBasketBars.ContainsKey(basket)) return;

        CookingProgressBarUI barUI = activeBasketBars[basket];
        if (barUI != null)
        {
            Destroy(barUI.gameObject);
        }

        activeBasketBars.Remove(basket);
        Debug.Log($"🔽 Unregistered fryer basket: {basket.name}");
    }

    // ═══════════════════════════════════════════════════════════════
    //  UPDATE METHODS
    // ═══════════════════════════════════════════════════════════════

    void UpdateAllBars()
    {
        // Clean up any null PAN entries
        List<CookingPan> pansToRemove = new List<CookingPan>();
        foreach (var kvp in activePanBars)
        {
            if (kvp.Key == null || kvp.Value == null || !kvp.Key.isOnGrill)
            {
                pansToRemove.Add(kvp.Key);
            }
        }

        foreach (var pan in pansToRemove)
        {
            UnregisterCookingPan(pan);
        }

        // Clean up any null BASKET entries
        List<FryerBasket> basketsToRemove = new List<FryerBasket>();
        foreach (var kvp in activeBasketBars)
        {
            if (kvp.Key == null || kvp.Value == null || !kvp.Key.isInFryer)
            {
                basketsToRemove.Add(kvp.Key);
            }
        }

        foreach (var basket in basketsToRemove)
        {
            UnregisterFryerBasket(basket);
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

        // Update each bar's content (both pans and baskets)
        foreach (var kvp in activePanBars)
        {
            kvp.Value.UpdateDisplay();
        }

        foreach (var kvp in activeBasketBars)
        {
            kvp.Value.UpdateDisplay();
        }
    }

    void UpdateTopOfScreenLayout()
    {
        // Stack bars vertically at top of screen
        int index = 0;

        // Position pan bars
        foreach (var barUI in activePanBars.Values)
        {
            RectTransform rect = barUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0, -20 - (index * barSpacing));
            }
            index++;
        }

        // Position basket bars (continue from where pans left off)
        foreach (var barUI in activeBasketBars.Values)
        {
            RectTransform rect = barUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(0, -20 - (index * barSpacing));
            }
            index++;
        }
    }

    void UpdateFollowPanLayout()
    {
        // Position each PAN bar above its corresponding pan in screen space
        foreach (var kvp in activePanBars)
        {
            CookingPan pan = kvp.Key;
            CookingProgressBarUI barUI = kvp.Value;

            if (pan != null && Camera.main != null)
            {
                PositionBarAboveObject(barUI, pan.transform);
            }
        }

        // Position each BASKET bar above its corresponding basket in screen space
        foreach (var kvp in activeBasketBars)
        {
            FryerBasket basket = kvp.Key;
            CookingProgressBarUI barUI = kvp.Value;

            if (basket != null && Camera.main != null)
            {
                PositionBarAboveObject(barUI, basket.transform);
            }
        }
    }

    void PositionBarAboveObject(CookingProgressBarUI barUI, Transform targetTransform)
    {
        if (Camera.main == null) return;

        // Get world position above the object
        Vector3 worldPos = targetTransform.position + worldOffset;

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
            // Object is behind camera, hide bar
            barUI.gameObject.SetActive(false);
        }
    }
}