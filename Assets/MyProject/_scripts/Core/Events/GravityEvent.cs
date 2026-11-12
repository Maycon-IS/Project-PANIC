
using System.Collections.Generic;
using UnityEngine;

public class GravityEvent : MonoBehaviour, IArenaEvent
{
    [Header("Nome do Evento")]
    [SerializeField] private string eventName = "Gravidade Aumentada";

    private enum GravityMode { Heavy, Light }
    private GravityMode currentMode;

    [Header("Intensidade da Gravidade")]
    [SerializeField, Min(1f)] private float heavyGravity = 1.5f;
    [SerializeField, Range(0.1f, 1f)] private float lightGravity = 0.5f;
    [SerializeField, Range(0f, 1f)] private float carrierInfluence = 0.3f;


    [Header("Indicador Visual")]
    [SerializeField] private GameObject gravityIndicator;

    private readonly List<PlayerController> affected = new List<PlayerController>();

    public string EventName => eventName;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        if (gravityIndicator != null)
            gravityIndicator.SetActive(false);
    }

    public void Activate()
    {
        IsActive = true;

        currentMode = (GravityMode)Random.Range(0, 2);

        affected.Clear();

        foreach (BombCarrier carrier in BombManager.Instance.ActivePlayers)
        {
            if (carrier.TryGetComponent<PlayerController>(out var controller))
                affected.Add(controller);
        }

        if (gravityIndicator != null)
            gravityIndicator.SetActive(true);

        ApplyGravity();

        Debug.Log($"[{eventName}] ATIVADO - Modo: {currentMode}");
    }

    public void Deactivate()
    {
        IsActive = false;

        ClearGravity();

        if (gravityIndicator != null)
            gravityIndicator.SetActive(false);

        affected.Clear();

        Debug.Log($"[{eventName}] DESATIVADO");
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        BombCarrier holder = BombManager.Instance.CurrentCarrier;

        float targetGravity = currentMode == GravityMode.Heavy
            ? heavyGravity
            : lightGravity;

        foreach (PlayerController controller in affected)
        {
            if (!controller.gameObject.activeInHierarchy) continue;

            bool isHolder = holder != null &&
                            controller.gameObject == holder.gameObject;

            float influence = isHolder ? carrierInfluence : 1f;

            controller.GravityMultiplier =
                1f + (targetGravity - 1f) * influence;
        }
    }

    private void ClearGravity()
    {
        foreach (PlayerController controller in affected)
        {
            if (controller != null)
                controller.GravityMultiplier = 1f;
        }
    }
}
