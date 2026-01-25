using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    [Header("Prefabs & Database")]
    [Tooltip("Drag your Card3D prefab here")]
    public Card3D cardPrefab;      // Instance field – now properly accessible

    [Tooltip("Drag your CardDatabase ScriptableObject here")]
    public CardDatabase database;

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = Vector3.up * 1f; // Where cards appear above spawner

    // ────────────────────────────────────────────────────────────────
    //  CONTEXT MENU COMMANDS (Now work correctly!)
    // ────────────────────────────────────────────────────────────────

    [ContextMenu("Spawn Random Card")]
    public void SpawnRandomCard()
    {
        if (!ValidateSetup()) return;

        CardData randomCard = database.GetRandomCard();
        SpawnCard(randomCard, transform.position + spawnOffset, Quaternion.identity);
    }

    [ContextMenu("Spawn Specific Card by ID")]
    public void SpawnCardByID(int cardID)
    {
        if (!ValidateSetup()) return;

        CardData card = database.GetCardByID(cardID);
        if (card != null)
        {
            SpawnCard(card, transform.position + spawnOffset, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"No card found with ID: {cardID}");
        }
    }

    // ────────────────────────────────────────────────────────────────
    //  STATIC SPAWN METHOD (Can be called from anywhere)
    // ────────────────────────────────────────────────────────────────

    public static GameObject SpawnCard(CardData data, Vector3 position, Quaternion rotation)
    {
        if (data == null)
        {
            Debug.LogError("Cannot spawn card – CardData is null!");
            return null;
        }

        // Instantiate the prefab
        GameObject cardObj = Instantiate(FindObjectOfType<CardSpawner>()?.cardPrefab.gameObject, position, rotation);
        if (cardObj == null)
        {
            Debug.LogError("Card prefab not assigned in CardSpawner!");
            return null;
        }

        Card3D card3D = cardObj.GetComponent<Card3D>();
        if (card3D != null)
        {
            card3D.cardData = data;
            card3D.InitializeCard(); // Now public – calls safely
        }

        // Tag for interaction
        cardObj.tag = "Interactable";

        Debug.Log($"Spawned card: {data.GetFullCardName()} at {position}");

        return cardObj;
    }

    // ────────────────────────────────────────────────────────────────
    //  HELPER
    // ────────────────────────────────────────────────────────────────

    private bool ValidateSetup()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("CardSpawner: cardPrefab is not assigned in Inspector!");
            return false;
        }
        if (database == null)
        {
            Debug.LogError("CardSpawner: CardDatabase is not assigned!");
            return false;
        }
        return true;
    }
}