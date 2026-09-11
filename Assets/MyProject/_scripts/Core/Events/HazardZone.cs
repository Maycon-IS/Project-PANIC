using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class HazardZone : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Collider2D _collider;
    private HazardDefinition _currentDefinition;

    private float _timer;
    private bool _triggered; 

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        ResetZone();
    }

    /// Configura este slot para se tornar um hazard .
    public void Configure(HazardDefinition definition)
    {
        gameObject.SetActive(true); 
        EnsureReferences();

        _currentDefinition = definition;
        _timer = 0f;
        _triggered = false;

        _renderer.sprite = definition.visual;
        _renderer.color = definition.tint;
        _renderer.enabled = true;

        // Ponto-chave: o tipo de collider depende do comportamento escolhido.
        _collider.isTrigger = definition.behavior == HazardBehavior.InstantKillTrigger;
        _collider.enabled = definition.behavior == HazardBehavior.BreakableGround;

        gameObject.SetActive(true);

         Debug.Log($"[HazardZone] '{gameObject.name}' configurado como '{definition.hazardName}'. " +
               $"IsTrigger={_collider.isTrigger}, ColliderEnabled={_collider.enabled}");
    }

    private void EnsureReferences()
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        if (_collider == null) _collider = GetComponent<Collider2D>();
    }

    public void ResetZone()
    {
        gameObject.SetActive(false);
        _currentDefinition = null;
        _collider.enabled = false;
    }

    public void Tick(float deltaTime)
    {
        if (_currentDefinition == null || _triggered) return;

        _timer += deltaTime;
        if (_timer < _currentDefinition.warningTime) return;

        _triggered = true;

        switch (_currentDefinition.behavior)
        {
            case HazardBehavior.InstantKillTrigger:
                _collider.enabled = true; // agora fica letal
                break;

            case HazardBehavior.BreakableGround:
                _collider.enabled = false; // ch�o some, jogador cai
                _renderer.enabled = false;
                break;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_currentDefinition == null || !_triggered) return;
        if (_currentDefinition.behavior != HazardBehavior.InstantKillTrigger) return;

        if (other.TryGetComponent<PlayerElimination>(out var elimination))
        {
            elimination.Kill();
        }
    }
}