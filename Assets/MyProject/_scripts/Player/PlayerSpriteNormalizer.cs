using UnityEngine;
using System.Collections.Generic;

public class PlayerSpriteNormalizer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite referenceSprite;

    [Header("Normalizacao Automatica")]
    [SerializeField] private bool automaticNormalization = true;

    [Range(0f, 1f)]
    [SerializeField] private float normalizationStrength = 1f;

    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;

    [Range(0.01f, 1f)]
    [SerializeField] private float alphaThreshold = 0.1f;

    [Header("Excecoes")]
    [SerializeField] private bool normalizeDash = false;
    [SerializeField] private bool normalizeDeath = false;

    // Armazena os tamanhos ja calculados.
    private readonly Dictionary<Sprite, float> heightCache =
        new Dictionary<Sprite, float>();

    private float referenceHeight;
    private bool initialized;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        if (referenceSprite == null && spriteRenderer != null)
            referenceSprite = spriteRenderer.sprite;

        if (referenceSprite == null)
        {
            Debug.LogError(
                "Normalizer: configure o Reference Sprite.", this);
            return;
        }

        referenceHeight = GetVisibleHeight(referenceSprite);
    }

    public float GetMultiplier(string animationName)
    {
        if (!automaticNormalization || spriteRenderer == null)
            return 1f;

        Initialize();

        if (referenceHeight <= 0f)
            return 1f;

        if (animationName == "Dash" && !normalizeDash)
            return 1f;

        if (animationName == "Death" && !normalizeDeath)
            return 1f;

        Sprite currentSprite = spriteRenderer.sprite;

        if (currentSprite == null)
            return 1f;

        float currentHeight = GetVisibleHeight(currentSprite);

        if (currentHeight <= 0f)
            return 1f;

        float correction = referenceHeight / currentHeight;

        correction = Mathf.Clamp(
            correction, minScale, maxScale);

        return Mathf.Lerp(
            1f, correction, normalizationStrength);
    }

    private float GetVisibleHeight(Sprite sprite)
    {
        if (heightCache.TryGetValue(sprite, out float cached))
            return cached;

        Texture2D texture = sprite.texture;

        if (!texture.isReadable || sprite.packed)
        {
            Debug.LogWarning(
                "Normalizer: textura precisa ser legivel " +
                "e o sprite nao pode estar empacotado.", this);

            heightCache[sprite] = 0f;
            return 0f;
        }

        Rect rect = sprite.rect;

        int x = Mathf.RoundToInt(rect.x);
        int y = Mathf.RoundToInt(rect.y);
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);

        Color[] pixels = texture.GetPixels(
            x, y, width, height);

        int minY = height;
        int maxY = -1;

        for (int py = 0; py < height; py++)
        {
            for (int px = 0; px < width; px++)
            {
                Color pixel = pixels[py * width + px];

                if (pixel.a < alphaThreshold)
                    continue;

                minY = Mathf.Min(minY, py);
                maxY = Mathf.Max(maxY, py);
            }
        }

        if (maxY < minY)
        {
            heightCache[sprite] = 0f;
            return 0f;
        }

        float visibleHeight = maxY - minY + 1;

        // Converte pixels para unidades da Unity.
        float worldHeight =
            visibleHeight / sprite.pixelsPerUnit;

        heightCache[sprite] = worldHeight;

        return worldHeight;
    }

    public void Recalculate()
    {
        heightCache.Clear();
        initialized = false;
        referenceHeight = 0f;

        Initialize();
    }
}