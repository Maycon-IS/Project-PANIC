
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Cenas de Gameplay")]
    [SerializeField] private string sceneNamePlayOff;
    [SerializeField] private string sceneNamePlayOn;

    [Header("Chuva")]
    [SerializeField] private GameObject rain;
    //[SerializeField] private bool isRain = true;

    [Header("Paineis Principais")]
    [SerializeField] private GameObject gameObjetPE;
    [SerializeField] private GameObject gameObjectPS;
    [SerializeField] private GameObject panelPlay;
    [SerializeField] private GameObject panelSettings;
    [SerializeField] private GameObject panelCretids;

    [Header("Navegacao")]
    [SerializeField] private KeyCode backKey = KeyCode.Escape;

    [Header("Elementos Visuais do Menu")]
    [SerializeField] private GameObject logo;

    [Header("Gerenciador Visual")]
    [SerializeField] private MenuVisual menuVisual;

    private readonly Stack<GameObject> historicoPaineis = new Stack<GameObject>();

    private GameObject painelAtual;
    private bool emTransicao = false;

    private void Start()
    {
        InitializationPaneis();

        if (menuVisual != null)
            menuVisual.Initialize();
    }

    private void Update()
    {
        if (emTransicao)
            return;

        InputCanPress();
    }

    // NAVEGACAO ENTRE PAINEIS
    public void OpenPanel(GameObject novoPainel)
    {
        if (emTransicao || novoPainel == null)
            return;

        if (novoPainel == painelAtual)
            return;

        // Tela inicial -> Menu principal
        if (painelAtual == gameObjetPE &&
            novoPainel == gameObjectPS)
        {
            StartCoroutine(TransitionToMenu());
            return;
        }

        ClearButtonSelection();

        if (painelAtual != null)
        {
            historicoPaineis.Push(painelAtual);
            painelAtual.SetActive(false);
        }

        novoPainel.SetActive(true);
        painelAtual = novoPainel;

        UpdateLogoVisibility();
    }

    public void BackPanel()
    {
        if (emTransicao || historicoPaineis.Count == 0)
            return;

        GameObject painelAnterior = historicoPaineis.Peek();

        // Menu principal -> Tela inicial
        if (painelAtual == gameObjectPS &&
            painelAnterior == gameObjetPE)
        {
            StartCoroutine(TransitionToStart());
            return;
        }

        ClearButtonSelection();

        if (painelAtual != null)
            painelAtual.SetActive(false);

        painelAtual = historicoPaineis.Pop();
        painelAtual.SetActive(true);

        UpdateLogoVisibility();
    }

    // TRANSICAO PARA O MENU
    private IEnumerator TransitionToMenu()
    {
        emTransicao = true;

        ClearButtonSelection();

        // Ativa o painel para poder animá-lo
        gameObjectPS.SetActive(true);

        if (menuVisual != null)
            yield return menuVisual.Animate(true);

        // Desativa a primeira tela ao terminar
        gameObjetPE.SetActive(false);

        historicoPaineis.Push(gameObjetPE);
        painelAtual = gameObjectPS;

        emTransicao = false;
    }

    // TRANSICAO PARA A TELA INICIAL
    private IEnumerator TransitionToStart()
    {
        emTransicao = true;

        ClearButtonSelection();

        // Reativa para permitir o fade do texto
        gameObjetPE.SetActive(true);

        if (menuVisual != null)
            yield return menuVisual.Animate(false);

        gameObjectPS.SetActive(false);

        painelAtual = historicoPaineis.Pop();

        emTransicao = false;
    }

    // BOTOES DO MENU
    public void btPlay()
    {
        OpenPanel(panelPlay);
    }

    public void btPlayOff()
    {
        SceneManager.LoadScene(sceneNamePlayOff);
    }

    public void btPlayOn()
    {
        SceneManager.LoadScene(sceneNamePlayOn);
    }

    public void btSettings()
    {
        OpenPanel(panelSettings);
    }

    public void btCredits()
    {
        OpenPanel(panelCretids);
    }

    public void btReturn()
    {
        BackPanel();
    }

    public void btExit()
    {
        Debug.Log("Saida");

#if UNITY_WEBGL && !UNITY_EDITOR
    // Redireciona para a página desejada
    Application.OpenURL("https://maycon-isac.itch.io/last-furse-rival-arena");

#else
        // Fecha o jogo na versão de computador 
        Application.Quit();

#endif
    }

    // CONTROLE DO TECLADO
    private void InputCanPress()
    {
        if (Input.GetKeyDown(backKey))
        {
            BackPanel();
            return;
        }

        // Somente na tela inicial
        if (painelAtual == gameObjetPE &&
            Input.anyKeyDown)
        {
            OpenPanel(gameObjectPS);
        }
    }

    // SELECAO DOS BOTOES
    private void ClearButtonSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void UpdateLogoVisibility()
    {
        if (logo == null)
            return;

        bool mostrarLogo =
            painelAtual == gameObjetPE ||
            painelAtual == gameObjectPS;

        logo.SetActive(mostrarLogo);
    }

    // INICIALIZACAO
    private void InitializationPaneis()
    {
        historicoPaineis.Clear();

        gameObjetPE.SetActive(true);
        gameObjectPS.SetActive(false);

        panelPlay.SetActive(false);
        panelSettings.SetActive(false);
        panelCretids.SetActive(false);

        logo.SetActive(true);
        rain.SetActive(true);

        painelAtual = gameObjetPE;
        emTransicao = false;

        UpdateLogoVisibility();
    }
}
