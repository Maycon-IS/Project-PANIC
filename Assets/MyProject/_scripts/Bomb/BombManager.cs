
using System;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    public static BombManager Instance { get; private set; }

    public enum MatchPhase
    {
        WaitingToStart,
        Playing,
        Intermission,
        Finished
    }

    [Header("Configuração da Bomba")]
    [SerializeField] private float bombTimer = 8f;
    [SerializeField] private float transferImmunity = 0.3f;
    [SerializeField] private float reassignDelay = 3f;
    [SerializeField] private float transferTimeBonus = 1f;

    [Header("Regras")]
    [SerializeField] private int eliminationsBeforeEvents = 2;

    private readonly List<BombCarrier> allPlayers =
        new List<BombCarrier>();

    private BombCarrier currentCarrier;
    private BombCarrier lastCarrier;

    private float timeRemaining;
    private float immunityRemaining;
    private float delayRemaining;

    private int roundCount;
    private int eliminationCount;

    public MatchPhase Phase { get; private set; } = MatchPhase.WaitingToStart;

    public IReadOnlyList<BombCarrier> ActivePlayers => allPlayers;
    public BombCarrier CurrentCarrier => currentCarrier;

    public int RoundCount => roundCount;
    public int EliminationCount => eliminationCount;
    public bool IsMatchFinished => Phase == MatchPhase.Finished;

    public static event Action OnRoundEnded;
    public static event Action OnRoundStarted;
    public static event Action OnHumanEliminated;
    public static event Action<BombCarrier> OnMatchEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        PlayerElimination.OnPlayerEliminated +=
            HandlePlayerEliminated;
    }

    private void OnDisable()
    {
        PlayerElimination.OnPlayerEliminated -=
            HandlePlayerEliminated;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RegisterPlayer(BombCarrier player)
    {
        if (player == null)
            return;

        if (Phase != MatchPhase.WaitingToStart)
        {
            Debug.LogWarning(
                "Tentativa de registrar jogador após início."
            );
            return;
        }

        if (!allPlayers.Contains(player))
            allPlayers.Add(player);
    }

    public void UnregisterPlayer(BombCarrier player)
    {
        allPlayers.Remove(player);
    }

    public void StartMatch()
    {
        if (Phase != MatchPhase.WaitingToStart)
            return;

        if (allPlayers.Count < 3)
        {
            Debug.LogError(
                "Partida requer pelo menos 3 participantes."
            );
            return;
        }

        roundCount = 0;
        eliminationCount = 0;

        timeRemaining = 0f;
        delayRemaining = 0f;
        immunityRemaining = 0f;

        currentCarrier = null;
        lastCarrier = null;

        ArenaEventManager.Instance?.ResetEvents();

        Phase = MatchPhase.Playing;

        AssignBombToRandomPlayer();
    }

    private void Update()
    {
        switch (Phase)
        {
            case MatchPhase.Playing:
                UpdateBomb();
                break;

            case MatchPhase.Intermission:
                UpdateIntermission();
                break;
        }
    }

    private void UpdateBomb()
    {
        if (currentCarrier == null)
            return;

        if (immunityRemaining > 0f)
        {
            immunityRemaining = Mathf.Max(
                0f,
                immunityRemaining - Time.deltaTime
            );
        }

        timeRemaining -= Time.deltaTime;

        MainGameView.Instance?.UpdateTimeBomb(
            Mathf.Max(0f, timeRemaining)
        );

        if (timeRemaining <= 0f)
            Explode();
    }

    private void UpdateIntermission()
    {
        delayRemaining -= Time.deltaTime;

        MainGameView.Instance?.UpdateDelayBomb(
            Mathf.Max(0f, delayRemaining)
        );

        if (delayRemaining <= 0f)
        {
            delayRemaining = 0f;
            AssignBombToRandomPlayer();
        }
    }

    public void TransferBomb(BombCarrier from, BombCarrier to)
    {
        if (Phase != MatchPhase.Playing)
            return;

        if (from == null || to == null || from == to)
            return;

        if (from != currentCarrier)
            return;

        // Garante que ambos ainda estao vivos na partida
        if (!allPlayers.Contains(from) ||
            !allPlayers.Contains(to))
            return;

        // Protecao adicional contra jogadores mortos
        PlayerElimination fromElimination =
            from.GetComponent<PlayerElimination>();

        PlayerElimination toElimination =
            to.GetComponent<PlayerElimination>();

        if (fromElimination != null && fromElimination.IsDead)
            return;

        if (toElimination != null && toElimination.IsDead)
            return;

        if (immunityRemaining > 0f && to == lastCarrier)
            return;

        from.SetBombState(false);
        to.SetBombState(true);

        lastCarrier = from;
        currentCarrier = to;

        timeRemaining += transferTimeBonus;
        immunityRemaining = transferImmunity;

        MainGameView.Instance?.UpdateTimeBomb(timeRemaining);
    }

    private void Explode()
    {
        if (currentCarrier == null)
            return;

        PlayerElimination elimination =
            currentCarrier.GetComponent<PlayerElimination>();

        if (elimination != null)
            elimination.Kill();
        else
            Debug.LogError(
                "Portador da bomba sem PlayerElimination."
            );
    }

    private void HandlePlayerEliminated(GameObject eliminatedGO)
    {
        if (Phase != MatchPhase.Playing &&
            Phase != MatchPhase.Intermission)
            return;

        BombCarrier eliminated =
            eliminatedGO.GetComponent<BombCarrier>();

        if (eliminated == null ||
            !allPlayers.Contains(eliminated))
            return;

        bool isHuman =
            eliminated.GetComponent<PlayerInputProvider>() != null;

        bool wasCarrier = eliminated == currentCarrier;

        eliminated.SetBombState(false);

        allPlayers.Remove(eliminated);
        eliminationCount++;

        if (lastCarrier == eliminated)
            lastCarrier = null;

        // Último sobrevivente encerra a partida,
        // independentemente de quem tinha a bomba.
        if (allPlayers.Count <= 1)
        {
            BombCarrier winner =
                allPlayers.Count == 1 ? allPlayers[0] : null;

            FinishMatch(winner);
            return;
        }

        // Se a morte aconteceu fora da rodada ativa,
        // ou o eliminado não possuía a bomba,
        // mantém o ciclo atual.
        if (!wasCarrier)
        {
            if (isHuman)
                OnHumanEliminated?.Invoke();

            return;
        }

        // O portador morreu: encerra a rodada.
        currentCarrier = null;
        lastCarrier = null;

        timeRemaining = 0f;
        immunityRemaining = 0f;

        ArenaEventManager.Instance?.DeactivateCurrentEvent();

        MainGameView.Instance?.UpdateTimeBomb(0f);

        OnRoundEnded?.Invoke();

        Phase = MatchPhase.Intermission;
        delayRemaining = reassignDelay;

        MainGameView.Instance?.UpdateDelayBomb(
            delayRemaining
        );

        // Eventos só começam depois da
        // quantidade definida de eliminações.
        if (eliminationCount >= eliminationsBeforeEvents)
        {
            ArenaEventManager.Instance?.PrepareNextEvent();
        }

        if (isHuman)
            OnHumanEliminated?.Invoke();
    }

    private void AssignBombToRandomPlayer()
    {
        if (Phase == MatchPhase.Finished)
            return;

        if (allPlayers.Count == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(
            0,
            allPlayers.Count
        );

        currentCarrier = allPlayers[randomIndex];

        lastCarrier = null;
        immunityRemaining = 0f;

        currentCarrier.SetBombState(true);

        timeRemaining = bombTimer;
        delayRemaining = 0f;

        roundCount++;
        Phase = MatchPhase.Playing;

        MainGameView.Instance?.UpdateTimeBomb(timeRemaining);
        MainGameView.Instance?.UpdateDelayBomb(0f);

        ArenaEventManager.Instance?.ActivatePreparedEvent();

        OnRoundStarted?.Invoke();

        Debug.Log(
            $"Rodada {roundCount}: {currentCarrier.name} recebeu a bomba."
        );
    }

    private void FinishMatch(BombCarrier winner)
    {
        if (Phase == MatchPhase.Finished)
            return;

        Phase = MatchPhase.Finished;

        if (currentCarrier != null)
            currentCarrier.SetBombState(false);

        currentCarrier = null;
        lastCarrier = null;

        timeRemaining = 0f;
        delayRemaining = 0f;
        immunityRemaining = 0f;

        // Desativa eventos sem apagar estatísticas.
        ArenaEventManager.Instance?.ResetEvents(false);

        MainGameView.Instance?.UpdateTimeBomb(0f);
        MainGameView.Instance?.UpdateDelayBomb(0f);

        OnMatchEnded?.Invoke(winner);
    }

    public void EndMatchEarly()
    {
        if (Phase == MatchPhase.WaitingToStart ||
            Phase == MatchPhase.Finished)
            return;

        FinishMatch(null);
    }
}
