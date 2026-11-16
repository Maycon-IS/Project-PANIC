using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonRegister : MonoBehaviour
{
    [Header("Button Animação: Configurações")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private string buttonTag = "AniamatterButton";
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float animationSpeed = 10f;

    private readonly Dictionary<RectTransform, Vector3> buttons = new Dictionary<RectTransform, Vector3>();
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    [Header("Animação da Cor do Texto")]
    [SerializeField] private Color hoverTextColor = new Color(0.12f, 0.24f, 0.44f);
    [SerializeField] private float textColorSpeed = 10f;

    private readonly Dictionary<RectTransform, TMP_Text> buttonTexts = new Dictionary<RectTransform, TMP_Text>();
    private readonly Dictionary<RectTransform, Color> originalTextColors = new Dictionary<RectTransform, Color>();

    private PointerEventData pointerData;

    private void Start()
    {
        if (EventSystem.current != null)
            pointerData = new PointerEventData(EventSystem.current);

        RegisterButtons();
    }

    private void Update()
    {
        RectEventSystem();
    }

    // REGISTRO DOS BOTOES
    public void RegisterButtons()
    {
        if (menuCanvas == null)
        {
            Debug.LogWarning("Canvas não configurado no Menu!");
            return;
        }

        // Restaura os valores antes de registrar novamente
        foreach (var item in buttons)
        {
            if (item.Key != null)
                item.Key.localScale = item.Value;
        }

        foreach (var item in buttonTexts)
        {
            if (item.Value != null &&
                originalTextColors.TryGetValue(item.Key, out Color color))
            {
                item.Value.color = color;
            }
        }

        // Limpa os registros anteriores
        buttons.Clear();
        buttonTexts.Clear();
        originalTextColors.Clear();

        // Procura botões, mesmo em painéis desativados
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

            // Registra o botão e seu tamanho original
            buttons.Add(rect, rect.localScale);

            // Procura automaticamente o texto dentro do botão
            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(true);

            if (text != null)
            {
                // Registra o texto e sua cor original
                buttonTexts.Add(rect, text);
                originalTextColors.Add(rect, text.color);
            }
        }

        Debug.Log("Botões registrados: " + buttons.Count);
    }


    // ANIMACAO DE HOVER
    private void RectEventSystem()
    {
        if (EventSystem.current == null || pointerData == null)
            return;

        RectTransform hoveredButton = GetHoveredButton();

        float smooth = 1f - Mathf.Exp(
            -animationSpeed * Time.unscaledDeltaTime
        );

        foreach (var item in buttons)
        {
            RectTransform rect = item.Key;

            if (rect == null)
                continue;

            Vector3 originalScale = item.Value;

            // Botões desativados mantêm tamanho original
            if (!rect.gameObject.activeInHierarchy)
            {
                rect.localScale = originalScale;
                continue;
            }

            Vector3 targetScale =
                rect == hoveredButton
                ? originalScale * hoverScale
                : originalScale;

            rect.localScale = Vector3.Lerp(
                rect.localScale,
                targetScale,
                smooth
            );

            if (buttonTexts.TryGetValue(rect, out TMP_Text text))
            {
                if (text != null)
                {
                    Color normalColor = originalTextColors[rect];

                    Color targetColor =
                        rect == hoveredButton
                        ? hoverTextColor
                        : normalColor;

                    if (!rect.gameObject.activeInHierarchy)
                        targetColor = normalColor;

                    float colorSmooth = 1f - Mathf.Exp(
                        -textColorSpeed * Time.unscaledDeltaTime
                    );

                    text.color = Color.Lerp(
                        text.color,
                        targetColor,
                        colorSmooth
                    );
                }
            }
        }
    }

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

                    if (button != null && button.IsActive() && button.IsInteractable())
                    {
                        return rect;
                    }
                }

                current = current.parent;
            }

            // Respeita o elemento de UI na frente
            break;
        }

        return null;
    }

    private void OnDisable()
    {
        foreach (var item in buttons)
        {
            if (item.Key != null)
                item.Key.localScale = item.Value;
        }
    }
}
