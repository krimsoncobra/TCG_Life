using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// UI for fry salting minigame
/// Shows shake progress and alternating key prompts
/// </summary>
public class FrySaltingUI : MonoBehaviour
{
    public static FrySaltingUI Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject uiPanel;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI progressText;
    public Image progressBar;

    [Header("Key Prompts")]
    public GameObject aKeyPrompt; // Visual for A key
    public GameObject dKeyPrompt; // Visual for D key

    [Header("Colors")]
    public Color activeKeyColor = Color.yellow;
    public Color inactiveKeyColor = Color.gray;

    private bool expectingA = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  SHOW/HIDE UI
    // ═══════════════════════════════════════════════════════════════

    public void ShowUI(int shakesRequired)
    {
        if (uiPanel == null) return;

        uiPanel.SetActive(true);

        // Set instruction
        if (instructionText != null)
        {
            instructionText.text = "Press A & D Alternating!";
        }

        // Set progress
        if (progressText != null)
        {
            progressText.text = $"0/{shakesRequired}";
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
        }

        // Show A key as first prompt
        expectingA = true;
        UpdateKeyPrompts();

        // Animate in
        if (uiPanel.transform is RectTransform rect)
        {
            rect.localScale = Vector3.zero;
            rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }
    }

    public void HideUI(bool success)
    {
        if (uiPanel == null) return;

        // Show result briefly
        if (instructionText != null)
        {
            instructionText.text = success ? "✅ Perfectly Salted!" : "❌ Not Salted!";
            instructionText.color = success ? Color.green : Color.red;
        }

        // Animate out after delay
        StartCoroutine(HideAfterDelay(1f));
    }

    System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (uiPanel != null)
        {
            if (uiPanel.transform is RectTransform rect)
            {
                rect.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    uiPanel.SetActive(false);
                });
            }
            else
            {
                uiPanel.SetActive(false);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  UPDATE PROGRESS
    // ═══════════════════════════════════════════════════════════════

    public void UpdateProgress(int currentShakes, int shakesRequired)
    {
        // Update text
        if (progressText != null)
        {
            progressText.text = $"{currentShakes}/{shakesRequired}";
        }

        // Update bar
        if (progressBar != null)
        {
            float progress = (float)currentShakes / shakesRequired;
            progressBar.DOFillAmount(progress, 0.2f);
        }

        // Switch expected key
        expectingA = !expectingA;
        UpdateKeyPrompts();
    }

    void UpdateKeyPrompts()
    {
        // Highlight which key to press next
        if (aKeyPrompt != null)
        {
            Image aImage = aKeyPrompt.GetComponent<Image>();
            if (aImage != null)
            {
                aImage.color = expectingA ? activeKeyColor : inactiveKeyColor;
            }

            // Scale pulse for active key
            if (expectingA)
            {
                aKeyPrompt.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            }
        }

        if (dKeyPrompt != null)
        {
            Image dImage = dKeyPrompt.GetComponent<Image>();
            if (dImage != null)
            {
                dImage.color = expectingA ? inactiveKeyColor : activeKeyColor;
            }

            // Scale pulse for active key
            if (!expectingA)
            {
                dKeyPrompt.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
            }
        }
    }
}