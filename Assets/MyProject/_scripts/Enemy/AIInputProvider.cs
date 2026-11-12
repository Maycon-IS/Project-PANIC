using UnityEngine;

[RequireComponent(typeof(BombCarrier))]
public class AIInputProvider : MonoBehaviour, IInputProvider
{
    private enum AIState { Wander, Chase, Flee }

    [Header("Decisao")]
    [SerializeField] private float decisionInterval = 0.2f;
    [SerializeField] private float awarenessRadius = 6f;

    [Header("Obstaculos e Chao")]
    [SerializeField] private float jumpCheckDistance = 0.6f;
    [SerializeField] private float groundAheadOffset = 0.5f;   // o qu�o � frente checa o ch�o
    [SerializeField] private float groundCheckDistance = 1.5f; // at� onde desce o raio, procurando ch�o
    [SerializeField] private LayerMask obstacleLayer;          // usada tanto pra parede quanto pra ch�o

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderInterval = 3f;

    [Header("Dash da IA")]
    [SerializeField] private float fleeDashDistance = 2f;   // perigo muito perto -> dash pra fugir
    [SerializeField] private float chaseDashDistance = 3f;  // alvo a m�dia dist�ncia -> dash pra alcan�ar

    [Header("Anti-Obstaculo (Stuck Detection)")]
    [SerializeField] private float stuckCheckInterval = 1f;
    [SerializeField] private float stuckMovementThreshold = 0.3f;
    [SerializeField] private float targetBlacklistDuration = 3f;

    private BombCarrier currentTarget;
    private BombCarrier blacklistedTarget;
    private float blacklistTimer;

    public float VisionMultiplier { get; set; } = 1f;

    private Vector2 positionAtLastStuckCheck;
    private float stuckCheckTimer;

    private BombCarrier myBombCarrier;
    private PlayerController myController;

    private float horizontalDecision;
    private bool jumpDecision;
    private bool dashDecision;
    private float decisionTimer;

    private AIState currentState;
    private float lastTargetDistance = float.MaxValue;

    private float wanderTargetX;
    private float wanderTimer;

    private void Awake()
    {
        myBombCarrier = GetComponent<BombCarrier>();
        myController = GetComponent<PlayerController>();
        PickNewWanderTarget();

        positionAtLastStuckCheck = transform.position;
        stuckCheckTimer = stuckCheckInterval;
    }

    private void Update()
    {
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            MakeDecision();
            decisionTimer = decisionInterval;
        }

        CheckJumpNeeded(); // todo frame, pra n�o perder o timing do pulo
        UpdateBlacklistTimer();
        CheckIfStuck();
    }

    private void MakeDecision()
    {
        BombCarrier target = FindRelevantTarget();
        currentTarget = target;

        if (target == null)
        {
            currentState = AIState.Wander;
            lastTargetDistance = float.MaxValue;
            dashDecision = false;
            UpdateWander();
            return;
        }

        float directionToTarget = Mathf.Sign(target.transform.position.x - transform.position.x);
        lastTargetDistance = Vector2.Distance(transform.position, target.transform.position);

        currentState = myBombCarrier.HasBomb ? AIState.Chase : AIState.Flee;
        horizontalDecision = myBombCarrier.HasBomb ? directionToTarget : -directionToTarget;

        EvaluateDash();
    }

    private void EvaluateDash()
    {
        switch (currentState)
        {
            case AIState.Flee:
                dashDecision = lastTargetDistance <= fleeDashDistance;
                break;

            case AIState.Chase:
                // s� faz sentido dashar se ainda n�o est� colado no alvo (sen�o desperdi�a o dash)
                dashDecision = lastTargetDistance > 1f && lastTargetDistance <= chaseDashDistance;
                break;

            default:
                dashDecision = false;
                break;
        }
    }

    private void UpdateWander()
    {
        wanderTimer -= decisionInterval;

        float distToTarget = Mathf.Abs(transform.position.x - wanderTargetX);
        if (wanderTimer <= 0f || distToTarget < 0.3f)
            PickNewWanderTarget();

        horizontalDecision = Mathf.Sign(wanderTargetX - transform.position.x);
    }

    private void PickNewWanderTarget()
    {
        wanderTargetX = transform.position.x + Random.Range(-wanderRadius, wanderRadius);
        wanderTimer = wanderInterval;
    }

    private BombCarrier FindRelevantTarget()
    {
        if (BombManager.Instance == null) return null;

        BombCarrier nearest = null;
        float nearestDist = float.MaxValue;

        foreach (BombCarrier candidate in BombManager.Instance.ActivePlayers)
        {
            if (candidate == myBombCarrier) continue;
            if (candidate == blacklistedTarget) continue; // ignora quem travou at� o cooldown acabar
            if (!myBombCarrier.HasBomb && !candidate.HasBomb) continue;

            float dist = Vector2.Distance(transform.position, candidate.transform.position);

            if (!myBombCarrier.HasBomb && dist > awarenessRadius * VisionMultiplier) continue;

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = candidate;
            }
        }

        return nearest;
    }

    private void CheckJumpNeeded()
    {
        if (myController == null || !myController.IsGrounded || horizontalDecision == 0f)
        {
            jumpDecision = false;
            return;
        }

        Vector2 direction = new Vector2(Mathf.Sign(horizontalDecision), 0f);

        bool wallAhead = Physics2D.Raycast(transform.position, direction, jumpCheckDistance, obstacleLayer);
        bool gapAhead = IsGapAhead(direction);

        if (wallAhead)
        {
            jumpDecision = true;
            return;
        }

        if (gapAhead)
        {
            // S� arrisca pular o buraco se realmente tem motivo (perseguindo ou fugindo).
            // Vagueando, prefere trocar de destino a se jogar num precip�cio.
            if (currentState == AIState.Wander)
            {
                PickNewWanderTarget();
                jumpDecision = false;
            }
            else
            {
                jumpDecision = true;
            }
            return;
        }

        jumpDecision = false;
    }

    private void UpdateBlacklistTimer()
    {
        if (blacklistTimer <= 0f) return;

        blacklistTimer -= Time.deltaTime;
        if (blacklistTimer <= 0f)
            blacklistedTarget = null; // depois do cooldown, volta a considerar esse alvo normalmente
    }

    private void CheckIfStuck()
    {
        stuckCheckTimer -= Time.deltaTime;
        if (stuckCheckTimer > 0f) return;

        stuckCheckTimer = stuckCheckInterval;

        float distMoved = Mathf.Abs(transform.position.x - positionAtLastStuckCheck.x);
        positionAtLastStuckCheck = transform.position;

        bool tryingToMove = horizontalDecision != 0f;
        bool failedToProgress = distMoved < stuckMovementThreshold;

        if (tryingToMove && failedToProgress && currentTarget != null)
        {
            blacklistedTarget = currentTarget;
            blacklistTimer = targetBlacklistDuration;
        }
    }

    private bool IsGapAhead(Vector2 direction)
    {
        Vector2 checkOrigin = (Vector2)transform.position + direction * groundAheadOffset;
        RaycastHit2D groundHit = Physics2D.Raycast(checkOrigin, Vector2.down, groundCheckDistance, obstacleLayer);
        return groundHit.collider == null; // se n�o achou ch�o, � buraco
    }

    public float GetHorizontal() => horizontalDecision;
    public bool JumpPressed() => jumpDecision;
    public bool DashPressed() => dashDecision;
}