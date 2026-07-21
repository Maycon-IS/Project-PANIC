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
        normalColor = sr.color; // guarda a cor original (inclui o alpha original = 1)
    }

    private void Start()
    {
        // Start (não Awake) garante que BombManager.Instance já existe,
        // já que todo Awake da cena roda antes de qualquer Start
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

    private void UpdateVisual()
    {
        Color targetColor = normalColor;
        targetColor.a = HasBomb ? bombAlpha : normalColor.a;
        sr.color = targetColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasBomb) return;

        BombCarrier otherCarrier = other.GetComponent<BombCarrier>();
        if (otherCarrier == null || otherCarrier == this) return;

        BombManager.Instance.TransferBomb(this, otherCarrier);
    }
}