using System;
using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    public static BombManager Instance { get; private set; }
    public IReadOnlyList<BombCarrier> ActivePlayers => allPlayers;

    [Header("Configuração da Bomba")]
    [SerializeField] private float bombTimer = 8f;
    [SerializeField] private float transferImmunity = 0.3f;
    [SerializeField] private float reassignDelay = 3f; // tempo de espera após eliminação

    private readonly List<BombCarrier> allPlayers = new List<BombCarrier>();
    private BombCarrier currentCarrier;
    private BombCarrier lastCarrier;
    
    private float timeRemaining;
    private float immunityRemaining;

    private float delayRemaining;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Invoke(nameof(AssignBombToRandomPlayer), 0.1f);
        // pequeno delay pra garantir que todos os BombCarrier já rodaram seu Start() e se registraram
        MainGameView.Instance?.UpdateTimeBomb(bombTimer);
        MainGameView.Instance?.UpdateDelayBomb(0f);

    }

    private void Update()
    {
        if (immunityRemaining > 0f)
            immunityRemaining -= Time.deltaTime;

        if (currentCarrier == null)
        {
            if (delayRemaining > 0f) {
                delayRemaining -= Time.deltaTime;
                MainGameView.Instance?.UpdateDelayBomb(delayRemaining); 
            }

            return;
        }

        timeRemaining -= Time.deltaTime;

        MainGameView.Instance?.UpdateTimeBomb(timeRemaining);

        if (timeRemaining <= 0f)
            Explode();
    }

    public void RegisterPlayer(BombCarrier player)
    {
        if (!allPlayers.Contains(player))
            allPlayers.Add(player);
    }

    public void UnregisterPlayer(BombCarrier player)
    {
        allPlayers.Remove(player);
    }

    public void TransferBomb(BombCarrier from, BombCarrier to)
    {
        bool isImmuneTransfer = immunityRemaining > 0f && to == lastCarrier;
        if (isImmuneTransfer) return;

        from.SetBombState(false);
        to.SetBombState(true);

        lastCarrier = from;
        currentCarrier = to;
        timeRemaining = bombTimer;
        immunityRemaining = transferImmunity;

        MainGameView.Instance?.UpdateTimeBomb(timeRemaining);

    }

    private void Explode()
    {
        BombCarrier eliminated = currentCarrier;
        allPlayers.Remove(eliminated);
        currentCarrier = null;

        PlayerElimination elimination = eliminated.GetComponent<PlayerElimination>();
        if (elimination != null)
            elimination.Eliminate();

        if (CheckWinCondition()) return;

        delayRemaining = reassignDelay;

        Invoke(nameof(AssignBombToRandomPlayer), reassignDelay);
    }

    private void AssignBombToRandomPlayer()
    {
        if (allPlayers.Count == 0) return;

        int index = UnityEngine.Random.Range(0, allPlayers.Count);
        currentCarrier = allPlayers[index];
        currentCarrier.SetBombState(true);
        timeRemaining = bombTimer;
        lastCarrier = null;

        MainGameView.Instance?.UpdateTimeBomb(timeRemaining);
        MainGameView.Instance?.UpdateDelayBomb(0f);

    }

    private bool CheckWinCondition()
    {
        if (allPlayers.Count == 1)
        {
            Debug.Log($"{allPlayers[0].name} venceu a partida!");
            // TODO: disparar tela de resultados (fora do escopo desse sistema)
            return true;
        }
        return false;
    }
}