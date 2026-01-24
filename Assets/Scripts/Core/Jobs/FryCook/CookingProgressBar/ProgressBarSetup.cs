using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to automatically configure a progress bar
/// Attach this to your ProgressBarEntry and it will set up everything
/// </summary>
[ExecuteInEditMode]
public class ProgressBarSetup : MonoBehaviour
{
    public Image fillImage;

    void Start()
    {
        SetupFillImage();
    }

    void OnValidate()
    {
        SetupFillImage();
    }

    void SetupFillImage()
    {
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;

            Debug.Log("✅ Fill image configured automatically!");
        }
    }
}