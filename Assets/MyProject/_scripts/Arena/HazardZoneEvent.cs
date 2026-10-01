using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardZoneEvent : MonoBehaviour, IArenaEvent
{
    [Header("Nome do Evento")]
    [SerializeField] private string eventName = "Lava";

    [Header("Zonea de Perigos (filhas, começam desativadas)")]
    [SerializeField] private GameObject[] hazardZones;

    public string EventName => eventName;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        SetZonesActive(false); // garante estado inicial limpo
    }

    public void Activate()
    {
        IsActive = true;
        SetZonesActive(true);
        Debug.Log($"[{eventName}] ATIVADO");
    }

    public void Deactivate()
    {
        IsActive = false;
        SetZonesActive(false);
        Debug.Log($"[{eventName}] DESATIVADO");
    }

    public void Tick(float deltaTime)
    {
        // Lava fixa: nada a fazer por frame por enquanto.
    }

    private void SetZonesActive(bool value)
    {
        foreach (GameObject zone in hazardZones)
            zone.SetActive(value);
    }
}