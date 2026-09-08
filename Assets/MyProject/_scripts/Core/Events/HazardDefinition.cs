using UnityEngine;

[CreateAssetMenu(fileName = "NewHazard", menuName = "LastFuse/Hazard Definition")]
public class HazardDefinition : ScriptableObject
{
    [Header("Identidade")]
    public string hazardName;
    public Sprite visual;
    public Color tint = Color.white;

    [Header("Comportamento")]
    public HazardBehavior behavior;

    [Header("Timing")]
    [Tooltip("Tempo de aviso antes de ficar letal/quebrar.")]
    public float warningTime = 1.0f;
}

public enum HazardBehavior
{
    InstantKillTrigger,   // Lava, Espinhos: trigger que mata ao tocar
    BreakableGround       // Plataforma: collider sólido que desaparece
}