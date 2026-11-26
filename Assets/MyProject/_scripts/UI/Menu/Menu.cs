
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Cenas de Gameplay")]
    [SerializeField] private string sceneNamePlayOn;
    [SerializeField] private string sceneNamePlayOff;

    [Header("Chuva")]
    [SerializeField] private GameObject rain;
    [SerializeField] private bool isRain = true;

    [Header("Paineis Principais")]
    [SerializeField] private GameObject gameObjetPE;
    [SerializeField] private GameObject gameObjectPS;
    [SerializeField] private GameObject panelPlay;
    [SerializeField] private GameObject panelSettings;
    [SerializeField] private GameObject panelCretids;
    [SerializeField] private GameObject rooms;

    [Header("Butões: Para Ativa ou Desativa")]
    [SerializeField] private Button[] buttons;

    [Header("Navegacao")]
    [SerializeField] private KeyCode backKey = KeyCode.Escape;

    [Header("Gerenciador e Elementos Visuais do Menu")]
    [SerializeField] private GameObject logo;
    [SerializeField] private MenuVisual menuVisual;

    [Header("Transicoes des Paineis")]
    [SerializeField] private PanelPlayTransition playTransition;
    [SerializeField] private PanelCreditsTransition creditsTransition;

    private readonly Stack<GameObject> historicoPaineis = new Stack<GameObject>();

    private GameObject painelAtual;
    private bool emTransicao = false;

    private void Start()
    {
        InitializationPaneis();

        verificationPanels();
    }

    private void Update()
    {
        if (emTransicao)
            return;

        InputCanPress();
        VerificationButtons();
        SelectRain();
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

        if (painelAtual == gameObjectPS && novoPainel == panelPlay &&
            playTransition != null)
        {
            StartCoroutine(TransitionToPlay());
            return;
        }

        if (painelAtual == gameObjectPS && novoPainel == panelCretids &&
            creditsTransition != null)
        {
            StartCoroutine(TransitionToCredits());
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

        if (painelAtual == panelPlay && painelAnterior == gameObjectPS &&
            playTransition != null)
        {
            StartCoroutine(TransitionFromPlay());
            return;
        }

        if (painelAtual == panelCretids && painelAnterior == gameObjectPS &&
            creditsTransition != null)
        {
            StartCoroutine(TransitionFromCredits());
            return;
        }

        ClearButtonSelection();

        if (painelAtual != null)
            painelAtual.SetActive(false);

        painelAtual = historicoPaineis.Pop();
        painelAtual.SetActive(true);

        UpdateLogoVisibility();
    }

    // ATIVACAO DOS BUTOES
    private void VerificationButtons() 
    { 
        if (buttons != null)
            DeactivationButtons();

        if (painelAtual == gameObjectPS && !AnyButtonInGameObjectPS())
            ActiveButtons();
    }

    private void ActiveButtons()
    {
        if (buttons != null)
        {
            foreach(var button in buttons)
            {
                button.interactable = true;
            }
        }
    }

    private void DeactivationButtons()
    {
        if (buttons != null)
        {
            foreach (var button in buttons)
            {
                button.interactable = false;
            }
        }
    }

    private bool AnyButtonInGameObjectPS()
    {
        if (buttons == null || gameObjectPS == null)
            return false;

        Transform targetTransform = gameObjectPS.transform;

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (button.transform == targetTransform || button.transform.IsChildOf(targetTransform))
                return true;
        }

        return false;
    }

    // TRANSICAO PARA O MENU

    private IEnumerator TransitionToCredits()
    {
        emTransicao = true;

        ClearButtonSelection();

        // Ativa os creditos para serem animados
        panelCretids.SetActive(true);

        // Mantem o menu principal visivel
        // enquanto ele desaparece gradualmente
        yield return creditsTransition.Animate(true);

        // Desativa o menu antigo
        gameObjectPS.SetActive(false);

        // Atualiza o historico
        historicoPaineis.Push(gameObjectPS);
        painelAtual = panelCretids;

        emTransicao = false;
    }

    private IEnumerator TransitionFromCredits()
    {
        emTransicao = true;

        ClearButtonSelection();

        // Reativa o menu principal para o fade
        gameObjectPS.SetActive(true);

        // Executa a animacao ao contrario
        yield return creditsTransition.Animate(false);

        // Esconde o painel Creditos
        panelCretids.SetActive(false);

        // Recupera o painel anterior
        painelAtual = historicoPaineis.Pop();

        emTransicao = false;
    }

    private IEnumerator TransitionToPlay()
    {
        emTransicao = true;

        ClearButtonSelection();

        // Ativa o painel novo, mas ele inicia invisível
        panelPlay.SetActive(true);

        // Mantém o menu principal ativo
        // enquanto a animação acontece
        yield return playTransition.Animate(true);

        // Agora pode desativar o menu antigo
        gameObjectPS.SetActive(false);

        // Atualiza o histórico
        historicoPaineis.Push(gameObjectPS);
        painelAtual = panelPlay;

        emTransicao = false;
    }
    private IEnumerator TransitionFromPlay()
    {
        emTransicao = true;

        ClearButtonSelection();

        // Reativa o menu principal,
        // que começa transparente
        gameObjectPS.SetActive(true);

        // Executa a animação inversa
        yield return playTransition.Animate(false);

        // Desativa o painel Jogar
        panelPlay.SetActive(false);

        // Recupera o painel anterior
        painelAtual = historicoPaineis.Pop();

        emTransicao = false;
    }

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
        Debug.Log("Saindo do Last Fuse...");

#if UNITY_EDITOR

        // Encerra o Play Mode dentro da Unity
        UnityEditor.EditorApplication.isPlaying = false;

#elif UNITY_WEBGL

    // Redireciona para a página do jogo
    Time.timeScale = 0f;
    Application.OpenURL(
        "https://maycon-isac.itch.io/last-furse-rival-arena"
    );

#else

    // Encerra o executável no Windows
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
        rooms.SetActive(true);

        painelAtual = gameObjetPE;
        emTransicao = false;

        UpdateLogoVisibility();
    }

    private void SelectRain() 
    {
        //rain.SetActive(!isRain);

#if UNITY_EDITOR
        if (isRain == true)
        {
            rain.SetActive(true);
        }
        else if (isRain == false)
        {
            rain.SetActive(false);
        }

#endif
    }

    private void verificationPanels()
    {
        if (menuVisual != null)
            menuVisual.Initialize();

        if (playTransition != null)
            playTransition.Initialize();

        if (creditsTransition != null)
            creditsTransition.Initialize();
    }
}
