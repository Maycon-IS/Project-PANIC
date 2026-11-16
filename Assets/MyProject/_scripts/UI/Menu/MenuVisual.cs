
using System.Collections;
using UnityEngine;

public class MenuVisual : MonoBehaviour
{
    [Header("Texto Pulsando")]
    [SerializeField] private GameObject textStart;
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float pulseSpeed = 1.5f;

    [Header("Transicao Visual")]
    [SerializeField] private MenuTransitionUI menuTransition;

    private Vector3 baseScale;
    private bool inicializado = false;

    // =====================================
    // INICIALIZACAO
    // =====================================

    public void Initialize()
    {
        if (textStart != null)
        {
            baseScale = textStart.transform.localScale;
        }
        else
        {
            Debug.LogWarning(
                "MenuVisual: Text Start nao configurado!"
            );
        }

        if (menuTransition != null)
        {
            menuTransition.Initialize();
        }
        else
        {
            Debug.LogWarning(
                "MenuVisual: Menu Transition nao configurado!"
            );
        }

        inicializado = true;
    }

    private void Update()
    {
        if (!inicializado)
            return;

        PulseText();
    }

    // =====================================
    // ANIMACAO DO TEXTO
    // =====================================

    private void PulseText()
    {
        if (textStart == null ||
            !textStart.activeInHierarchy)
        {
            return;
        }

        float pulse =
            (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f;

        pulse = Mathf.SmoothStep(0f, 1f, pulse);

        float scaleValue =
            Mathf.Lerp(minScale, maxScale, pulse);

        textStart.transform.localScale =
            baseScale * scaleValue;
    }

    // =====================================
    // TRANSICAO DE IDA E VOLTA
    // =====================================

    public IEnumerator Animate(bool irParaMenu)
    {
        if (!inicializado)
            yield break;

        if (menuTransition != null)
        {
            yield return menuTransition.Animate(irParaMenu);
        }
    }
}
