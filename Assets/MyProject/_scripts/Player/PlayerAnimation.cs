using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent (typeof(PlayerElimination))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("Configuracoes")]
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float verticalThreshold = 0.1f;

    [SerializeField] private Animator animator; 
    
    private PlayerController controller;
    private PlayerElimination elimination;

    private string currentAnimation;

    private bool wasGrounded;
    private bool initialized;
    private bool isLanding;
    private bool isDead;

    // Nomes dos estados do Animator
    private const string IDLE = "Idle";
    private const string RUN = "Run";
    private const string JUMP = "Jump";
    private const string FALL = "Fall";
    private const string LAND = "Land";
    private const string DASH = "Dash";
    private const string DEATH = "Death";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        controller = GetComponent<PlayerController>();
        elimination = GetComponent<PlayerElimination>();

        if (animator == null ||
            animator.runtimeAnimatorController == null)
        {
            Debug.LogError(
                "PlayerAnimation: Animator sem Controller configurado.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        currentAnimation = null;
        initialized = false;
        isLanding = false;
        isDead = false;

        PlayAnimation(IDLE);
    }

    private void LateUpdate()
    {
        if (animator == null ||
            animator.runtimeAnimatorController == null ||
            !animator.isActiveAndEnabled ||
            !animator.isInitialized)
        {
            return;
        }

        if (isDead)
            return;

        if (elimination != null && elimination.IsDead)
        {
            PlayDeath();
            return;
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        float horizontalVelocity = controller.HorizontalVelocity;
        float verticalVelocity = controller.VerticalVelocity;

        // Evita permanecer no Idle no inicio do pulo
        bool grounded = controller.IsGrounded &&
                        verticalVelocity <= verticalThreshold;

        if (!initialized)
        {
            wasGrounded = grounded;
            initialized = true;
        }

        bool justLanded = grounded && !wasGrounded;

        wasGrounded = grounded;

        // PRIORIDADE 1: Dash
        if (controller.IsDashing)
        {
            isLanding = false;
            PlayAnimation(DASH);
            return;
        }

        // PRIORIDADE 2: Movimento aereo
        if (!grounded)
        {
            isLanding = false;

            if (verticalVelocity > verticalThreshold)
                PlayAnimation(JUMP);
            else
                PlayAnimation(FALL);

            return;
        }

        // PRIORIDADE 3: Aterrissagem
        if (justLanded)
        {
            isLanding = true;

            PlayAnimation(LAND);

            return;
        }

        if (isLanding)
        {
            if (!HasAnimationFinished(LAND))
                return;

            isLanding = false;
        }

        // PRIORIDADE 4: Corrida
        if (Mathf.Abs(horizontalVelocity) > movementThreshold)
        {
            PlayAnimation(RUN);
            return;
        }

        // PRIORIDADE 5: Parado
        PlayAnimation(IDLE);
    }

    private void PlayAnimation(string animationName)
    {
        // Impede que a animacao reinicie todo frame
        if (currentAnimation == animationName)
            return;

        animator.Play(animationName, 0, 0f);

        currentAnimation = animationName;
    }

    private bool HasAnimationFinished(string animationName)
    {
        AnimatorStateInfo state =
            animator.GetCurrentAnimatorStateInfo(0);

        return state.IsName(animationName) &&
               state.normalizedTime >= 1f;
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;
        isLanding = false;

        PlayAnimation(DEATH);
    }

    public bool IsDeathAnimationFinished()
    {
        return isDead && HasAnimationFinished(DEATH);
    }
}