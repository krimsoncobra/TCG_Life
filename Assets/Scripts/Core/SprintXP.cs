using StarterAssets;
using UnityEngine;

public class SprintXP : MonoBehaviour
{
    [Header("XP Settings")]
    [Tooltip("XP gained per second while sprinting and actually moving")]
    public float xpPerSecond = 1f;

    [Header("Debug")]
    public bool showDebugLogs = true; // Turn off after testing

    private ThirdPersonController playerController;
    private PlayerStats stats;
    private StarterAssetsInputs input;

    private float lastLogTime;

    void Start()
    {
        playerController = GetComponent<ThirdPersonController>();
        stats = PlayerStats.Instance;
        input = GetComponent<StarterAssetsInputs>();

        if (playerController == null || stats == null || input == null)
        {
            Debug.LogError("SprintXP: Missing required component!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        bool isSprinting = playerController.IsSprinting();           // Uses your public method
        bool isMoving = input.move.sqrMagnitude > 0.01f;             // Actually trying to move
        bool shouldGainXP = isSprinting && isMoving;

        // Debug output (every second)
        if (showDebugLogs && Time.time - lastLogTime > 1f)
        {
            Debug.Log($"SprintXP → Sprinting: {isSprinting} | Moving: {isMoving} | Gaining XP: {shouldGainXP}");
            lastLogTime = Time.time;
        }

        if (shouldGainXP)
        {
            float xpThisFrame = xpPerSecond * Time.deltaTime;
            stats.AddXP(0, xpThisFrame); // Speed = index 0

            if (showDebugLogs)
                Debug.Log($"→ Gained {xpThisFrame:F3} Speed XP (total: {stats.skills[0]:F1})");
        }
    }
}