using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Add this to any button to get smooth scale/animation effects on hover
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [Tooltip("Scale multiplier on hover (1.1 = 10% bigger)")]
    public float hoverScale = 1.05f;

    [Tooltip("Animation duration in seconds")]
    public float animDuration = 0.2f;

    [Tooltip("Optional: Play sound on hover")]
    public AudioClip hoverSound;

    private Vector3 originalScale;
    private AudioSource audioSource;

    void Awake()
    {
        originalScale = transform.localScale;

        // Setup audio if hover sound is assigned
        if (hoverSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = hoverSound;
            audioSource.volume = 0.3f;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Scale up
        transform.DOScale(originalScale * hoverScale, animDuration)
            .SetEase(Ease.OutQuad);

        // Play sound
        if (audioSource != null)
            audioSource.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Scale back to normal
        transform.DOScale(originalScale, animDuration)
            .SetEase(Ease.OutQuad);
    }

    void OnDisable()
    {
        // Reset scale when disabled
        transform.DOKill();
        transform.localScale = originalScale;
    }
}