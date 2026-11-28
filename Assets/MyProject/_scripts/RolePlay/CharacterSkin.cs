using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSkin",
    menuName = "Project PANIC/Character Skin"
)]
public class CharacterSkin : ScriptableObject
{
    [Header("Identificacao")]
    public string skinID;
    public string skinName;

    [Header("Visual")]
    public Sprite characterSprite;
    public Color characterColor = Color.white;

    [Header("Animacoes")]
    public RuntimeAnimatorController animatorController;

    [Header("Ajustes Corporais")]
    public PlayerBodyAdjuster.BodySettings[] bodySettings;
}