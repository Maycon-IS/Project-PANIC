using System.Collections.Generic;
using UnityEngine;

public class ArenaEventManager : MonoBehaviour
{
    public static ArenaEventManager Instance { get; private set; }

    [SerializeField] private List<MonoBehaviour> eventBehaviours;

    private List<IArenaEvent> _events = new List<IArenaEvent>();
    private IArenaEvent _currentEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var behaviour in eventBehaviours)
        {
            if (behaviour is IArenaEvent arenaEvent)
                _events.Add(arenaEvent);
            else
                Debug.LogWarning($"{behaviour.name} nao implementa IArenaEvent.");
        }
    }

    private void OnEnable()
    {
        PlayerElimination.OnPlayerEliminated += HandlePlayerEliminated;
    }

    private void OnDisable()
    {
        PlayerElimination.OnPlayerEliminated -= HandlePlayerEliminated;
    }

    private void Update()
    {
        _currentEvent?.Tick(Time.deltaTime);
    }

    private void HandlePlayerEliminated(GameObject eliminatedPlayer)
    {
        Debug.Log($"[ArenaEventManager] {eliminatedPlayer.name} eliminado. " +
                   $"Eventos disponíveis: {_events.Count}");

        _currentEvent?.Deactivate();

        if (_events.Count > 0)
        {
            int index = Random.Range(0, _events.Count);
            _currentEvent = _events[index];

            Debug.Log($"[ArenaEventManager] Evento sorteado: {_currentEvent.EventName}");

            _currentEvent.Activate();
        }
    }
}