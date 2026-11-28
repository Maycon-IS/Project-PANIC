using UnityEngine;

public class SkinManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerBodyAdjuster bodyAdjuster;

    [Header("Skin Inicial")]
    [SerializeField] private CharacterSkin defaultSkin;

    private CharacterSkin currentSkin;

    public CharacterSkin CurrentSkin => currentSkin;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();

        if (bodyAdjuster == null)
            bodyAdjuster =
                GetComponent<PlayerBodyAdjuster>();
    }

    private void Start()
    {
        if (currentSkin == null && defaultSkin != null)
            ApplySkin(defaultSkin);
    }

    public void ApplySkin(CharacterSkin skin)
    {
        if (skin == null)
            return;

        if (animator == null ||
            skin.animatorController == null)
        {
            Debug.LogError(
                "SkinManager: Animator Controller invalido.",
                this
            );

            return;
        }

        animator.runtimeAnimatorController =
            skin.animatorController;

        if (spriteRenderer != null)
            spriteRenderer.color = skin.characterColor;

        if (bodyAdjuster != null)
            bodyAdjuster.ApplySkinSettings(
                skin.bodySettings
            );

        currentSkin = skin;
    }

    public void ResetSkin()
    {
        ApplySkin(defaultSkin);
    }
}