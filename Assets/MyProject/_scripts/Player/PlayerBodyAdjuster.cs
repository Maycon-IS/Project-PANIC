using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerBodyAdjuster : MonoBehaviour
{
    [System.Serializable]
    public class BodySettings
    {
        public string animationName;

        [Min(0.01f)]
        public float visualScale = 1f;

        public float visualOffsetY = 0f;

        public Vector2 colliderMultiplier = Vector2.one;
    }

    [Header("Referencias")]
    [SerializeField] private Transform visual;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CapsuleCollider2D bodyCollider;

    [Header("Collider Automatico")]
    [SerializeField] private bool autoFitCollider = true;

    [Header("Bomb Detector")]
    [SerializeField] private CapsuleCollider2D bombDetector;

    [Min(0f)]
    [SerializeField] private float bombDetectorMargin = 0.08f;

    [Range(0.1f, 1f)]
    [SerializeField] private float widthFit = 0.65f;

    [Range(0.1f, 1f)]
    [SerializeField] private float heightFit = 0.9f;

    [SerializeField] private bool keepColliderBottom = true;

    [Header("Ajustes por Animacao")]
    [SerializeField] private BodySettings[] settings;

    private Vector3 baseScale;
    private Vector3 basePosition;

    private Vector2 baseColliderSize;
    private Vector2 baseColliderOffset;

    private Vector2 targetColliderSize;
    private Vector2 targetColliderOffset;

    private BodySettings currentSettings;
    private bool initialized;

    private void Awake()
    {
        if (bodyCollider == null)
            bodyCollider = GetComponent<CapsuleCollider2D>();

        if (visual != null)
        {
            if (animator == null)
                animator = visual.GetComponent<Animator>();

            if (spriteRenderer == null)
                spriteRenderer =
                    visual.GetComponent<SpriteRenderer>();
        }

        if (visual == null ||
            animator == null ||
            spriteRenderer == null ||
            visual == transform ||
            animator.runtimeAnimatorController == null)
        {
            Debug.LogError(
                "PlayerBodyAdjuster: referencias invalidas.",
                this
            );

            enabled = false;
            return;
        }

        baseScale = visual.localScale;
        basePosition = visual.localPosition;

        baseColliderSize = bodyCollider.size;
        baseColliderOffset = bodyCollider.offset;

        targetColliderSize = baseColliderSize;
        targetColliderOffset = baseColliderOffset;

        initialized = true;
    }

    private void LateUpdate()
    {
        if (!animator.isInitialized ||
            !animator.isActiveAndEnabled)
            return;

        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(0);

        currentSettings = null;

        if (settings != null)
        {
            foreach (BodySettings setting in settings)
            {
                if (setting != null &&
                    state.IsName(setting.animationName))
                {
                    currentSettings = setting;
                    break;
                }
            }
        }

        ApplyVisual();
        CalculateCollider();
    }

    private void ApplyVisual()
    {
        float scale = currentSettings != null
            ? currentSettings.visualScale
            : 1f;

        float offsetY = currentSettings != null
            ? currentSettings.visualOffsetY
            : 0f;

        visual.localScale = new Vector3(
            baseScale.x * scale,
            baseScale.y * scale,
            baseScale.z
        );

        visual.localPosition =
            basePosition + Vector3.up * offsetY;
    }

    private void UpdateBombDetector()
{
    if (bombDetector == null || bombDetector == bodyCollider)
        return;

    // Converte o centro para o espaco local do BombDetector.
    Vector3 worldCenter =
        bodyCollider.transform.TransformPoint(bodyCollider.offset);

    Vector2 localCenter =
        bombDetector.transform.InverseTransformPoint(worldCenter);

    // Adiciona uma margem em todos os lados.
    Vector2 size = bodyCollider.size +
        Vector2.one * (bombDetectorMargin * 2f);

    bombDetector.direction = bodyCollider.direction;
    bombDetector.size = size;
    bombDetector.offset = localCenter;
}

    private void CalculateCollider()
    {
        Vector2 size = baseColliderSize;
        Vector2 offset = baseColliderOffset;

        if (autoFitCollider && spriteRenderer.sprite != null)
        {
            Bounds bounds = spriteRenderer.sprite.bounds;

            Vector3 center = bounds.center;

            if (spriteRenderer.flipX)
                center.x = -center.x;

            if (spriteRenderer.flipY)
                center.y = -center.y;

            Vector3 localCenter =
                transform.InverseTransformPoint(
                    spriteRenderer.transform.TransformPoint(center)
                );

            Vector3 widthVector =
                transform.InverseTransformVector(
                    spriteRenderer.transform.TransformVector(
                        Vector3.right * bounds.size.x
                    )
                );

            Vector3 heightVector =
                transform.InverseTransformVector(
                    spriteRenderer.transform.TransformVector(
                        Vector3.up * bounds.size.y
                    )
                );

            size = new Vector2(
                (Mathf.Abs(widthVector.x) +
                 Mathf.Abs(heightVector.x)) * widthFit,

                (Mathf.Abs(widthVector.y) +
                 Mathf.Abs(heightVector.y)) * heightFit
            );

            offset = localCenter;
        }

        if (currentSettings != null)
        {
            size = Vector2.Scale(
                size,
                currentSettings.colliderMultiplier
            );
        }

        size.x = Mathf.Max(0.01f, size.x);
        size.y = Mathf.Max(0.01f, size.y);

        if (keepColliderBottom)
        {
            float bottom =
                baseColliderOffset.y -
                baseColliderSize.y * 0.5f;

            offset.y = bottom + size.y * 0.5f;
        }

        targetColliderSize = size;
        targetColliderOffset = offset;
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        if (bodyCollider.size != targetColliderSize)
            bodyCollider.size = targetColliderSize;

        if (bodyCollider.offset != targetColliderOffset)
            bodyCollider.offset = targetColliderOffset; 

        UpdateBombDetector();

    }

    private void OnDisable()
    {
        if (!initialized)
            return;

        visual.localScale = baseScale;
        visual.localPosition = basePosition;

        bodyCollider.size = baseColliderSize;
        bodyCollider.offset = baseColliderOffset;

        targetColliderSize = baseColliderSize;
        targetColliderOffset = baseColliderOffset;

        currentSettings = null;

        UpdateBombDetector();
    }
}