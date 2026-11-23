using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIRain : MaskableGraphic
{
    [Header("Configuracoes da Chuva")]
    [SerializeField, Range(10, 300)]
    private int quantidadeGotas = 120;

    [Header("Controle de Intensidade")]
    [SerializeField, Range(0f, 1f)]
    private float intensidade = 1f;

    private int quantidadeOriginal;
    private float transparenciaOriginal;

    private float intensidadeAplicada = -1f;

    [SerializeField] private float velocidadeMinima = 350f;
    [SerializeField] private float velocidadeMaxima = 650f;

    [SerializeField] private float tamanhoMinimo = 12f;
    [SerializeField] private float tamanhoMaximo = 30f;

    [SerializeField] private float espessura = 2f;

    [Header("Aparencia")]
    [SerializeField, Range(0f, 1f)]
    private float transparencia = 0.45f;

    [SerializeField] private float inclinacao = 5f;

    private struct Gota
    {
        public float x;
        public float y;
        public float velocidade;
        public float tamanho;
        public float alpha;
    }

    private Gota[] gotas;
    private RectTransform meuRect;

    protected override void Awake()
    {
        base.Awake();

        meuRect = GetComponent<RectTransform>();
        raycastTarget = false;

        color = new Color(0.7f, 0.85f, 1f, 1f);

        quantidadeOriginal = quantidadeGotas;
        transparenciaOriginal = transparencia;
    }

    protected override void Start()
    {
        base.Start();

        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.OnSettingsApplied -=
                ApplySavedRainSettings;

            GameSettingsManager.Instance.OnSettingsApplied +=
                ApplySavedRainSettings;

            ApplySavedRainSettings();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InicializarGotas();

        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.OnSettingsApplied +=
                ApplySavedRainSettings;

            ApplySavedRainSettings();
        }
    }

    protected override void OnDisable()
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.OnSettingsApplied -=
                ApplySavedRainSettings;
        }

        base.OnDisable();
    }

    private void ApplySavedRainSettings()
    {
        if (GameSettingsManager.Instance == null)
            return;

        float savedIntensity =
            GameSettingsManager.Instance.Current.rainIntensity;

        SetIntensity(savedIntensity);
    }

    private void InicializarGotas()
    {
        if (meuRect == null)
            meuRect = GetComponent<RectTransform>();

        Rect area = meuRect.rect;

        gotas = new Gota[quantidadeGotas];

        for (int i = 0; i < gotas.Length; i++)
        {
            gotas[i] = CriarGota(
                Random.Range(area.yMin, area.yMax)
            );
        }

        SetVerticesDirty();
    }

    public void SetIntensity(float value)
    {
        intensidade = Mathf.Clamp01(value);

        if (Mathf.Approximately(intensidade, intensidadeAplicada))
            return;

        intensidadeAplicada = intensidade;

        // Calcula quantas gotas devem aparecer.
        quantidadeGotas = Mathf.RoundToInt(
            Mathf.Lerp(0f, quantidadeOriginal, intensidade)
        );

        // Controla a transparência.
        transparencia = Mathf.Lerp(
            0f,
            transparenciaOriginal,
            intensidade
        );

        // Atualiza a quantidade de gotas.
        InicializarGotas();
    }

    private Gota CriarGota(float posicaoY)
    {
        Rect area = meuRect.rect;

        // Espaço adicional nas laterais para a chuva inclinada
        float margem = area.height *
            Mathf.Abs(inclinacao) /
            Mathf.Max(1f, tamanhoMinimo) + 100f;

        return new Gota
        {
            x = Random.Range(
                area.xMin - margem,
                area.xMax + margem
            ),

            y = posicaoY,

            velocidade = Random.Range(
                velocidadeMinima,
                velocidadeMaxima
            ),

            tamanho = Random.Range(
                tamanhoMinimo,
                tamanhoMaximo
            ),

            alpha = Random.Range(0.5f, 1f)
        };
    }

    private void Update()
    {
        if (quantidadeGotas <= 0)
        {
            if (gotas == null || gotas.Length != 0)
            {
                gotas = new Gota[0];
                SetVerticesDirty();
            }

            return;
        }

        if (gotas == null || gotas.Length != quantidadeGotas)
        {
            InicializarGotas();
        }

        Rect area = meuRect.rect;
        float delta = Time.unscaledDeltaTime;

        for (int i = 0; i < gotas.Length; i++)
        {
            Gota gota = gotas[i];

            // Movimento vertical
            float movimentoY = gota.velocidade * delta;

            // Movimento horizontal proporcional à inclinação
            float movimentoX = (inclinacao / gota.tamanho) * movimentoY;

            // Aplica o movimento diagonal
            gota.x += movimentoX;
            gota.y -= movimentoY;

            // Reinicia a gota quando sair da área visível
            // Reinicia somente quando a gota passa da parte inferior
            if (gota.y < area.yMin - gota.tamanho)
            {
                gota = CriarGota(
                    area.yMax + Random.Range(0f, 100f)
                );
            }

            gotas[i] = gota;
        }

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (gotas == null)
            return;

        for (int i = 0; i < gotas.Length; i++)
        {
            Gota gota = gotas[i];

            Color cor = color;
            cor.a *= transparencia * gota.alpha;

            Vector2 topo = new Vector2(gota.x, gota.y);
            Vector2 baixo = new Vector2(
                gota.x + inclinacao,
                gota.y - gota.tamanho
            );

            Vector2 lado = new Vector2(espessura / 2f, 0f);

            int indice = vh.currentVertCount;

            UIVertex vertice = UIVertex.simpleVert;
            vertice.color = cor;

            vertice.position = topo - lado;
            vh.AddVert(vertice);

            vertice.position = topo + lado;
            vh.AddVert(vertice);

            vertice.position = baixo + lado;
            vh.AddVert(vertice);

            vertice.position = baixo - lado;
            vh.AddVert(vertice);

            vh.AddTriangle(indice, indice + 1, indice + 2);
            vh.AddTriangle(indice, indice + 2, indice + 3);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        if (isActiveAndEnabled && meuRect != null)
            InicializarGotas();
    }
}
