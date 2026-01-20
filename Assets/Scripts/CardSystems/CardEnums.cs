using UnityEngine;

/// <summary>
/// Card rarity tiers - affects pull rates and value
/// </summary>
public enum CardRarity
{
    Common,      // 60% pull rate
    Uncommon,    // 30% pull rate
    Rare,        // 9% pull rate
    Extinct      // 1% pull rate - prehistoric/endangered animals
}

/// <summary>
/// Card visual types - different art treatments
/// </summary>
public enum CardType
{
    Regular,      // Standard card art
    ReverseHolo,  // Holographic background, regular image
    HoloRare,     // Holographic image
    FullArt,      // Art extends to edges, no template
    GoldenArt     // Full art with golden foil treatment (ultra rare)
}

/// <summary>
/// Card stats used in battle
/// </summary>
public enum CardStat
{
    Power,
    Speed,
    Intelligence
}

/// <summary>
/// Helper class for card stat utilities
/// </summary>
public static class CardStatHelper
{
    public static Color GetStatColor(CardStat stat)
    {
        return stat switch
        {
            CardStat.Power => new Color(0.9f, 0.2f, 0.2f),        // Red
            CardStat.Speed => new Color(0.2f, 0.8f, 0.3f),        // Green
            CardStat.Intelligence => new Color(0.3f, 0.5f, 0.9f), // Blue
            _ => Color.white
        };
    }

    public static string GetStatIcon(CardStat stat)
    {
        return stat switch
        {
            CardStat.Power => "💪",
            CardStat.Speed => "⚡",
            CardStat.Intelligence => "🧠",
            _ => "?"
        };
    }
}