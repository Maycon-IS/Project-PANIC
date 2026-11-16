
using System.Collections.Generic;
using UnityEngine;

public class DustStormEvent : MonoBehaviour, IArenaEvent
{
    private enum StormPhase { Intense, Mild }

    [Header("Nome do Evento")]
    [SerializeField] private string eventName = "Chuva de Areia";

    [Header("Duracao das Fases")]
    [SerializeField, Min(0.1f)] private float intenseDuration = 4f;
    [SerializeField, Min(0.1f)] private float mildDuration = 2f;

    [Header("Intensidade Visual")]
    [SerializeField, Range(0f, 1f)] private float intenseOpacity = 0.65f;
    [SerializeField, Range(0f, 1f)] private float mildOpacity = 0.2f;
    [SerializeField, Min(0f)] private float fadeSpeed = 2f;

    [Header("Visibilidade das IAs")]
    [SerializeField, Range(0.1f, 1f)] private float intenseVision = 0.5f;
    [SerializeField, Range(0.1f, 1f)] private float mildVision = 0.85f;

    [Header("Interface")]
    [SerializeField] private CanvasGroup dustCanvasGroup;
    [SerializeField] private GameObject dustIndicator;

    private readonly List<AIInputProvider> affectedAI =
        new List<AIInputProvider>();

    private StormPhase currentPhase;
    private float timer;
    private float targetOpacity;

    public string EventName => eventName;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        if (dustCanvasGroup != null)
        {
            dustCanvasGroup.alpha = 0f;
            dustCanvasGroup.interactable = false;
            dustCanvasGroup.blocksRaycasts = false;
        }

        if (dustIndicator != null)
            dustIndicator.SetActive(false);
    }

    public void Activate()
    {
        IsActive = true;

        affectedAI.Clear();

        foreach (BombCarrier carrier in BombManager.Instance.ActivePlayers)
        {
            if (carrier.TryGetComponent<AIInputProvider>(out var ai))
                affectedAI.Add(ai);
        }

        if (dustIndicator != null)
            dustIndicator.SetActive(true);

        StartIntensePhase();

        Debug.Log($"[{eventName}] ATIVADO");
    }

    public void Deactivate()
    {
        IsActive = false;

        foreach (AIInputProvider ai in affectedAI)
        {
            if (ai != null)
                ai.VisionMultiplier = 1f;
        }

        affectedAI.Clear();

        if (dustCanvasGroup != null)
            dustCanvasGroup.alpha = 0f;

        if (dustIndicator != null)
            dustIndicator.SetActive(false);

        Debug.Log($"[{eventName}] DESATIVADO");
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        timer -= deltaTime;

        if (timer <= 0f)
        {
            if (currentPhase == StormPhase.Intense)
                StartMildPhase();
            else
                StartIntensePhase();
        }

        UpdateDustVisual(deltaTime);
        UpdateAIVision();
    }

    private void StartIntensePhase()
    {
        currentPhase = StormPhase.Intense;
        timer = intenseDuration;
        targetOpacity = intenseOpacity;

        UpdateAIVision();

        Debug.Log($"[{eventName}] Poeira intensa");
    }

    private void StartMildPhase()
    {
        currentPhase = StormPhase.Mild;
        timer = mildDuration;
        targetOpacity = mildOpacity;

        UpdateAIVision();

        Debug.Log($"[{eventName}] Poeira leve");
    }

    private void UpdateDustVisual(float deltaTime)
    {
        if (dustCanvasGroup == null) return;

        dustCanvasGroup.alpha = Mathf.MoveTowards(
            dustCanvasGroup.alpha,
            targetOpacity,
            fadeSpeed * deltaTime
        );
    }

    private void UpdateAIVision()
    {
        BombCarrier holder = BombManager.Instance.CurrentCarrier;

        float vision = currentPhase == StormPhase.Intense ? intenseVision : mildVision;

        foreach (AIInputProvider ai in affectedAI)
        {
            if (ai == null || !ai.gameObject.activeInHierarchy)
                continue;

            bool isHolder = holder != null && ai.gameObject == holder.gameObject;

            ai.VisionMultiplier = isHolder ? 1f : vision;
        }
    }
}
