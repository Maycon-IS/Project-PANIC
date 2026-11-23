
using System.Collections.Generic;
using UnityEngine;

public class ArenaEventManager : MonoBehaviour
{
    public static ArenaEventManager Instance { get; private set; }

    [Header("Eventos Disponíveis")]
    [SerializeField] private List<MonoBehaviour> eventBehaviours;

    private readonly List<IArenaEvent> events =
        new List<IArenaEvent>();

    private IArenaEvent currentEvent;
    private IArenaEvent preparedEvent;

    private int activatedEventCount;

    public int ActivatedEventCount => activatedEventCount;

    public string CurrentEventName =>
        currentEvent != null
            ? currentEvent.EventName
            : "";

    public string NextEventName =>
        preparedEvent != null
            ? preparedEvent.EventName
            : "";

    public bool HasPreparedEvent => preparedEvent != null;
    public bool HasActiveEvent => currentEvent != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (MonoBehaviour behaviour in eventBehaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour is IArenaEvent arenaEvent)
                events.Add(arenaEvent);
            else
                Debug.LogWarning(
                    $"{behaviour.name} não implementa IArenaEvent."
                );
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        currentEvent?.Tick(Time.deltaTime);
    }

    public void PrepareNextEvent()
    {
        DeactivateCurrentEvent();

        preparedEvent = null;

        if (events.Count == 0)
            return;

        int index = Random.Range(0, events.Count);
        preparedEvent = events[index];

        Debug.Log(
            $"Próximo evento: {preparedEvent.EventName}"
        );
    }

    public void ActivatePreparedEvent()
    {
        if (preparedEvent == null)
            return;

        DeactivateCurrentEvent();

        currentEvent = preparedEvent;
        preparedEvent = null;

        currentEvent.Activate();
        activatedEventCount++;

        Debug.Log(
            $"Evento ativado: {currentEvent.EventName}"
        );
    }

    public void DeactivateCurrentEvent()
    {
        if (currentEvent == null)
            return;

        currentEvent.Deactivate();
        currentEvent = null;
    }

    public void ResetEvents(bool resetStatistics = true)
    {
        DeactivateCurrentEvent();

        preparedEvent = null;

        if (resetStatistics)
            activatedEventCount = 0;
    }
}
