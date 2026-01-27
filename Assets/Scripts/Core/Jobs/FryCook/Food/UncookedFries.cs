using UnityEngine;

/// <summary>
/// Uncooked fries that need to be fried
/// Uses the FoodItem system for cooking
/// </summary>
public class UncookedFries : FoodItem
{
    [Header("Salting Status")]
    public bool isSalted = false;

    [Header("Pickup Settings")]
    public Vector3 handOffset = Vector3.zero;
    public Vector3 handRotation = Vector3.zero;
    public Vector3 handScale = Vector3.one;

    [Header("Visual Feedback")]
    public Renderer friesRenderer;
    public Color rawColor = new Color(0.95f, 0.9f, 0.7f); // Light potato color
    public Color cookingColor = new Color(0.95f, 0.85f, 0.5f); // Yellowing
    public Color cookedColor = new Color(0.9f, 0.7f, 0.2f); // Golden brown
    public Color burntColor = new Color(0.2f, 0.15f, 0.1f); // Dark brown/black

    void Start()
    {
        foodName = "French Fries";
        currentState = CookingState.Raw;
        cookTime = 4f; // 4 seconds to cook
        burnTime = 2f; // 2 seconds before burning
        flipBonus = 0f; // Fries don't need flipping
    }

    void LateUpdate()
    {
        // Handle cooking logic
        if (isOnGrill && currentState != CookingState.Burnt)
        {
            currentCookTimer += Time.deltaTime;

            // Start cooking
            if (currentState == CookingState.Prepared || currentState == CookingState.Raw)
            {
                currentState = CookingState.Cooking;
            }

            // Check if done cooking
            if (currentState == CookingState.Cooking)
            {
                if (currentCookTimer >= cookTime)
                {
                    currentState = CookingState.Cooked;
                    currentCookTimer = 0f;
                    Debug.Log("🍟 Fries are COOKED!");
                }
            }
            // Check if burning
            else if (currentState == CookingState.Cooked)
            {
                if (currentCookTimer >= burnTime)
                {
                    currentState = CookingState.Burnt;
                    Debug.Log("🔥 Fries BURNT!");
                }
            }
        }

        // Update visuals
        UpdateVisual();

        // Debug (every 60 frames)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🍟 State: {currentState}, Timer: {currentCookTimer:F1}, IsOnGrill: {isOnGrill}");
        }
    }

    void UpdateVisual()
    {
        if (friesRenderer == null) return;

        switch (currentState)
        {
            case CookingState.Raw:
            case CookingState.Prepared:
                friesRenderer.material.color = rawColor;
                break;

            case CookingState.Cooking:
                float cookProgress = GetCookProgress();
                friesRenderer.material.color = Color.Lerp(rawColor, cookedColor, cookProgress);
                break;

            case CookingState.Cooked:
                float burnProgress = GetBurnProgress();
                friesRenderer.material.color = Color.Lerp(cookedColor, burntColor, burnProgress);
                break;

            case CookingState.Burnt:
                friesRenderer.material.color = burntColor;
                break;
        }
    }

    public new void OnPickup(Transform handPosition)
    {
        base.OnPickup(handPosition);
        transform.localPosition = handOffset;
        transform.localRotation = Quaternion.Euler(handRotation);
    }
}