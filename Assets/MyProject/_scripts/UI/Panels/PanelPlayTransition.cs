
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelPlayTransition : MonoBehaviour
{
    [Header("Paineis")]
    [SerializeField] private CanvasGroup menuPrincipal;
    [SerializeField] private CanvasGroup painelPlay;

    [Header("Logo Principal")]
    [SerializeField] private CanvasGroup logoPrincipal;

    [Header("Cartao Local")]
    [SerializeField] private RectTransform cartaoLocal;
    [SerializeField] private CanvasGroup grupoLocal;

    [Header("Cartao Online")]
    [SerializeField] private RectTransform cartaoOnline;
    [SerializeField] private CanvasGroup grupoOnline;

    [Header("Configuracoes da Animacao")]
    [SerializeField] private float duracao = 0.65f;
    [SerializeField] private float distanciaSubida = 120f;

    private Vector2 posOriginalLocal;
    private Vector2 posOriginalOnline;

    private bool inicializado = false;

    // 0 = Menu Principal
    // 1 = Painel Jogar
    private float progresso = 0f;

    // =====================================
    // INICIALIZACAO
    // =====================================

    public void Initialize()
    {
        if (menuPrincipal == null ||
            painelPlay == null ||
            logoPrincipal == null ||
            cartaoLocal == null ||
            cartaoOnline == null ||
            grupoLocal == null ||
            grupoOnline == null)
        {
            Debug.LogError(
                "PanelPlayTransition: faltam referencias!"
            );
            return;
        }

        posOriginalLocal = cartaoLocal.anchoredPosition;
        posOriginalOnline = cartaoOnline.anchoredPosition;

        inicializado = true;
        progresso = 0f;

        AplicarVisual(progresso);
        ConfigurarInteracao(false);
    }

    // =====================================
    // ANIMACAO DE ENTRADA E SAIDA
    // =====================================

    public IEnumerator Animate(bool abrirPlay)
    {
        if (!inicializado)
            yield break;

        LimparSelecao();
        BloquearInteracao();

        // Mantem a logo ativa durante o fade
        logoPrincipal.gameObject.SetActive(true);

        float origem = progresso;
        float destino = abrirPlay ? 1f : 0f;

        float tempo = 0f;
        float tempoTotal = Mathf.Max(0.01f, duracao);

        while (tempo < tempoTotal)
        {
            tempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tempo / tempoTotal);
            t = Mathf.SmoothStep(0f, 1f, t);

            progresso = Mathf.Lerp(origem, destino, t);

            AplicarVisual(progresso);

            yield return null;
        }

        progresso = destino;
        AplicarVisual(progresso);

        LimparSelecao();
        ConfigurarInteracao(abrirPlay);
    }

    // =====================================
    // MOVIMENTO E TRANSPARENCIA
    // =====================================

    private void AplicarVisual(float valor)
    {
        // Menu principal desaparece
        float saidaMenu = Suavizar(valor / 0.45f);

        menuPrincipal.alpha = 1f - saidaMenu;
        logoPrincipal.alpha = 1f - saidaMenu;

        // Painel Jogar aparece
        float entradaPlay = Suavizar(
            (valor - 0.25f) / 0.75f
        );

        painelPlay.alpha = entradaPlay;

        // Cartao Local
        float tLocal = Suavizar(
            (valor - 0.35f) / 0.55f
        );

        cartaoLocal.anchoredPosition = Vector2.Lerp(
            posOriginalLocal + Vector2.down * distanciaSubida,
            posOriginalLocal,
            tLocal
        );

        grupoLocal.alpha = tLocal;

        // Cartao Online
        float tOnline = Suavizar(
            (valor - 0.45f) / 0.55f
        );

        cartaoOnline.anchoredPosition = Vector2.Lerp(
            posOriginalOnline + Vector2.down * distanciaSubida,
            posOriginalOnline,
            tOnline
        );

        grupoOnline.alpha = tOnline;
    }

    // =====================================
    // SUAVIZACAO
    // =====================================

    private float Suavizar(float valor)
    {
        valor = Mathf.Clamp01(valor);
        return Mathf.SmoothStep(0f, 1f, valor);
    }

    // =====================================
    // CONTROLE DE INTERACAO
    // =====================================

    private void BloquearInteracao()
    {
        menuPrincipal.interactable = false;
        menuPrincipal.blocksRaycasts = false;

        painelPlay.interactable = false;
        painelPlay.blocksRaycasts = false;

        grupoLocal.interactable = false;
        grupoLocal.blocksRaycasts = false;

        grupoOnline.interactable = false;
        grupoOnline.blocksRaycasts = false;
    }

    private void ConfigurarInteracao(bool playAberto)
    {
        menuPrincipal.interactable = !playAberto;
        menuPrincipal.blocksRaycasts = !playAberto;

        painelPlay.interactable = playAberto;
        painelPlay.blocksRaycasts = playAberto;

        grupoLocal.interactable = playAberto;
        grupoLocal.blocksRaycasts = playAberto;

        grupoOnline.interactable = playAberto;
        grupoOnline.blocksRaycasts = playAberto;
    }

    // =====================================
    // LIMPEZA DA SELECAO
    // =====================================

    private void LimparSelecao()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
