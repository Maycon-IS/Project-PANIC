
using System.Collections;
using UnityEngine;

public class MenuTransitionUI : MonoBehaviour
{
    [Header("Logo")]
    [SerializeField] private RectTransform logo;
    [SerializeField] private RectTransform posLogoInicio;
    [SerializeField] private RectTransform posLogoMenu;

    [Header("Tamanho da Logo")]
    [SerializeField] private float escalaInicio = 1f;
    [SerializeField] private float escalaMenu = 0.55f;

    [Header("Painel de Botões")]
    [SerializeField] private RectTransform painelBotoes;
    [SerializeField] private CanvasGroup grupoBotoes;
    [SerializeField] private float distanciaEntrada = 350f;

    [Header("Texto Começar")]
    [SerializeField] private CanvasGroup grupoComecar;

    [Header("Tempo da Transição")]
    [SerializeField] private float duracao = 0.65f;
    [Range(0f, 0.7f)]
    [SerializeField] private float atrasoBotoes = 0.15f;

    private Vector3 escalaOriginalLogo;
    private Vector2 posicaoOriginalBotoes;

    private float progresso = 0f;
    private bool inicializado = false;

    // =========================================
    // INICIALIZAÇÃO
    // =========================================

    public void Initialize()
    {
        if (logo == null ||
            posLogoInicio == null ||
            posLogoMenu == null ||
            painelBotoes == null ||
            grupoBotoes == null ||
            grupoComecar == null)
        {
            Debug.LogError(
                "MenuTransitionUI: faltam referências no Inspector!"
            );
            return;
        }

        escalaOriginalLogo = logo.localScale;
        posicaoOriginalBotoes = painelBotoes.anchoredPosition;

        inicializado = true;

        // Começa na tela inicial
        AplicarVisual(0f);

        grupoBotoes.interactable = false;
        grupoBotoes.blocksRaycasts = false;
    }

    // =========================================
    // ANIMAÇÃO PRINCIPAL
    // =========================================

    public IEnumerator Animate(bool irParaMenu)
    {
        if (!inicializado)
            yield break;

        // Bloqueia interação durante a transição
        grupoBotoes.interactable = false;
        grupoBotoes.blocksRaycasts = false;

        float inicio = progresso;
        float destino = irParaMenu ? 1f : 0f;

        float tempo = 0f;
        float tempoTotal = Mathf.Max(0.01f, duracao);

        while (tempo < tempoTotal)
        {
            tempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tempo / tempoTotal);

            // Aceleração e desaceleração suaves
            t = Mathf.SmoothStep(0f, 1f, t);

            progresso = Mathf.Lerp(inicio, destino, t);

            AplicarVisual(progresso);

            yield return null;
        }

        progresso = destino;
        AplicarVisual(progresso);

        // Libera botões somente quando o menu
        // estiver totalmente aberto
        grupoBotoes.interactable = irParaMenu;
        grupoBotoes.blocksRaycasts = irParaMenu;
    }

    // =========================================
    // MOVIMENTO DOS ELEMENTOS
    // =========================================

    private void AplicarVisual(float valor)
    {
        // -------------------------------------
        // 1. POSIÇÃO DA LOGO
        // -------------------------------------

        float logoT = Mathf.SmoothStep(0f, 1f, valor);

        logo.anchoredPosition = Vector2.Lerp(
            posLogoInicio.anchoredPosition,
            posLogoMenu.anchoredPosition,
            logoT
        );

        // -------------------------------------
        // 2. TAMANHO DA LOGO
        // -------------------------------------

        /* float escala = Mathf.Lerp(
            escalaInicio,
            escalaMenu,
            logoT
        );
        */

        //logo.localScale = escalaOriginalLogo * escala;

        // -------------------------------------
        // 3. ENTRADA DO PAINEL
        // -------------------------------------

        float painelT = Mathf.Clamp01(
            (valor - atrasoBotoes) /
            Mathf.Max(0.01f, 1f - atrasoBotoes)
        );

        painelT = Mathf.SmoothStep(0f, 1f, painelT);

        Vector2 posicaoFora =
            posicaoOriginalBotoes +
            Vector2.up * distanciaEntrada;

        painelBotoes.anchoredPosition = Vector2.Lerp(
            posicaoFora,
            posicaoOriginalBotoes,
            painelT
        );

        // O conjunto inteiro aparece gradualmente
        grupoBotoes.alpha = painelT;

        // -------------------------------------
        // 4. TEXTO COMEÇAR
        // -------------------------------------

        float textoT = Mathf.SmoothStep(
            0f,
            1f,
            Mathf.Clamp01(valor / 0.35f)
        );

        grupoComecar.alpha = 1f - textoT;
    }
}
