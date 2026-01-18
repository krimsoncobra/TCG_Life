using DG.Tweening; // DOTween
using UnityEngine;
using UnityEngine.SceneManagement; // ← ADDED: For SceneManager
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

    [Header("Clock UI")] // ← ADDED: Missing header/field
    public TextMeshProUGUI clockText;

    private float cycleTime = 0f;
    private Tween cycleTween;

    void Start()
    {
        // ← FIXED: Use FindFirstObjectByType (non-obsolete)
        if (sunLight == null)
            sunLight = Object.FindFirstObjectByType<Light>();

        // ← FIXED: Added proper fallback with SceneManager using
        if (sunLight == null)
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            sunLight = lights.Length > 0 ? lights[0] : null;
        }

        StartCycle();
    }

    void StartCycle()
    {
        cycleTween?.Kill();
        cycleTween = DOTween.To(() => cycleTime, x => cycleTime = x, 1f, dayDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .OnUpdate(UpdateCycle);
    }

    void UpdateCycle()
    {
        float normalizedTime = cycleTime % 1f;
        float sunAngle = normalizedTime * 360f;

        // Rotate sun
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 45f, 0f); // 45° tilt for realism

            // Dawn/Dusk transition (0.25-0.75 normalized)
            float t = Mathf.Clamp01(Mathf.Abs(normalizedTime - 0.5f) * 2f); // 0 at noon/midnight, 1 at dawn/dusk
            Color lightColor = Color.Lerp(nightColor, dayColor, t);
            float lightIntensity = Mathf.Lerp(nightIntensity, dayIntensity, t);

            sunLight.color = lightColor;
            sunLight.intensity = lightIntensity;
        }

        // ← FIXED: Clock update (now with proper field reference)
        UpdateClock();
    }

    // ← MOVED: Separate method for clock to avoid scope issues
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

    public float GetDayTimeHours() => (cycleTime % 1f) * 24f; // 0-24 for clock/phone
}