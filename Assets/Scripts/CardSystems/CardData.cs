using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single card's data.
/// Create instances via: Right-click → Create → IRLTCG → Card Data
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "IRLTCG/Card Data", order = 1)]
public class CardData : ScriptableObject
{
    [Header("Card Identity")]
    [Tooltip("Display name (e.g. 'Horse', 'Cow', 'T-Rex')")]
    public string cardName;

    [Tooltip("Unique ID for this card (1-80)")]
    [Range(1, 80)]
    public int cardID;

    [Tooltip("Is this a stat modifier card instead of an animal?")]
    public bool isStatCard = false;

    [Header("Card Classification")]
    public CardRarity rarity = CardRarity.Common;
    public CardType cardType = CardType.Regular;

    [Header("Card Stats (1-10 Scale)")]
    [Tooltip("Health - How much damage before knockout")]
    [Range(1, 10)]
    public int health = 5;

    [Tooltip("Power - Physical strength in battle")]
    [Range(1, 10)]
    public int powerStat = 5;

    [Tooltip("Speed - Agility and quickness")]
    [Range(1, 10)]
    public int speedStat = 5;

    [Tooltip("Intelligence - Mental capability")]
    [Range(1, 10)]
    public int intelligenceStat = 5;

    [Header("Card Visuals")]
    [Tooltip("Main card artwork (render from Blender)")]
    public Sprite cardArtwork;

    [Tooltip("3D model for in-world representation (optional)")]
    public GameObject cardModel3D;

    [Tooltip("VFX prefab for holographic effects (for Holo/Full Art types)")]
    public GameObject holoEffectPrefab;

    [Header("Card Information")]
    [Tooltip("Fun fact about this animal (educational flavor text)")]
    [TextArea(3, 6)]
    public string funFact;

    [Tooltip("Scientific name (optional)")]
    public string scientificName;

    [Header("Economy")]
    [Tooltip("Base value when selling to shop")]
    public int baseValue = 10;

    // ═══════════════════════════════════════════════════════════════
    //  HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get stat value by enum
    /// </summary>
    public int GetStat(CardStat stat)
    {
        return stat switch
        {
            CardStat.Power => powerStat,
            CardStat.Speed => speedStat,
            CardStat.Intelligence => intelligenceStat,
            _ => 0
        };
    }

    /// <summary>
    /// Get total stat points (useful for overall card power level)
    /// </summary>
    public int GetTotalStats()
    {
        return powerStat + speedStat + intelligenceStat;
    }

    /// <summary>
    /// Calculate card's market value based on rarity and type
    /// </summary>
    public int GetMarketValue()
    {
        float multiplier = 1f;

        // Rarity multiplier
        multiplier *= rarity switch
        {
            CardRarity.Common => 1f,
            CardRarity.Uncommon => 2f,
            CardRarity.Rare => 5f,
            CardRarity.Extinct => 10f,
            _ => 1f
        };

        // Type multiplier
        multiplier *= cardType switch
        {
            CardType.Regular => 1f,
            CardType.ReverseHolo => 1.5f,
            CardType.HoloRare => 2f,
            CardType.FullArt => 3f,
            CardType.GoldenArt => 5f,
            _ => 1f
        };

        return Mathf.RoundToInt(baseValue * multiplier);
    }

    /// <summary>
    /// Get full card identifier (e.g. "Horse #023 [Holo Rare]")
    /// </summary>
    public string GetFullCardName()
    {
        string typeSuffix = cardType != CardType.Regular ? $" [{cardType}]" : "";
        return $"{cardName} #{cardID:D3}{typeSuffix}";
    }

    /// <summary>
    /// Check if this card variant is considered "premium"
    /// </summary>
    public bool IsPremiumCard()
    {
        return cardType == CardType.FullArt || cardType == CardType.GoldenArt;
    }

    /// <summary>
    /// Get stat distribution as string (for debugging/display)
    /// </summary>
    public string GetStatSummary()
    {
        return $"HP:{health} | PWR:{powerStat} | SPD:{speedStat} | INT:{intelligenceStat}";
    }

    // ═══════════════════════════════════════════════════════════════
    //  EDITOR VALIDATION
    // ═══════════════════════════════════════════════════════════════

    void OnValidate()
    {
        // Auto-set base value based on rarity if it's at default
        if (baseValue == 10)
        {
            baseValue = rarity switch
            {
                CardRarity.Common => 10,
                CardRarity.Uncommon => 25,
                CardRarity.Rare => 75,
                CardRarity.Extinct => 200,
                _ => 10
            };
        }

        // Ensure stats are within 1-10 range
        health = Mathf.Clamp(health, 1, 10);
        powerStat = Mathf.Clamp(powerStat, 1, 10);
        speedStat = Mathf.Clamp(speedStat, 1, 10);
        intelligenceStat = Mathf.Clamp(intelligenceStat, 1, 10);
    }
}