using UnityEngine;

[System.Serializable]
public class BreakablePlatform
{
    public enum State { Intact, Warning, Broken }

    [SerializeField] private Collider2D col;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private PlatformStepDetector stepDetector;

    public State CurrentState { get; private set; }

    private bool isArmed;
    private float timer;
    private float breakDelay;
    private Color originalColor;

    public void Initialize()
    {
        originalColor = sprite.color;
        stepDetector.OnPlayerStepped += HandlePlayerStepped;
        Restore();
    }

    public void Arm(float delay)
    {
        breakDelay = delay;
        isArmed = true; // só reage a passos enquanto armada (evento ativo)
    }

    public void Disarm()
    {
        isArmed = false;
        Restore();
    }

    private void HandlePlayerStepped()
    {
        if (!isArmed) return;
        if (CurrentState != State.Intact) return; // já piscando ou já quebrada, ignora novos passos

        CurrentState = State.Warning;
        timer = breakDelay;
    }

    public void Tick(float deltaTime, Color warningColor, float blinkFrequency)
    {
        if (CurrentState != State.Warning) return;

        timer -= deltaTime;

        if (timer <= 0f)
        {
            Break();
            return;
        }

        bool blinkOn = Mathf.Sin(timer * blinkFrequency) > 0f;
        sprite.color = blinkOn ? warningColor : originalColor;
    }

    private void Break()
    {
        CurrentState = State.Broken;
        col.enabled = false;
        sprite.enabled = false;
    }

    public void Restore()
    {
        col.enabled = true;
        sprite.enabled = true;
        sprite.color = originalColor;
        CurrentState = State.Intact;
    }
}