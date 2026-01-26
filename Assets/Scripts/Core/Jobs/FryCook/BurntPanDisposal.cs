using UnityEngine;

/// <summary>
/// Handles disposal of burnt burger pan - when trashed, spawns a clean pan in player's hands
/// Attach this to the BurntBurgerPan prefab
/// </summary>
public class BurntPanDisposal : MonoBehaviour
{
    [Header("Clean Pan Replacement")]
    [Tooltip("Prefab of a clean cooking pan to spawn when burnt pan is trashed")]
    public GameObject cleanPanPrefab;

    private bool hasBeenDisposed = false;

    /// <summary>
    /// Called when this item is placed in trash
    /// Should be called by TrashCan script when item is disposed
    /// </summary>
    public void OnDisposed()
    {
        if (hasBeenDisposed)
        {
            Debug.LogWarning("⚠️ OnDisposed called but already disposed!");
            return;
        }

        hasBeenDisposed = true;
        Debug.Log("🗑️ BurntPanDisposal.OnDisposed() called!");

        // Spawn clean pan in player's hands BEFORE destroying this object
        SpawnCleanPan();

        Debug.Log("🗑️ About to destroy burnt pan...");

        // Destroy this burnt pan after a short delay to ensure spawn completes
        Destroy(gameObject, 0.2f);
    }

    void SpawnCleanPan()
    {
        Debug.Log("🔧 SpawnCleanPan() called");

        if (cleanPanPrefab == null)
        {
            Debug.LogError("❌ Clean Pan Prefab not assigned! Assign in Inspector on BurntPanDisposal component!");
            return;
        }

        Debug.Log($"✅ Clean Pan Prefab found: {cleanPanPrefab.name}");

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ Could not find Player GameObject with tag 'Player'!");
            return;
        }

        Debug.Log($"✅ Player found: {player.name}");

        // Spawn clean pan in front of player
        Vector3 spawnPos = player.transform.position + player.transform.forward * 1.5f;
        spawnPos.y = player.transform.position.y + 1f;

        Debug.Log($"📍 Spawning clean pan at position: {spawnPos}");

        GameObject cleanPan = Instantiate(cleanPanPrefab, spawnPos, Quaternion.identity);

        if (cleanPan == null)
        {
            Debug.LogError("❌ Failed to instantiate clean pan!");
            return;
        }

        Debug.Log($"✨ Successfully spawned clean pan: {cleanPan.name}");

        // Optionally auto-equip it
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            Debug.Log("🤲 Attempting to auto-equip clean pan...");

            // Try to auto-pickup the clean pan using TryPickup
            bool pickedUp = PlayerHands.Instance.TryPickup(cleanPan);
            if (pickedUp)
            {
                Debug.Log("✅ Auto-equipped clean pan to player hands");
            }
            else
            {
                Debug.LogWarning("⚠️ Failed to auto-equip clean pan - player might need to pick it up manually");
            }
        }
        else
        {
            Debug.Log("ℹ️ Player hands are full or PlayerHands.Instance is null - clean pan left on ground");
        }
    }

    /// <summary>
    /// Alternative: If your trash can uses a different method name, you can call OnDisposed from there
    /// Or we can detect collision with trash can here
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // If trash can doesn't call OnDisposed, we can detect it here
        if (other.CompareTag("Trash") || other.name.Contains("Trash"))
        {
            Debug.Log("🗑️ Burnt pan entered trash trigger");
            // Wait a moment to ensure the trash can's logic runs first
            Invoke(nameof(OnDisposed), 0.1f);
        }
    }
}