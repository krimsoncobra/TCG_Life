using DG.Tweening;
using UnityEngine;
using TMPro;

public class DayNightManager : MonoBehaviour
{
    [Header("Light")]
    public Light sunLight;
    public float dayDuration = 600f; // 10min day/night (real-time seconds)

    [Header("Colors")]
    public Color dayColor = new Color(1f, 0.95f, 0.8f); // Warm daylight
    public Color nightColor = new Color(0.1f, 0.1f, 0.3f); // Cool night
    public float dayIntensity = 1.5f;
    public float nightIntensity = 0.3f;

    [Header("Clock UI")]
    public TextMeshProUGUI clockText;

    private float cycleTime = 0f;
    private Tween cycleTween;

    void Start()
    {
        if (sunLight == null)
            sunLight = FindFirstObjectByType<Light>();

        if (sunLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            sunLight = lights.Length > 0 ? lights[0] : null;
        }

        // Load time from GameManager if it exists
        if (GameManager.Instance != null)
        {
            cycleTime = GameManager.Instance.dayNightCycle;
            Debug.Log($"⏰ Loaded cycle time from GameManager: {cycleTime:F2}");
        }

        StartCycle();
    }

    void StartCycle()
    {
        cycleTween?.Kill();

        // Start from current cycleTime (loaded from GameManager)
        float startTime = cycleTime % 1f;

        cycleTween = DOTween.To(() => cycleTime, x => cycleTime = x, cycleTime + 1f, dayDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .OnUpdate(UpdateCycle);

        Debug.Log($"⏰ Day/Night cycle started at {startTime:F2}");
    }

    void UpdateCycle()
    {
        float normalizedTime = cycleTime % 1f;
        float sunAngle = normalizedTime * 360f;

        // Rotate sun
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f);

            // Dawn/Dusk transition
            float t = Mathf.Clamp01(Mathf.Abs(normalizedTime - 0.5f) * 2f);
            Color lightColor = Color.Lerp(nightColor, dayColor, t);
            float lightIntensity = Mathf.Lerp(nightIntensity, dayIntensity, t);

            sunLight.color = lightColor;
            sunLight.intensity = lightIntensity;
        }

        // Update clock
        UpdateClock();

        // Save to GameManager periodically (every second)
        if (GameManager.Instance != null && Time.frameCount % 60 == 0)
        {
            GameManager.Instance.UpdateDayNightCycle(cycleTime);
        }
    }

    void UpdateClock()
    {
        if (clockText == null) return;

        float hours24 = GetDayTimeHours();
        string ampm = hours24 < 12 ? "AM" : "PM";
        int displayHours = Mathf.FloorToInt(hours24 % 12f);
        if (displayHours == 0) displayHours = 12;
        int minutes = Mathf.FloorToInt((hours24 % 1f) * 60f);
        clockText.text = $"{displayHours:D2}:{minutes:D2} {ampm}";
    }

    public float GetDayTimeHours() => (cycleTime % 1f) * 24f;

    public float GetCycleTime() => cycleTime;

    /// <summary>
    /// Set cycle time (called by GameManager when loading scene)
    /// </summary>
    public void SetCycleTime(float time)
    {
        cycleTime = time;
        Debug.Log($"⏰ Cycle time set to: {time:F2}");
        UpdateCycle(); // Immediate update
    }

    void OnDestroy()
    {
        // Save final time to GameManager before scene unloads
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateDayNightCycle(cycleTime);
        }

        cycleTween?.Kill();
    }
}