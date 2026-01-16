using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Panels - CanvasGroups")]
    public CanvasGroup pauseCanvasGroup;    // Stats / Pause main screen
    public CanvasGroup jobsCanvasGroup;     // Jobs menu
    public CanvasGroup cardsCanvasGroup;    // Cards collection

    [Header("Cards")]
    public Transform cardsContent;          // ScrollView Content
    public GameObject cardSlotPrefab;
    public Button backToStatsButton;        // In cards menu

    // Dummy card data (proto - will be replaced later)
    [System.Serializable]
    public class DummyCard
    {
        public string name;
        public int index;
        public bool owned;
    }
    public List<DummyCard> allCards = new List<DummyCard>();

    private CanvasGroup currentOpenPanel;   // Helps track what's currently visible

    void Start()
    {
        PopulateDummyCards();

        // Wire back button in cards menu
        if (backToStatsButton != null)
            backToStatsButton.onClick.AddListener(() => ShowPanel(pauseCanvasGroup));

        // Optional: start with pause hidden
        HideAllPanels();
    }

    void PopulateDummyCards()
    {
        allCards.Clear(); // Clear existing cards to prevent duplicates

        for (int i = 1; i <= 50; i++)
        {
            allCards.Add(new DummyCard
            {
                name = $"Mystic {GetRarity(i)} #{i}",
                index = i,
                owned = (i <= 10 || Random.value > 0.7f)
            });
        }
    }

    string GetRarity(int idx) => idx % 5 == 0 ? "Rare" : "Common";

    // ────────────────────────────────────────────────────────────────
    //           MAIN MENU SWITCHING METHODS
    // ────────────────────────────────────────────────────────────────

    public void ShowStatsPanel() => ShowPanel(pauseCanvasGroup);
    public void ShowJobsPanel() => ShowPanel(jobsCanvasGroup);
    public void ShowCardsPanel()
    {
        ShowPanel(cardsCanvasGroup);
        PopulateCards();   // Refresh cards when opening
    }

    private void ShowPanel(CanvasGroup target)
    {
        if (target == null) return;

        // Hide everything first
        HideAllPanels();

        // Show target
        target.alpha = 1f;
        target.interactable = true;
        target.blocksRaycasts = true;

        currentOpenPanel = target;
    }

    private void HideAllPanels()
    {
        HidePanel(pauseCanvasGroup);
        HidePanel(jobsCanvasGroup);
        HidePanel(cardsCanvasGroup);
    }

    private void HidePanel(CanvasGroup group)
    {
        if (group == null) return;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    // ────────────────────────────────────────────────────────────────
    //           CARDS POPULATION (unchanged)
    // ────────────────────────────────────────────────────────────────

    void PopulateCards()
    {
        foreach (Transform child in cardsContent)
            Destroy(child.gameObject);

        foreach (var card in allCards)
        {
            var slot = Instantiate(cardSlotPrefab, cardsContent);
            var img = slot.GetComponent<Image>();
            var nameText = slot.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var idxText = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            // Null check for safety
            if (img == null || nameText == null || idxText == null)
            {
                Debug.LogWarning("Missing UI component in card slot prefab!");
                continue;
            }

            img.color = card.owned ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            nameText.text = card.name;
            idxText.text = $"#{card.index:D3}"; // nicer formatting: #001, #002...
        }
    }

    // ────────────────────────────────────────────────────────────────
    //           QUIT & OTHER GLOBAL FUNCTIONS
    // ────────────────────────────────────────────────────────────────

    public void QuitGame()
    {
        Debug.Log("Quit requested!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Optional: Resume game (unpause everything)
    public void ResumeGame()
    {
        HideAllPanels();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        // Add any other unpause logic here
    }
}