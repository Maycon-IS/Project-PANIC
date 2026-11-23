
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUIController : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject eliminationPrompt;
    [SerializeField] private GameObject resultsPanel;

    [Header("Eliminação")]
    [SerializeField] private TMP_Text eliminationTimerText;
    [SerializeField] private float eliminationPromptDuration = 3f;

    [Header("Resultados")]
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text totalRoundsText;
    [SerializeField] private TMP_Text totalEventsText;

    [Header("Cenas")]
    [SerializeField] private string menuScene = "Menu";
    [SerializeField] private string lobbyScene = "LobbyOff";

    private bool isPaused;
    private bool isShowingResults;

    private bool eliminationPromptActive;
    private float eliminationPromptRemaining;

    private void OnEnable()
    {
        BombManager.OnHumanEliminated +=
            HandleHumanEliminated;

        BombManager.OnMatchEnded +=
            HandleMatchEnded;
    }

    private void OnDisable()
    {
        BombManager.OnHumanEliminated -=
            HandleHumanEliminated;

        BombManager.OnMatchEnded -=
            HandleMatchEnded;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        SetPanel(pausePanel, false);
        SetPanel(eliminationPrompt, false);
        SetPanel(resultsPanel, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            HandleEscape();

        UpdateEliminationPrompt();
    }

    private void HandleEscape()
    {
        if (isShowingResults)
            return;

        BombManager manager = BombManager.Instance;

        if (manager == null)
            return;

        if (manager.Phase ==
            BombManager.MatchPhase.WaitingToStart)
            return;

        if (manager.Phase ==
            BombManager.MatchPhase.Finished)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused || isShowingResults)
            return;

        BombManager manager = BombManager.Instance;

        if (manager == null ||
            manager.Phase ==
            BombManager.MatchPhase.WaitingToStart ||
            manager.IsMatchFinished)
            return;

        isPaused = true;

        Time.timeScale = 0f;

        SetPanel(pausePanel, true);
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        SetPanel(pausePanel, false);

        isPaused = false;
        Time.timeScale = 1f;
    }

    private void HandleHumanEliminated()
    {
        if (isShowingResults)
            return;

        eliminationPromptActive = true;
        eliminationPromptRemaining =
            eliminationPromptDuration;

        SetPanel(eliminationPrompt, true);

        UpdateEliminationTimerText();
    }

    private void UpdateEliminationPrompt()
    {
        if (!eliminationPromptActive ||
            isShowingResults ||
            isPaused)
            return;

        eliminationPromptRemaining -= Time.deltaTime;

        UpdateEliminationTimerText();

        if (eliminationPromptRemaining <= 0f)
        {
            eliminationPromptActive = false;
            SetPanel(eliminationPrompt, false);
        }
    }

    private void UpdateEliminationTimerText()
    {
        if (eliminationTimerText == null)
            return;

        eliminationTimerText.text =
            "Desaparece em " +
            Mathf.Max(0f, eliminationPromptRemaining)
                .ToString("F1") +
            "s";
    }

    public void EndMatchNow()
    {
        if (isShowingResults)
            return;

        eliminationPromptActive = false;
        SetPanel(eliminationPrompt, false);

        BombManager.Instance?.EndMatchEarly();
    }

    private void HandleMatchEnded(BombCarrier winner)
    {
        isShowingResults = true;
        eliminationPromptActive = false;
        isPaused = false;

        SetPanel(pausePanel, false);
        SetPanel(eliminationPrompt, false);

        if (resultTitleText != null)
        {
            resultTitleText.text =
                winner != null
                    ? "FIM DE PARTIDA"
                    : "PARTIDA ENCERRADA";
        }

        if (winnerText != null)
        {
            winnerText.text =
                winner != null
                    ? winner.name + " VENCEU!"
                    : "SEM VENCEDOR";
        }

        BombManager bombManager =
            BombManager.Instance;

        ArenaEventManager arenaManager =
            ArenaEventManager.Instance;

        if (totalRoundsText != null)
        {
            totalRoundsText.text =
                "RODADAS: " +
                (bombManager != null
                    ? bombManager.RoundCount
                    : 0);
        }

        if (totalEventsText != null)
        {
            totalEventsText.text =
                "EVENTOS: " +
                (arenaManager != null
                    ? arenaManager.ActivatedEventCount
                    : 0);
        }

        SetPanel(resultsPanel, true);

        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(menuScene);
    }

    public void RestartMatch()
    {
        Time.timeScale = 1f;

        // Reinicia a mesma cena e volta ao lobby.
        SceneManager.LoadScene(lobbyScene);
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
