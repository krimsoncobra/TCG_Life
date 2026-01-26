using UnityEngine;

/// <summary>
/// Attach to Player GameObject - automatically sets up layer and tag
/// Ensures player is properly ignored by interaction raycast
/// </summary>
public class PlayerLayerSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Automatically set player to 'Player' layer")]
    public bool autoSetLayer = true;

    [Tooltip("Automatically set player tag to 'Player'")]
    public bool autoSetTag = true;

    [Tooltip("Apply to all child objects too")]
    public bool applyToChildren = true;

    void Awake()
    {
        SetupPlayer();
    }

    void SetupPlayer()
    {
        // Set layer
        if (autoSetLayer)
        {
            int playerLayer = LayerMask.NameToLayer("Player");

            if (playerLayer == -1)
            {
                Debug.LogError("❌ 'Player' layer doesn't exist! Create it in Edit → Project Settings → Tags and Layers");
            }
            else
            {
                gameObject.layer = playerLayer;
                Debug.Log($"✅ Set {gameObject.name} to 'Player' layer");

                if (applyToChildren)
                {
                    SetLayerRecursively(gameObject, playerLayer);
                }
            }
        }

        // Set tag
        if (autoSetTag)
        {
            if (!CompareTag("Player"))
            {
                gameObject.tag = "Player";
                Debug.Log($"✅ Set {gameObject.name} tag to 'Player'");
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    [ContextMenu("Setup Player Now")]
    public void ManualSetup()
    {
        SetupPlayer();
    }
}