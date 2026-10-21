using UnityEngine;

public class BreakingPlatformsEvent : MonoBehaviour, IArenaEvent
{
    [Header("Identidade")]
    [SerializeField] private string eventName = "Plataformas Frageis";

    [Header("Plataformas que podem ser pisadas e quebrar")]
    [SerializeField] private BreakablePlatform[] candidates;

    [Header("Balanceamento")]
    [SerializeField] private float breakDelay = 1.5f; // tempo entre pisar e quebrar

    [Header("Feedback visual")]
    [SerializeField] private Color warningColor = Color.white;
    [SerializeField] private float blinkFrequency = 30f;

    public string EventName => eventName;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        foreach (var platform in candidates)
            platform.Initialize();
    }

    public void Activate()
    {
        IsActive = true;

        foreach (var platform in candidates)
            platform.Arm(breakDelay);

        //Debug.Log($"[{eventName}] ATIVADO ({candidates.Length} plataformas armadas)");
    }

    public void Deactivate()
    {
        IsActive = false;

        foreach (var platform in candidates)
            platform.Disarm();

        //Debug.Log($"[{eventName}] DESATIVADO");
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        foreach (var platform in candidates)
            platform.Tick(deltaTime, warningColor, blinkFrequency);
    }
}