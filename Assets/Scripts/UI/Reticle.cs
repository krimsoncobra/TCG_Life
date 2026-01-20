using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple crosshair/reticle that shows center of screen.
/// Changes color when hovering over interactables.
/// </summary>
public class CrosshairReticle : MonoBehaviour
{
    [Header("Reticle Settings")]
    public Image reticleImage;
    public Color normalColor = new Color(1f, 1f, 1f, 0.5f); // White, semi-transparent
    public Color hoverColor = new Color(0f, 1f, 0f, 0.8f);  // Green when hovering
    public float size = 20f;

    [Header("Detection")]
    public float detectionRange = 4f;
    public LayerMask interactLayer;

    private Camera mainCam;
    private bool isHoveringInteractable = false;

    void Start()
    {
        mainCam = Camera.main;
        interactLayer = ~LayerMask.GetMask("Player");

        if (reticleImage != null)
        {
            reticleImage.color = normalColor;

            // Set size
            RectTransform rect = reticleImage.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
        }
    }

    void Update()
    {
        if (mainCam == null || reticleImage == null) return;

        // Check what we're aiming at
        CheckHover();
    }

    void CheckHover()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = mainCam.ScreenPointToRay(screenCenter);

        bool wasHovering = isHoveringInteractable;

        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange, interactLayer))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                isHoveringInteractable = true;
            }
            else
            {
                isHoveringInteractable = false;
            }
        }
        else
        {
            isHoveringInteractable = false;
        }

        // Update color if state changed
        if (wasHovering != isHoveringInteractable)
        {
            reticleImage.color = isHoveringInteractable ? hoverColor : normalColor;
        }
    }
}