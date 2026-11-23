
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BombCarrier : MonoBehaviour
{
    [Header("Feedback Visual")]
    [SerializeField] private float bombAlpha = 0.4f;

    public bool HasBomb { get; private set; }

    private SpriteRenderer sr;
    private Color normalColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        normalColor = sr.color;
    }

    private void Start()
    {
        BombManager.Instance.RegisterPlayer(this);
    }

    private void OnDestroy()
    {
        if (BombManager.Instance != null)
            BombManager.Instance.UnregisterPlayer(this);
    }

    public void SetBombState(bool hasBomb)
    {
        HasBomb = hasBomb;
        UpdateVisual();
    }

    // NOVO: Aplica a skin escolhida no LobbyOff.
    public void SetAppearance(Sprite sprite, Color color)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        sr.sprite = sprite;
        normalColor = color;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Color targetColor = normalColor;

        targetColor.a = HasBomb
            ? bombAlpha
            : normalColor.a;

        sr.color = targetColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasBomb)
            return;

        BombCarrier otherCarrier =
            other.GetComponent<BombCarrier>();

        if (otherCarrier == null || otherCarrier == this)
            return;

        BombManager.Instance.TransferBomb(
            this,
            otherCarrier
        );
    }
}
