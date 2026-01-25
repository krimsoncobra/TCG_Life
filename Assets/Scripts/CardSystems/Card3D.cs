using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents a physical card object in the game world.
/// Can be picked up, examined, traded, etc.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Card3D : MonoBehaviour, IInteractable
{
    [Header("Card Data")]
    public CardData cardData;

    [Header("Visual Components")]
    public MeshRenderer cardRenderer;
    public GameObject holoEffectInstance;

    [Header("Animation Settings")]
    public float hoverHeight = 0.2f;
    public float hoverSpeed = 1f;
    public float rotationSpeed = 30f;

    [Header("Stats Text (Child Objects)")]
    public TextMeshPro hpLabel;
    public TextMeshPro powerLabel;
    public TextMeshPro speedLabel;
    public TextMeshPro intLabel;
    public TextMeshPro nameLabel;

    private Vector3 startPosition;
    private bool isHovering = true;

    void Awake()
    {
        if (cardRenderer == null)
            cardRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        if (cardData != null)
            InitializeCard();

        startPosition = transform.position;

        if (isHovering)
            StartHoverAnimation();
    }

    // ═══════════════════════════════════════════════════════════════
    //  CARD INITIALIZATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Setup the 3D card based on its data
    /// </summary>
    public void InitializeCard()
    {
        if (cardData == null) return;

        // Art (your existing)
        if (cardData.cardArtwork != null && cardRenderer != null)
            cardRenderer.material.mainTexture = cardData.cardArtwork.texture;

        // Holo (your existing)
        if (cardData.cardType >= CardType.HoloRare && cardData.holoEffectPrefab != null)
        {
            if (holoEffectInstance == null)
                holoEffectInstance = Instantiate(cardData.holoEffectPrefab, transform);
        }

        // NEW: Stats Display
        nameLabel.text = cardData.cardName;
        hpLabel.text = $"HP: {cardData.health}";
        powerLabel.text = $"PWR: {cardData.powerStat}";
        speedLabel.text = $"SPD: {cardData.speedStat}";
        intLabel.text = $"INT: {cardData.intelligenceStat}";

        // Name card for debug
        gameObject.name = $"Card3D - {cardData.GetFullCardName()}";
    }

    // ═══════════════════════════════════════════════════════════════
    //  IINTERACTABLE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════════

    public string GetPromptText()
    {
        if (cardData == null)
            return "E to Pick Up Card";

        return $"E to Pick Up {cardData.cardName} Card";
    }

    public void Interact()
    {
        // Add to player's collection
        if (PlayerCollection.Instance != null)
        {
            PlayerCollection.Instance.AddCard(cardData);
            Debug.Log($"Picked up: {cardData.GetFullCardName()}");
        }

        // Play pickup animation and destroy
        PickupAnimation();
    }

    // ═══════════════════════════════════════════════════════════════
    //  ANIMATIONS
    // ═══════════════════════════════════════════════════════════════

    void StartHoverAnimation()
    {
        // Gentle up/down floating
        transform.DOMoveY(startPosition.y + hoverHeight, hoverSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Slow rotation
        transform.DORotate(new Vector3(0, 360, 0), 360f / rotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    void PickupAnimation()
    {
        // Stop hovering
        transform.DOKill();
        isHovering = false;

        // Animate card flying to camera
        transform.DOScale(0f, 0.5f).SetEase(Ease.InBack);
        transform.DOMoveY(transform.position.y + 2f, 0.5f).SetEase(Ease.OutQuad);

        // Destroy after animation
        Destroy(gameObject, 0.6f);
    }

    // ═══════════════════════════════════════════════════════════════
    //  UTILITY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a card GameObject at runtime
    /// </summary>
    public static GameObject SpawnCard(CardData data, Vector3 position, Quaternion rotation)
    {
        // Create basic card mesh (or use prefab)
        GameObject cardObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cardObj.transform.position = position;
        cardObj.transform.rotation = rotation;
        cardObj.transform.localScale = new Vector3(0.6f, 0.85f, 0.1f); // Trading card proportions

        // Add components
        Card3D card3D = cardObj.AddComponent<Card3D>();
        card3D.cardData = data;

        // Tag for interaction
        cardObj.tag = "Interactable";

        // Add collider
        BoxCollider collider = cardObj.GetComponent<BoxCollider>();
        collider.isTrigger = false;

        return cardObj;
    }

    void OnDestroy()
    {
        // Clean up DOTween animations
        transform.DOKill();
    }
}