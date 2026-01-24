using UnityEngine;

public class BurgerPatty : FoodItem
{
    [Header("Pickup Settings")]
    public Vector3 handOffset = Vector3.zero;
    public Vector3 handRotation = Vector3.zero;
    public Vector3 handScale = Vector3.one;

    [Header("Visual Feedback")]
    public Renderer foodRenderer;
    public Color rawColor = new Color(0.8f, 0.3f, 0.3f);
    public Color cookedColor = new Color(0.5f, 0.3f, 0.2f);
    public Color burntColor = new Color(0.1f, 0.05f, 0.05f);

    void Start()
    {
        foodName = "Burger Patty";
        currentState = CookingState.Prepared;
        cookTime = 5f;
        burnTime = 3f;
        flipBonus = 3f;
    }

    void LateUpdate()
    {
        // Handle cooking logic directly here
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
                    Debug.Log("🍔 Burger is COOKED!");
                }
            }
            // Check if burning
            else if (currentState == CookingState.Cooked)
            {
                float effectiveBurnTime = burnTime + flipBonusTimer;
                if (currentCookTimer >= effectiveBurnTime)
                {
                    currentState = CookingState.Burnt;
                    Debug.Log("🔥 Burger BURNT!");
                }
            }
        }

        // Countdown flip bonus
        if (flipBonusTimer > 0f)
        {
            flipBonusTimer -= Time.deltaTime;
        }

        // Update visuals
        UpdateVisual();

        // Debug
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🍔 State: {currentState}, Timer: {currentCookTimer:F1}, IsOnGrill: {isOnGrill}");
        }
    }

    void UpdateVisual()
    {
        if (foodRenderer == null) return;

        switch (currentState)
        {
            case CookingState.Raw:
            case CookingState.Prepared:
                foodRenderer.material.color = rawColor;
                break;

            case CookingState.Cooking:
                float cookProgress = GetCookProgress();
                foodRenderer.material.color = Color.Lerp(rawColor, cookedColor, cookProgress);
                break;

            case CookingState.Cooked:
                float burnProgress = GetBurnProgress();
                foodRenderer.material.color = Color.Lerp(cookedColor, burntColor, burnProgress);
                break;

            case CookingState.Burnt:
                foodRenderer.material.color = burntColor;
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