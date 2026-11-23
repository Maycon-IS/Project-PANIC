
using System.Globalization;
using TMPro;
using UnityEngine;

public class MainGameView : MonoBehaviour
{
    public static MainGameView Instance { get; private set; }

    [Header("Temporizadores")]
    [SerializeField] private TMP_Text timeBombText;
    [SerializeField] private TMP_Text timeDelayBombText;
    [SerializeField] private TMP_Text txtNameActionBomb;

    [Header("Visual da Bomba")]
    [SerializeField] private GameObject bombTimerBackground;

    [Header("Eventos")]
    [SerializeField] private TMP_Text currentEventText;
    [SerializeField] private TMP_Text nextEventText;
    [SerializeField] private TMP_Text txtNameActionEvent;

    [Header("Rodadas")]
    [SerializeField] private TMP_Text roundText;

    private float bombTime;
    private float delayTime;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        bombTime = 0f;
        delayTime = 0f;

        RefreshHUD();
    }

    private void Update()
    {
        RefreshHUD();
    }

    // Atualizado pelo BombManager durante a bomba.
    public void UpdateTimeBomb(float time)
    {
        bombTime = Mathf.Max(0f, time);
    }

    // Atualizado pelo BombManager durante a espera.
    public void UpdateDelayBomb(float time)
    {
        delayTime = Mathf.Max(0f, time);
    }

    private void RefreshHUD()
    {
        BombManager bombManager = BombManager.Instance;
        ArenaEventManager eventManager =
            ArenaEventManager.Instance;

        if (bombManager == null)
            return;

        bool playing =
            bombManager.Phase == BombManager.MatchPhase.Playing;

        bool intermission =
            bombManager.Phase == BombManager.MatchPhase.Intermission;

        // Exibe a HUD somente durante a partida.
        bool showHUD = playing || intermission;

        // ========================================
        // TEMPORIZADORES
        // ========================================

        SetTextVisible(timeBombText, playing);
        SetTextVisible(timeDelayBombText, intermission);

        // Fundo vermelho somente durante a explosão.
        if (bombTimerBackground != null)
            bombTimerBackground.SetActive(playing);

        if (timeBombText != null && playing)
            timeBombText.text = FormatTime(bombTime);

        if (timeDelayBombText != null && intermission)
            timeDelayBombText.text = FormatTime(delayTime);

        if (txtNameActionBomb != null)
        {
            txtNameActionBomb.gameObject.SetActive(showHUD);

            if (playing)
                txtNameActionBomb.text = "Explosão";
            else if (intermission)
                txtNameActionBomb.text = "Próxima Bomba";
        }

        // ========================================
        // EVENTOS
        // ========================================

        SetTextVisible(currentEventText, playing);
        SetTextVisible(nextEventText, intermission);

        if (txtNameActionEvent != null)
        {
            txtNameActionEvent.gameObject.SetActive(showHUD);

            if (playing)
                txtNameActionEvent.text = "EVENTO ATUAL";
            else if (intermission)
                txtNameActionEvent.text = "PRÓXIMO EVENTO";
        }

        if (eventManager != null)
        {
            if (playing && currentEventText != null)
            {
                currentEventText.text =
                    eventManager.HasActiveEvent
                        ? eventManager.CurrentEventName.ToUpperInvariant()
                        : "SEM EVENTO";
            }

            if (intermission && nextEventText != null)
            {
                nextEventText.text =
                    eventManager.HasPreparedEvent
                        ? eventManager.NextEventName.ToUpperInvariant()
                        : "SEM EVENTO";
            }
        }
        else
        {
            if (playing && currentEventText != null)
                currentEventText.text = "SEM EVENTO";

            if (intermission && nextEventText != null)
                nextEventText.text = "SEM EVENTO";
        }

        // ========================================
        // RODADAS
        // ========================================

        if (roundText != null)
        {
            roundText.gameObject.SetActive(showHUD);

            if (showHUD)
                roundText.text =
                    "RODADA " + bombManager.RoundCount;
        }
    }

    private string FormatTime(float time)
    {
        // Formato: 08.00 / 03.50 / 00.00
        return Mathf.Max(0f, time).ToString(
            "00.00",
            CultureInfo.InvariantCulture
        );
    }

    private void SetTextVisible(TMP_Text text, bool visible)
    {
        if (text == null)
            return;

        if (text.gameObject.activeSelf != visible)
            text.gameObject.SetActive(visible);
    }
}
