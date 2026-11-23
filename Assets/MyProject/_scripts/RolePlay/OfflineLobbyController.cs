
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OfflineLobbyController : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject gameplayHUD;

    [Header("Gerenciador de Spawn")]
    [SerializeField] private MatchSpawner matchSpawner;

    [Header("Quantidade de IAs")]
    [SerializeField] private int minAI = 2;
    [SerializeField] private int maxAI = 8;
    [SerializeField] private TMP_Text aiAmountText;

    [Header("Seleção de Skin")]
    [SerializeField] private List<CharacterSkin> skins;
    [SerializeField] private Image characterPreview;
    [SerializeField] private TMP_Text skinNameText;

    [Header("Botões")]
    [SerializeField] private Button startGameButton;

    [Header("Cena do Menu")]
    [SerializeField] private string menuScene = "Menu";

    private int aiAmount;
    private int selectedSkinIndex;

    private bool isStarting;

    private void Start()
    {
        Time.timeScale = 1f;

        aiAmount = Mathf.Clamp(4, minAI, maxAI);
        selectedSkinIndex = 0;

        isStarting = false;

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(false);

        UpdateAIText();
        UpdateSkinPreview();
    }

    public void IncreaseAI()
    {
        if (isStarting) return;

        aiAmount = Mathf.Clamp(
            aiAmount + 1,
            minAI,
            maxAI
        );

        UpdateAIText();
    }

    public void DecreaseAI()
    {
        if (isStarting) return;

        aiAmount = Mathf.Clamp(
            aiAmount - 1,
            minAI,
            maxAI
        );

        UpdateAIText();
    }

    public void NextSkin()
    {
        if (isStarting || skins.Count == 0)
            return;

        selectedSkinIndex =
            (selectedSkinIndex + 1) % skins.Count;

        UpdateSkinPreview();
    }

    public void PreviousSkin()
    {
        if (isStarting || skins.Count == 0)
            return;

        selectedSkinIndex =
            (selectedSkinIndex - 1 + skins.Count)
            % skins.Count;

        UpdateSkinPreview();
    }

    private void UpdateAIText()
    {
        if (aiAmountText != null)
            aiAmountText.text = aiAmount.ToString();
    }

    private void UpdateSkinPreview()
    {
        if (skins.Count == 0)
            return;

        CharacterSkin skin = skins[selectedSkinIndex];

        if (skin == null)
            return;

        if (characterPreview != null)
        {
            characterPreview.sprite = skin.characterSprite;
            characterPreview.color = skin.characterColor;
        }

        if (skinNameText != null)
            skinNameText.text = skin.skinName;
    }

    public void StartGame()
    {
        if (isStarting)
            return;

        if (skins.Count == 0 ||
            selectedSkinIndex >= skins.Count)
        {
            Debug.LogError("Nenhuma skin configurada.");
            return;
        }

        CharacterSkin selectedSkin =
            skins[selectedSkinIndex];

        if (selectedSkin == null)
        {
            Debug.LogError("Skin inválida.");
            return;
        }

        if (matchSpawner == null)
        {
            Debug.LogError("MatchSpawner não configurado.");
            return;
        }

        bool started = matchSpawner.BeginMatch(
            aiAmount,
            selectedSkin,
            HandleMatchInitialized
        );

        if (started)
        {
            isStarting = true;

            if (startGameButton != null)
                startGameButton.interactable = false;
        }
    }

    private void HandleMatchInitialized(bool success)
    {
        isStarting = false;

        if (!success)
        {
            Debug.LogError(
                "Não foi possível iniciar a partida."
            );

            if (startGameButton != null)
                startGameButton.interactable = true;

            return;
        }

        if (gameplayHUD != null)
            gameplayHUD.SetActive(true);

        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        if (isStarting)
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }
}
