using UnityEngine;

/// <summary>
/// Serving Tray Station - Spawns serving trays for the player
/// Similar to PanStation and BasketStation
/// </summary>
public class ServingTrayStation : MonoBehaviour, IInteractable
{
    [Header("Prefab Settings")]
    [Tooltip("The serving tray prefab to spawn")]
    public GameObject servingTrayPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Where trays spawn")]
    public Transform spawnPoint;

    [Header("Inventory")]
    [Tooltip("Number of trays available (-1 = infinite)")]
    public int trayCount = -1; // -1 = infinite

    [Header("Visual Feedback")]
    public ParticleSystem spawnEffect; // Optional: particles when grabbing tray

    public string GetPromptText()
    {
        // Empty hands → grab tray
        if (PlayerHands.Instance != null && !PlayerHands.Instance.IsHoldingSomething())
        {
            if (trayCount == -1)
                return "E to Grab Tray";
            else if (trayCount > 0)
                return $"E to Grab Tray ({trayCount})";
            else
                return "No Trays Left!";
        }

        return "Tray Station";
    }

    public void Interact()
    {
        if (PlayerHands.Instance == null) return;

        // Only grab if hands are empty
        if (!PlayerHands.Instance.IsHoldingSomething())
        {
            // Check if we have trays
            if (trayCount == 0)
            {
                Debug.LogWarning("⚠️ No trays left!");
                return;
            }

            if (servingTrayPrefab == null)
            {
                Debug.LogError("❌ No tray prefab assigned!");
                return;
            }

            // Spawn tray
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up;
            Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            GameObject tray = Instantiate(servingTrayPrefab, spawnPos, spawnRot);

            // Try to pick it up
            if (PlayerHands.Instance.TryPickup(tray))
            {
                // Decrease count (unless infinite)
                if (trayCount > 0)
                {
                    trayCount--;
                    Debug.Log($"🍽️ Grabbed tray! ({trayCount} remaining)");
                }
                else
                {
                    Debug.Log("🍽️ Grabbed tray!");
                }

                // Play effect
                if (spawnEffect != null)
                {
                    spawnEffect.Play();
                }
            }
            else
            {
                // Failed to pick up, destroy it
                Destroy(tray);
                Debug.LogWarning("⚠️ Failed to pickup tray!");
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw spawn point indicator
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(spawnPoint.position, new Vector3(0.3f, 0.05f, 0.3f));
        }
    }
}