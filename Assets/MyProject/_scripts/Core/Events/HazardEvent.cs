using System.Collections.Generic;
using UnityEngine;

public class HazardEvent : MonoBehaviour, IArenaEvent
{
    [Header("Slots físicos na arena")]
    [SerializeField] private List<HazardZone> hazardZones;

    [Header("Tipos de hazard disponíveis (pool)")]
    [SerializeField] private List<HazardDefinition> hazardPool;

    public string EventName => "Hazard";
    public bool IsActive { get; private set; }

    public void Activate()
    {
        IsActive = true;

        var chosenDefinition = hazardPool[Random.Range(0, hazardPool.Count)];

        foreach (var zone in hazardZones)
        {
            var randomDefinition = hazardPool[Random.Range(0, hazardPool.Count)];
            zone.Configure(randomDefinition);
        }

        Debug.Log(chosenDefinition.hazardName);
    }

    public void Deactivate()
    {
        IsActive = false;
        foreach (var zone in hazardZones)
            zone.ResetZone();
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;
        foreach (var zone in hazardZones)
            zone.Tick(deltaTime);
    }
}