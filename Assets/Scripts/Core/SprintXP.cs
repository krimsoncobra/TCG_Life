using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class SprintXP : MonoBehaviour
{
    [Header("XP Gain")]
    public float xpPerSecond = 1f; // ~1 XP/sec sprinting

    private ThirdPersonController playerController;
    private PlayerStats stats;
    private StarterAssetsInputs input; // Direct ref to inputs

    void Start()
    {
        playerController = GetComponent<ThirdPersonController>();
        stats = PlayerStats.Instance;
        input = GetComponent<StarterAssetsInputs>();

        if (playerController == null) Debug.LogError("SprintXP: No ThirdPersonController on this GameObject!");
        if (stats == null) Debug.LogError("SprintXP: PlayerStats.Instance is null!");
        if (input == null) Debug.LogError("SprintXP: No StarterAssetsInputs on this GameObject!");
    }

    void Update()
    {
        // Log sprint state every 1 sec for testing
        if (Time.frameCount % 60 == 0) // ~1/sec
        {
            Debug.Log($"SprintXP Debug: Sprint Input = {input?.sprint}, Controller Sprint = {playerController?.IsSprinting()}, Stats = {(stats != null ? "OK" : "NULL")}");
        }

        if (playerController != null && playerController.IsSprinting() && stats != null)
        {
            float xpThisFrame = xpPerSecond * Time.deltaTime;
            stats.AddXP(0, xpThisFrame); // Speed index 0
            Debug.Log($"Added {xpThisFrame:F2} XP to Speed. Total: {stats.skills[0]:F1}");
        }
    }
}