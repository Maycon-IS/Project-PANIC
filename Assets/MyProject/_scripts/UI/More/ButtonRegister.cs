
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonRegister : MonoBehaviour
{
    [Header("Configuracoes dos Botoes")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private string buttonTag = "AniamatterButton";
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Animacao da Cor do Texto")]
    [SerializeField]
    private Color hoverTextColor =
        new Color(0.12f, 0.24f, 0.44f);

    [SerializeField] private float textColorSpeed = 10f;

    private readonly Dictionary<RectTransform, Vector3> buttons =
        new Dictionary<RectTransform, Vector3>();

    private readonly Dictionary<RectTransform, TMP_Text> buttonTexts =
        new Dictionary<RectTransform, TMP_Text>();

    private readonly Dictionary<RectTransform, Color> originalTextColors =
        new Dictionary<RectTransform, Color>();

    private readonly List<RaycastResult> raycastResults =
        new List<RaycastResult>();

    private PointerEventData pointerData;

    // =====================================
    // INICIALIZACAO
    // =====================================

    private void Start()
    {
        RegisterButtons();
    }

    private void Update()
    {
        RectEventSystem();
    }

    // =====================================
    // REGISTRO DOS BOTOES
    // =====================================

    public void RegisterButtons()
    {
        if (menuCanvas == null)
        {
            Debug.LogWarning(
                "ButtonRegister: Canvas nao configurado!"
            );
            return;
        }

        RestaurarBotoes();

        buttons.Clear();
        buttonTexts.Clear();
        originalTextColors.Clear();

        // Procura inclusive em paineis desativados
        Button[] foundButtons =
            menuCanvas.GetComponentsInChildren<Button>(true);

        foreach (Button button in foundButtons)
        {
            if (!button.CompareTag(buttonTag))
                continue;

            RectTransform rect =
                button.GetComponent<RectTransform>();

            if (rect == null)
                continue;

            buttons.Add(rect, rect.localScale);

            TMP_Text texto =
                button.GetComponentInChildren<TMP_Text>(true);

            if (texto != null)
            {
                buttonTexts.Add(rect, texto);
                originalTextColors.Add(rect, texto.color);
            }
        }

        Debug.Log("Botoes registrados: " + buttons.Count);
    }

    // =====================================
    // ANIMACAO DE HOVER
    // =====================================

    private void RectEventSystem()
    {
        if (EventSystem.current == null)
            return;

        if (pointerData == null)
            pointerData = new PointerEventData(EventSystem.current);

        RectTransform hoveredButton = GetHoveredButton();

        float smooth = 1f - Mathf.Exp(
            -animationSpeed * Time.unscaledDeltaTime
        );

        float colorSmooth = 1f - Mathf.Exp(
            -textColorSpeed * Time.unscaledDeltaTime
        );

        foreach (var item in buttons)
        {
            RectTransform rect = item.Key;

            if (rect == null)
                continue;

            Vector3 originalScale = item.Value;

            bool ativo = rect.gameObject.activeInHierarchy;
            bool hovered = ativo && rect == hoveredButton;

            Vector3 targetScale = hovered
                ? originalScale * hoverScale
                : originalScale;

            // Restaura imediatamente os paineis desativados
            rect.localScale = ativo
                ? Vector3.Lerp(rect.localScale, targetScale, smooth)
                : originalScale;

            if (buttonTexts.TryGetValue(rect, out TMP_Text texto)
                && texto != null
                && originalTextColors.TryGetValue(
                    rect, out Color normalColor))
            {
                Color targetColor = hovered
                    ? hoverTextColor
                    : normalColor;

                texto.color = ativo
                    ? Color.Lerp(texto.color, targetColor, colorSmooth)
                    : normalColor;
            }
        }
    }

    // =====================================
    // DETECCAO DO MOUSE
    // =====================================

    private RectTransform GetHoveredButton()
    {
        pointerData.position = Input.mousePosition;
        raycastResults.Clear();

        EventSystem.current.RaycastAll(
            pointerData,
            raycastResults
        );

        foreach (RaycastResult result in raycastResults)
        {
            Transform current = result.gameObject.transform;

            while (current != null)
            {
                RectTransform rect =
                    current.GetComponent<RectTransform>();

                if (rect != null && buttons.ContainsKey(rect))
                {
                    Button button = current.GetComponent<Button>();

                    if (button != null &&
                        button.isActiveAndEnabled &&
                        button.IsInteractable())
                    {
                        return rect;
                    }
                }

                current = current.parent;
            }

            // Respeita o elemento na frente
            break;
        }

        return null;
    }

    // =====================================
    // RESTAURAR VALORES ORIGINAIS
    // =====================================

    private void RestaurarBotoes()
    {
        foreach (var item in buttons)
        {
            if (item.Key != null)
                item.Key.localScale = item.Value;
        }

        foreach (var item in buttonTexts)
        {
            if (item.Value != null &&
                originalTextColors.TryGetValue(
                    item.Key, out Color normalColor))
            {
                item.Value.color = normalColor;
            }
        }
    }

    private void OnDisable()
    {
        RestaurarBotoes();
    }
}
