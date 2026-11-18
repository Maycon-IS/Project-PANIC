
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelCreditsTransition : MonoBehaviour
{
    [Header("Menu Principal")]
    [SerializeField] private CanvasGroup menuPrincipal;
    [SerializeField] private CanvasGroup logoPrincipal;

    [Header("Painel de Creditos")]
    [SerializeField] private CanvasGroup painelCredits;

    [Header("Conteudo dos Creditos")]
    [SerializeField] private RectTransform contentCredits;
    [SerializeField] private CanvasGroup grupoContent;

    [Header("Botao Voltar")]
    [SerializeField] private CanvasGroup grupoVoltar;

    [Header("Configuracoes da Animacao")]
    [SerializeField] private float duracao = 0.65f;
    [SerializeField] private float distanciaSubida = 140f;

    private Vector2 posicaoOriginal;
    private float progresso = 0f;
    private bool inicializado = false;

    // =====================================
    // INICIALIZACAO
    // =====================================

    public void Initialize()
    {
        if (menuPrincipal == null ||
            logoPrincipal == null ||
            painelCredits == null ||
            contentCredits == null ||
            grupoContent == null ||
            grupoVoltar == null)
        {
            Debug.LogError(
                "PanelCreditsTransition: faltam referencias!"
            );
            return;
        }

        posicaoOriginal = contentCredits.anchoredPosition;

        progresso = 0f;
        inicializado = true;

        AplicarVisual(0f);
        ConfigurarInteracao(false);
    }

    // =====================================
    // ANIMACAO DE ENTRADA E SAIDA
    // =====================================

    public IEnumerator Animate(bool abrirCredits)
    {
        if (!inicializado)
            yield break;

        LimparSelecao();
        BloquearInteracao();

        // Mantem a logo disponivel para o fade
        logoPrincipal.gameObject.SetActive(true);

        float origem = progresso;
        float destino = abrirCredits ? 1f : 0f;

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
        ConfigurarInteracao(abrirCredits);
    }

    // =====================================
    // CONTROLE VISUAL
    // =====================================

    private void AplicarVisual(float valor)
    {
        // 1. O menu principal desaparece
        float saidaMenu = Suavizar(valor / 0.45f);

        menuPrincipal.alpha = 1f - saidaMenu;
        logoPrincipal.alpha = 1f - saidaMenu;

        // 2. O fundo dos creditos aparece
        float entradaCredits = Suavizar(
            (valor - 0.20f) / 0.80f
        );

        painelCredits.alpha = entradaCredits;

        // 3. Todo o conteudo sobe junto
        float entradaContent = Suavizar(
            (valor - 0.30f) / 0.65f
        );

        Vector2 posicaoInicial =
            posicaoOriginal + Vector2.down * distanciaSubida;

        contentCredits.anchoredPosition = Vector2.Lerp(
            posicaoInicial,
            posicaoOriginal,
            entradaContent
        );

        grupoContent.alpha = entradaContent;

        // 4. O botao VOLTAR somente aparece
        float entradaVoltar = Suavizar(
            (valor - 0.65f) / 0.35f
        );

        grupoVoltar.alpha = entradaVoltar;
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
    // BLOQUEIO DA INTERACAO
    // =====================================

    private void BloquearInteracao()
    {
        menuPrincipal.interactable = false;
        menuPrincipal.blocksRaycasts = false;

        painelCredits.interactable = false;
        painelCredits.blocksRaycasts = false;

        grupoContent.interactable = false;
        grupoContent.blocksRaycasts = false;

        grupoVoltar.interactable = false;
        grupoVoltar.blocksRaycasts = false;
    }

    private void ConfigurarInteracao(bool creditsAberto)
    {
        menuPrincipal.interactable = !creditsAberto;
        menuPrincipal.blocksRaycasts = !creditsAberto;

        painelCredits.interactable = creditsAberto;
        painelCredits.blocksRaycasts = creditsAberto;

        grupoContent.interactable = creditsAberto;
        grupoContent.blocksRaycasts = creditsAberto;

        grupoVoltar.interactable = creditsAberto;
        grupoVoltar.blocksRaycasts = creditsAberto;
    }

    private void LimparSelecao()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
