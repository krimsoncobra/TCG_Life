using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the player's card collection.
/// Tracks which cards they own and how many of each.
/// Singleton pattern for global access.
/// </summary>
public class PlayerCollection : MonoBehaviour
{
    public static PlayerCollection Instance;

    [Header("Collection Data")]
    [Tooltip("Dictionary of card IDs to quantity owned")]
    public Dictionary<int, int> ownedCards = new Dictionary<int, int>();

    [Header("Collection Stats")]
    [SerializeField] private int totalCardsOwned = 0;
    [SerializeField] private int uniqueCardsOwned = 0;

    [Header("References")]
    public CardDatabase cardDatabase;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadCollection(); // TODO: Implement save/load system
    }

    // ═══════════════════════════════════════════════════════════════
    //  COLLECTION MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Add a card to the collection
    /// </summary>
    public void AddCard(CardData card)
    {
        if (card == null) return;

        int cardID = card.cardID;

        if (ownedCards.ContainsKey(cardID))
        {
            ownedCards[cardID]++;
        }
        else
        {
            ownedCards[cardID] = 1;
            uniqueCardsOwned++;
        }

        totalCardsOwned++;

        Debug.Log($"Added {card.cardName} to collection! Total owned: {ownedCards[cardID]}");

        // Update player stats
        UpdateCollectionStats();
    }

    /// <summary>
    /// Remove a card from collection (selling, trading, etc.)
    /// </summary>
    public bool RemoveCard(CardData card)
    {
        if (card == null) return false;

        int cardID = card.cardID;

        if (!ownedCards.ContainsKey(cardID) || ownedCards[cardID] <= 0)
        {
            Debug.LogWarning($"Cannot remove {card.cardName} - not in collection!");
            return false;
        }

        ownedCards[cardID]--;
        totalCardsOwned--;

        if (ownedCards[cardID] == 0)
        {
            ownedCards.Remove(cardID);
            uniqueCardsOwned--;
        }

        UpdateCollectionStats();
        return true;
    }

    /// <summary>
    /// Check if player owns at least one of this card
    /// </summary>
    public bool OwnsCard(int cardID)
    {
        return ownedCards.ContainsKey(cardID) && ownedCards[cardID] > 0;
    }

    /// <summary>
    /// Get quantity of specific card owned
    /// </summary>
    public int GetCardCount(int cardID)
    {
        return ownedCards.ContainsKey(cardID) ? ownedCards[cardID] : 0;
    }

    /// <summary>
    /// Get all cards the player owns at least one of
    /// </summary>
    public List<CardData> GetOwnedCards()
    {
        if (cardDatabase == null) return new List<CardData>();

        List<CardData> owned = new List<CardData>();

        foreach (int cardID in ownedCards.Keys)
        {
            CardData card = cardDatabase.GetCardByID(cardID);
            if (card != null)
                owned.Add(card);
        }

        return owned;
    }

    // ═══════════════════════════════════════════════════════════════
    //  COLLECTION STATS
    // ═══════════════════════════════════════════════════════════════

    void UpdateCollectionStats()
    {
        // Update PlayerStats collector stat
        if (PlayerStats.Instance != null && cardDatabase != null)
        {
            int totalPossible = cardDatabase.allCards.Count;
            float completionPercent = (float)uniqueCardsOwned / totalPossible * 100f;

            // You could use a separate collector level system
            // For now, just track completion percentage
            Debug.Log($"Collection: {uniqueCardsOwned}/{totalPossible} ({completionPercent:F1}%)");
        }
    }

    /// <summary>
    /// Get collection completion percentage
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (cardDatabase == null) return 0f;

        int totalPossible = cardDatabase.allCards.Count;
        return totalPossible > 0 ? (float)uniqueCardsOwned / totalPossible * 100f : 0f;
    }

    /// <summary>
    /// Check if collection is complete
    /// </summary>
    public bool IsCollectionComplete()
    {
        return cardDatabase != null && uniqueCardsOwned >= cardDatabase.allCards.Count;
    }

    // ═══════════════════════════════════════════════════════════════
    //  SAVE/LOAD (Placeholder)
    // ═══════════════════════════════════════════════════════════════

    void LoadCollection()
    {
        // TODO: Implement PlayerPrefs or JSON save system
        Debug.Log("Collection loaded (placeholder)");
    }

    public void SaveCollection()
    {
        // TODO: Serialize ownedCards dictionary to JSON/PlayerPrefs
        Debug.Log("Collection saved (placeholder)");
    }

    // ═══════════════════════════════════════════════════════════════
    //  DEBUG UTILITIES
    // ═══════════════════════════════════════════════════════════════

    [ContextMenu("Add Random Card")]
    public void DebugAddRandomCard()
    {
        if (cardDatabase != null)
        {
            CardData randomCard = cardDatabase.GetRandomCard();
            AddCard(randomCard);
        }
    }

    [ContextMenu("Clear Collection")]
    public void DebugClearCollection()
    {
        ownedCards.Clear();
        totalCardsOwned = 0;
        uniqueCardsOwned = 0;
        Debug.Log("Collection cleared!");
    }

    [ContextMenu("Print Collection")]
    public void DebugPrintCollection()
    {
        Debug.Log($"=== COLLECTION ({totalCardsOwned} total, {uniqueCardsOwned} unique) ===");
        foreach (var kvp in ownedCards.OrderBy(x => x.Key))
        {
            CardData card = cardDatabase.GetCardByID(kvp.Key);
            string cardName = card != null ? card.cardName : "Unknown";
            Debug.Log($"#{kvp.Key:D3} {cardName}: x{kvp.Value}");
        }
    }
}