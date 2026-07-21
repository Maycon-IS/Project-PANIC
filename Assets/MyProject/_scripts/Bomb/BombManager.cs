using System.Collections.Generic;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    public static BombManager Instance { get; private set; }

    [Header("Configuração da Bomba")]
    [SerializeField] private float bombTimer = 8f;
    [SerializeField] private float transferImmunity = 0.3f;

    private readonly List<BombCarrier> allPlayers = new List<BombCarrier>();
    private BombCarrier currentCarrier;
    private BombCarrier lastCarrier;
    private float timeRemaining;
    private float immunityRemaining;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Invoke(nameof(AssignBombToRandomPlayer), 0.1f);
        // pequeno delay pra garantir que todos os BombCarrier já rodaram seu Start() e se registraram
    }

    private void Update()
    {
        if (immunityRemaining > 0f)
            immunityRemaining -= Time.deltaTime;

        if (currentCarrier == null) return;

        timeRemaining -= Time.deltaTime;
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

        AssignBombToRandomPlayer();
    }

    private void AssignBombToRandomPlayer()
    {
        if (allPlayers.Count == 0) return;

        int index = Random.Range(0, allPlayers.Count);
        currentCarrier = allPlayers[index];
        currentCarrier.SetBombState(true);
        timeRemaining = bombTimer;
        lastCarrier = null;
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