
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSkin",
    menuName = "Project PANIC/Character Skin"
)]
public class CharacterSkin : ScriptableObject
{
    public string skinID;
    public string skinName;
    public Sprite characterSprite;
    public Color characterColor = Color.white;
}
