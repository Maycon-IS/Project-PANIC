using System.Collections.Generic;
using UnityEngine;

public class ArenaEventManager : MonoBehaviour
{
    public static ArenaEventManager Instance { get; private set; }

    [SerializeField] private List<MonoBehaviour> eventBehaviours;

    private List<IArenaEvent> _events = new List<IArenaEvent>();
    private IArenaEvent _currentEvent;
    private bool isFirstRound = true;

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
        BombManager.OnRoundEnded += HandleRoundEnded;
        BombManager.OnRoundStarted += HandleRoundStarted;
    }

    private void OnDisable()
    {
        BombManager.OnRoundEnded -= HandleRoundEnded;
        BombManager.OnRoundStarted -= HandleRoundStarted;
    }

    private void Update()
    {
        _currentEvent?.Tick(Time.deltaTime);
    }

    private void HandleRoundEnded()
    {
        Debug.Log("[ArenaEventManager] Rodada encerrada — evento desativado.");
        _currentEvent?.Deactivate();
        _currentEvent = null;
    }

    private void HandleRoundStarted()
    {
        if (isFirstRound)
        {
            isFirstRound = false;
            Debug.Log("[ArenaEventManager] 1ª rodada — sem evento de arena.");
            return;
        }

        if (_events.Count == 0) return;

        int index = Random.Range(0, _events.Count);
        _currentEvent = _events[index];

        Debug.Log($"[ArenaEventManager] Nova rodada — evento sorteado: {_currentEvent.EventName}");
        _currentEvent.Activate();
    }
}