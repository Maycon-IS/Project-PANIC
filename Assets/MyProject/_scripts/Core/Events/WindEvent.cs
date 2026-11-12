using System.Collections.Generic;
using UnityEngine;

public class WindEvent : MonoBehaviour, IArenaEvent
{
    private enum WindDirection { Left, Right, Down }
    private enum Phase { Calm, Gust }

    [Header("Nome do Evento")]
    [SerializeField] private string eventName = "Ventania";

    [Header("Tempos (valores do GDD)")]
    [SerializeField] private float gustDuration = 3f;
    [SerializeField] private float calmDuration = 2f;

    [Header("Intensidade (ajustar nos testes)")]
    [SerializeField] private float horizontalStrength = 2f;
    [SerializeField, Range(0.1f, 1f)] private float verticalJumpMultiplier = 0.75f;
    [SerializeField, Range(0f, 1f)] private float carrierInfluence = 0.4f;

    [Header("Indicadores (aparecem antes da rajada)")]
    [SerializeField] private GameObject leftIndicator;
    [SerializeField] private GameObject rightIndicator;
    [SerializeField] private GameObject downIndicator;

    private readonly List<PlayerController> affected = new List<PlayerController>();
    private Phase phase;
    private WindDirection direction;
    private float timer;

    public string EventName => eventName;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        HideIndicators();
    }

    public void Activate()
    {
        IsActive = true;

        affected.Clear();
        foreach (BombCarrier carrier in BombManager.Instance.ActivePlayers)
        {
            if (carrier.TryGetComponent<PlayerController>(out var controller))
                affected.Add(controller);
        }

        StartCalm();
        Debug.Log($"[{eventName}] ATIVADO ({affected.Count} jogadores afetados)");
    }

    public void Deactivate()
    {
        IsActive = false;

        ClearEffects();
        HideIndicators();
        affected.Clear();

        Debug.Log($"[{eventName}] DESATIVADO");
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        timer -= deltaTime;

        if (phase == Phase.Calm)
        {
            if (timer <= 0f) StartGust();
            return;
        }

        if (timer <= 0f)
        {
            StartCalm();
            return;
        }

        ApplyGust();
    }

    private void StartCalm()
    {
        phase = Phase.Calm;
        timer = calmDuration;
        direction = (WindDirection)Random.Range(0, 3);

        ClearEffects();
        ShowIndicator(direction);

        Debug.Log($"[{eventName}] Próxima rajada: {direction} (em {calmDuration}s)");
    }

    private void StartGust()
    {
        phase = Phase.Gust;
        timer = gustDuration;

        HideIndicators();
        Debug.Log($"[{eventName}] RAJADA: {direction}");
    }

    private void ApplyGust()
    {
        BombCarrier holder = BombManager.Instance.CurrentCarrier;

        foreach (PlayerController controller in affected)
        {
            if (!controller.gameObject.activeInHierarchy) continue;

            bool isHolder = holder != null && controller.gameObject == holder.gameObject;
            float influence = isHolder ? carrierInfluence : 1f;

            switch (direction)
            {
                case WindDirection.Left:
                    controller.ExternalVelocityX = -horizontalStrength * influence;
                    controller.JumpForceMultiplier = 1f;
                    break;

                case WindDirection.Right:
                    controller.ExternalVelocityX = horizontalStrength * influence;
                    controller.JumpForceMultiplier = 1f;
                    break;

                case WindDirection.Down:
                    controller.ExternalVelocityX = 0f;
                    controller.JumpForceMultiplier = 1f - (1f - verticalJumpMultiplier) * influence;
                    break;
            }
        }
    }

    private void ClearEffects()
    {
        foreach (PlayerController controller in affected)
        {
            controller.ExternalVelocityX = 0f;
            controller.JumpForceMultiplier = 1f;
        }
    }

    private void ShowIndicator(WindDirection dir)
    {
        HideIndicators();

        GameObject target = dir == WindDirection.Left ? leftIndicator
                          : dir == WindDirection.Right ? rightIndicator
                          : downIndicator;

        if (target != null) target.SetActive(true);
    }

    private void HideIndicators()
    {
        if (leftIndicator != null) leftIndicator.SetActive(false);
        if (rightIndicator != null) rightIndicator.SetActive(false);
        if (downIndicator != null) downIndicator.SetActive(false);
    }
}