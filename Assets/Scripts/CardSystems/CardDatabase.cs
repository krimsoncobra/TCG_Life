using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Central database of all cards in the game.
/// Singleton pattern for easy access from anywhere.
/// Create via: Right-click → Create → IRLTCG → Card Database
/// </summary>
[CreateAssetMenu(fileName = "CardDatabase", menuName = "IRLTCG/Card Database", order = 0)]
public class CardDatabase : ScriptableObject
{
    [Header("All Cards")]
    [Tooltip("Drag all CardData assets here")]
    public List<CardData> allCards = new List<CardData>();

    [Header("Quick Reference Lists")]
    [Tooltip("These auto-populate when you click 'Refresh Lists' below")]
    public List<CardData> commonCards = new List<CardData>();
    public List<CardData> uncommonCards = new List<CardData>();
    public List<CardData> rareCards = new List<CardData>();
    public List<CardData> extinctCards = new List<CardData>();

    // ═══════════════════════════════════════════════════════════════
    //  LOOKUP METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get card by ID
    /// </summary>
    public CardData GetCardByID(int id)
    {
        return allCards.FirstOrDefault(card => card.cardID == id);
    }

    /// <summary>
    /// Get card by name
    /// </summary>
    public CardData GetCardByName(string name)
    {
        return allCards.FirstOrDefault(card => card.cardName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all cards of a specific rarity
    /// </summary>
    public List<CardData> GetCardsByRarity(CardRarity rarity)
    {
        return allCards.Where(card => card.rarity == rarity).ToList();
    }

    /// <summary>
    /// Get all cards of a specific type
    /// </summary>
    public List<CardData> GetCardsByType(CardType type)
    {
        return allCards.Where(card => card.cardType == type).ToList();
    }

    /// <summary>
    /// Get random card based on rarity pull rates
    /// Common: 60%, Uncommon: 30%, Rare: 9%, Extinct: 1%
    /// </summary>
    public CardData GetRandomCard()
    {
        float roll = Random.value;

        CardRarity targetRarity;
        if (roll < 0.60f) // 60%
            targetRarity = CardRarity.Common;
        else if (roll < 0.90f) // 30%
            targetRarity = CardRarity.Uncommon;
        else if (roll < 0.99f) // 9%
            targetRarity = CardRarity.Rare;
        else // 1%
            targetRarity = CardRarity.Extinct;

        List<CardData> pool = GetCardsByRarity(targetRarity);

        if (pool.Count == 0)
        {
            Debug.LogWarning($"No cards found for rarity: {targetRarity}. Returning random from all cards.");
            return allCards[Random.Range(0, allCards.Count)];
        }

        return pool[Random.Range(0, pool.Count)];
    }

    /// <summary>
    /// Get random card with specific type variant (e.g. guarantee a Holo)
    /// </summary>
    public CardData GetRandomCardWithType(CardType type)
    {
        CardData baseCard = GetRandomCard();

        // Find or create variant with desired type
        CardData variant = allCards.FirstOrDefault(c =>
            c.cardID == baseCard.cardID && c.cardType == type);

        return variant ?? baseCard; // Fallback to base if variant doesn't exist
    }

    // ═══════════════════════════════════════════════════════════════
    //  EDITOR UTILITIES
    // ═══════════════════════════════════════════════════════════════

    [ContextMenu("Refresh Rarity Lists")]
    public void RefreshRarityLists()
    {
        commonCards = GetCardsByRarity(CardRarity.Common);
        uncommonCards = GetCardsByRarity(CardRarity.Uncommon);
        rareCards = GetCardsByRarity(CardRarity.Rare);
        extinctCards = GetCardsByRarity(CardRarity.Extinct);

        Debug.Log($"Refreshed! Common: {commonCards.Count}, Uncommon: {uncommonCards.Count}, " +
                  $"Rare: {rareCards.Count}, Extinct: {extinctCards.Count}");
    }

    [ContextMenu("Validate Database")]
    public void ValidateDatabase()
    {
        // Check for duplicate IDs
        var duplicateIDs = allCards.GroupBy(c => c.cardID)
                                   .Where(g => g.Count() > 1)
                                   .Select(g => g.Key);

        if (duplicateIDs.Any())
        {
            Debug.LogError($"Duplicate IDs found: {string.Join(", ", duplicateIDs)}");
        }
        else
        {
            Debug.Log($"✓ Database valid! {allCards.Count} unique cards registered.");
        }
    }

    void OnValidate()
    {
        // Auto-sort by ID for easier management
        allCards = allCards.OrderBy(c => c.cardID).ThenBy(c => c.cardType).ToList();
    }
}