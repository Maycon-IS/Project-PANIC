using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class HazardZone : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Collider2D _collider;
    private HazardDefinition _currentDefinition;

    private float _timer;
    private bool _triggered; // já ficou letal / já quebrou

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        ResetZone();
    }

    /// Configura este slot para se tornar um hazard específico.
    public void Configure(HazardDefinition definition)
    {
        _currentDefinition = definition;
        _timer = 0f;
        _triggered = false;

        _renderer.sprite = definition.visual;
        _renderer.color = definition.tint;
        _renderer.enabled = true;

        // Ponto-chave: o tipo de collider depende do comportamento escolhido.
        _collider.isTrigger = definition.behavior == HazardBehavior.InstantKillTrigger;
        _collider.enabled = definition.behavior == HazardBehavior.BreakableGround;
        // Plataforma já começa sólida (pisável); Lava/Espinho começam "frios" (ver Tick).

        gameObject.SetActive(true);
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
                _collider.enabled = false; // chão some, jogador cai
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