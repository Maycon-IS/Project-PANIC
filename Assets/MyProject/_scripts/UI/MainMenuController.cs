using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Painéis do Menu")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    private void Start()
    {
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        CloseAllPanels();

        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    public void OpenPlayMenu()
    {
        CloseAllPanels();

        if (playPanel != null)
            playPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseAllPanels();

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        CloseAllPanels();

        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    private void CloseAllPanels()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (playPanel != null)
            playPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Fechando o jogo...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

