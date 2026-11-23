
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleSwitch : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private RectTransform checkmark;
    [SerializeField] private Image background;

    [Header("Posições")]
    [SerializeField] private float disPosicao = 34f;

    private float posicaoLigado => disPosicao;
    private float posicaoDesligado => -1 * disPosicao;

    [Header("Cores")]
    [SerializeField] private Color corLigado = Color.yellow;
    [SerializeField] private Color corDesligado = Color.gray;

    [Header("Animação")]
    [SerializeField] private float velocidade = 12f;

    private Toggle toggle;
    private float posicaoAlvo;
    private Color corAlvo;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();

        toggle.onValueChanged.AddListener(AtualizarEstado);

        AtualizarEstado(toggle.isOn);

        // Define o estado inicial sem animação
        checkmark.anchoredPosition = new Vector2(
            posicaoAlvo, 0f
        );

        background.color = corAlvo;
    }

    private void Update()
    {
        if (checkmark == null || background == null)
            return;

        float fator = 1f - Mathf.Exp(
            -velocidade * Time.unscaledDeltaTime
        );

        Vector2 destino = new Vector2(posicaoAlvo, 0f);

        checkmark.anchoredPosition = Vector2.Lerp(
            checkmark.anchoredPosition,
            destino,
            fator
        );

        background.color = Color.Lerp(
            background.color,
            corAlvo,
            fator
        );
    }

    private void AtualizarEstado(bool ligado)
    {
        posicaoAlvo = ligado
            ? posicaoLigado
            : posicaoDesligado;

        corAlvo = ligado
            ? corLigado
            : corDesligado;
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(AtualizarEstado);
    }
}
