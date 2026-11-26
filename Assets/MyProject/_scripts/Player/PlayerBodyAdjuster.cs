using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerBodyAdjuster : MonoBehaviour
{
    [System.Serializable]
    public class AnimationBody
    {
        public string animationName;

        public Vector2 visualScale = Vector2.one;
        public Vector2 visualOffset = Vector2.zero;

        public Vector2 colliderMultiplier = Vector2.one;
    }

    [SerializeField] private PlayerSpriteNormalizer normalizer;

    [Header("Referencias")]
    [SerializeField] private Transform visual;
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D bodyCollider;

    [Header("Suavizacao Visual")]
    [SerializeField] private float smoothSpeed = 20f;

    [Header("Ajustes por Animacao")]
    [SerializeField]
    private AnimationBody[] animations =
    {
        new AnimationBody { animationName = "Idle" },
        new AnimationBody { animationName = "Run" },
        new AnimationBody { animationName = "Jump" },
        new AnimationBody { animationName = "Fall" },
        new AnimationBody { animationName = "Land" },
        new AnimationBody { animationName = "Dash" },
        new AnimationBody { animationName = "Death" }
    };

    private Vector3 baseScale;
    private Vector3 basePosition;

    private Vector2 baseColliderSize;
    private Vector2 baseColliderOffset;

    private AnimationBody currentBody;

    private void Awake()
    {
        if (normalizer == null)
            normalizer = GetComponent<PlayerSpriteNormalizer>();

        if (bodyCollider == null)
            bodyCollider = GetComponent<CapsuleCollider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (visual == null && animator != null)
            visual = animator.transform;

        if (visual == null || visual == transform ||
            animator == null)
        {
            Debug.LogError(
                "Configure o objeto filho Visual.", this);

            enabled = false;
            return;
        }

        baseScale = visual.localScale;
        basePosition = visual.localPosition;

        baseColliderSize = bodyCollider.size;
        baseColliderOffset = bodyCollider.offset;
    }

    private void LateUpdate()
    {
        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(0);

        currentBody = null;

        foreach (AnimationBody body in animations)
        {
            if (state.IsName(body.animationName))
            {
                currentBody = body;
                break;
            }
        }

        Vector2 scale = currentBody != null
                    ? currentBody.visualScale
                    : Vector2.one;

        if (normalizer != null)
        {
            string animationName = currentBody != null
                ? currentBody.animationName
                : "";

            float automaticScale =
                normalizer.GetMultiplier(animationName);

            scale *= automaticScale;
        }

        Vector2 offset = currentBody != null
            ? currentBody.visualOffset
            : Vector2.zero;

        Vector3 targetScale = new Vector3(
            baseScale.x * scale.x,
            baseScale.y * scale.y,
            baseScale.z
        );

        Vector3 targetPosition = basePosition +
            new Vector3(offset.x, offset.y, 0f);

        float t = smoothSpeed > 0f
            ? 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime)
            : 1f;

        visual.localScale = Vector3.Lerp(
            visual.localScale, targetScale, t);

        visual.localPosition = Vector3.Lerp(
            visual.localPosition, targetPosition, t);
    }

    private void FixedUpdate()
    {
        Vector2 multiplier = currentBody != null
            ? currentBody.colliderMultiplier
            : Vector2.one;

        Vector2 targetSize = new Vector2(
            baseColliderSize.x * Mathf.Max(0.1f, multiplier.x),
            baseColliderSize.y * Mathf.Max(0.1f, multiplier.y)
        );

        // Mantem a base do collider na mesma altura.
        Vector2 targetOffset = baseColliderOffset;

        targetOffset.y +=
            (targetSize.y - baseColliderSize.y) / 2f;

        bodyCollider.size = targetSize;
        bodyCollider.offset = targetOffset;
    }

    private void OnDisable()
    {
        if (visual != null && visual != transform)
        {
            visual.localScale = baseScale;
            visual.localPosition = basePosition;
        }

        if (bodyCollider != null && baseColliderSize.x > 0f)
        {
            bodyCollider.size = baseColliderSize;
            bodyCollider.offset = baseColliderOffset;
        }

        currentBody = null;
    }
}