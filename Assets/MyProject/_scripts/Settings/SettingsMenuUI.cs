
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Áudio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("Vídeo")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle showFPSToggle;

    [Header("Chuva")]
    [SerializeField] private Slider rainSlider;
    [SerializeField] private TMP_Text rainValueText;

    [Header("Online")]
    [SerializeField] private Toggle onlineToggle;
    [SerializeField] private Toggle showPingToggle;
    [SerializeField] private TMP_Dropdown regionDropdown;

    [Header("Controles")]
    [SerializeField] private TMP_Text moveLeftText;
    [SerializeField] private TMP_Text moveRightText;
    [SerializeField] private TMP_Text jumpText;
    [SerializeField] private TMP_Text dashText;

    [Header("Remapeamento")]
    [SerializeField] private GameObject rebindPanel;
    [SerializeField] private TMP_Text rebindInstructionText;

    private GameSettingsData pending;
    private readonly List<Vector2Int> resolutions = new List<Vector2Int>();

    private enum RebindAction
    {
        None,

        Left,
        LeftAlt,

        Right,
        RightAlt,

        Jump,
        JumpAlt,
        JumpAlt2,

        Dash,
        DashAlt,
        DashAlt2,
        DashAlt3
    }

    private RebindAction waitingFor = RebindAction.None;
    private int rebindStartFrame;

    private void OnEnable()
    {
        if (GameSettingsManager.Instance != null)
            InitializeMenu();
    }

    private void Start()
    {
        if (pending == null &&
            GameSettingsManager.Instance != null)
        {
            InitializeMenu();
        }
    }

    private void InitializeMenu()
    {
        pending = GameSettingsManager.Instance.Current.Copy();

        waitingFor = RebindAction.None;

        if (rebindPanel != null)
            rebindPanel.SetActive(false);

        SetupResolutions();
        SetupRegions();
        UpdateUI();
    }

    private void SetupResolutions()
    {
        resolutions.Clear();

        if (resolutionDropdown == null)
            return;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (Resolution res in Screen.resolutions)
        {
            Vector2Int size =
                new Vector2Int(res.width, res.height);

            if (resolutions.Contains(size))
                continue;

            resolutions.Add(size);
            options.Add($"{res.width} x {res.height}");
        }

        Vector2Int saved = new Vector2Int(
            pending.resolutionWidth,
            pending.resolutionHeight
        );

        if (!resolutions.Contains(saved))
        {
            resolutions.Add(saved);
            options.Add($"{saved.x} x {saved.y}");
        }

        resolutionDropdown.AddOptions(options);
    }

    private void SetupRegions()
    {
        if (regionDropdown == null)
            return;

        regionDropdown.ClearOptions();

        regionDropdown.AddOptions(
            new List<string>
            {
                "América do Sul",
                "América do Norte",
                "Europa",
                "Ásia"
            }
        );
    }

    private void UpdateUI()
    {
        if (pending == null)
            return;

        musicSlider?.SetValueWithoutNotify(
            pending.musicVolume
        );

        sfxSlider?.SetValueWithoutNotify(
            pending.sfxVolume
        );

        rainSlider?.SetValueWithoutNotify(
            pending.rainIntensity
        );

        showFPSToggle?.SetIsOnWithoutNotify(
            pending.showFPS
        );

        onlineToggle?.SetIsOnWithoutNotify(
            pending.onlineEnabled
        );

        showPingToggle?.SetIsOnWithoutNotify(
            pending.showPing
        );

        if (regionDropdown != null)
        {
            int region = Mathf.Clamp(
                pending.serverRegion,
                0,
                regionDropdown.options.Count - 1
            );

            regionDropdown.SetValueWithoutNotify(region);
            regionDropdown.RefreshShownValue();
        }

        if (resolutionDropdown != null)
        {
            Vector2Int selected = new Vector2Int(
                pending.resolutionWidth,
                pending.resolutionHeight
            );

            int index = resolutions.IndexOf(selected);

            if (index >= 0)
            {
                resolutionDropdown.SetValueWithoutNotify(index);
                resolutionDropdown.RefreshShownValue();
            }
        }

        UpdateValueTexts();
        UpdateControlTexts();
    }

    private void UpdateValueTexts()
    {
        if (musicValueText != null)
            musicValueText.text =
                Mathf.RoundToInt(pending.musicVolume * 100f)
                + "%";

        if (sfxValueText != null)
            sfxValueText.text =
                Mathf.RoundToInt(pending.sfxVolume * 100f)
                + "%";

        if (rainValueText != null)
            rainValueText.text =
                Mathf.RoundToInt(pending.rainIntensity * 100f)
                + "%";
    }

    public void SetMusic(float value)
    {
        pending.musicVolume = value;
        UpdateValueTexts();
    }

    public void SetSFX(float value)
    {
        pending.sfxVolume = value;
        UpdateValueTexts();
    }

    public void SetRain(float value)
    {
        pending.rainIntensity = value;
        UpdateValueTexts();
    }

    public void SetShowFPS(bool value)
    {
        pending.showFPS = value;
    }

    public void SetOnline(bool value)
    {
        pending.onlineEnabled = value;
    }

    public void SetShowPing(bool value)
    {
        pending.showPing = value;
    }

    public void SetRegion(int index)
    {
        pending.serverRegion = index;
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Count)
            return;

        pending.resolutionWidth = resolutions[index].x;
        pending.resolutionHeight = resolutions[index].y;
    }

    public void ApplySettings()
    {
        if (pending == null)
            return;

        GameSettingsManager.Instance.Apply(pending);

        Debug.Log("Configurações aplicadas e salvas.");
    }

    public void DiscardChanges()
    {
        InitializeMenu();
    }

    public void OpenRebindPanel()
    {
        if (rebindPanel != null)
            rebindPanel.SetActive(true);
    }

    public void CloseRebindPanel()
    {
        waitingFor = RebindAction.None;

        if (rebindPanel != null)
            rebindPanel.SetActive(false);
    }

    public void RebindLeft()
    {
        BeginRebind(RebindAction.Left);
    }

    public void RebindRight()
    {
        BeginRebind(RebindAction.Right);
    }

    public void RebindJump()
    {
        BeginRebind(RebindAction.Jump);
    }

    public void RebindDash()
    {
        BeginRebind(RebindAction.Dash);
    }

    public void RebindLeftAlt()
    {
        BeginRebind(RebindAction.LeftAlt);
    }

    public void RebindRightAlt()
    {
        BeginRebind(RebindAction.RightAlt);
    }

    public void RebindJumpAlt()
    {
        BeginRebind(RebindAction.JumpAlt);
    }

    public void RebindJumpAlt2()
    {
        BeginRebind(RebindAction.JumpAlt2);
    }

    public void RebindDashAlt()
    {
        BeginRebind(RebindAction.DashAlt);
    }

    public void RebindDashAlt2()
    {
        BeginRebind(RebindAction.DashAlt2);
    }

    public void RebindDashAlt3()
    {
        BeginRebind(RebindAction.DashAlt3);
    }

    private void BeginRebind(RebindAction action)
    {
        waitingFor = action;
        rebindStartFrame = Time.frameCount;

        if (rebindInstructionText != null)
        {
            rebindInstructionText.text =
                "Pressione uma tecla ou botão do mouse. ESC cancela.";
        }
    }

    private void Update()
    {
        if (waitingFor == RebindAction.None)
            return;

        // Não captura o clique utilizado para abrir a seleção.
        if (Time.frameCount == rebindStartFrame)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelRebind();
            return;
        }

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (!IsAllowedKey(key))
                continue;

            if (!Input.GetKeyDown(key))
                continue;

            if (IsDuplicateKey(key))
            {
                if (rebindInstructionText != null)
                    rebindInstructionText.text =
                        "Tecla já utilizada. Escolha outra.";

                return;
            }

            switch (waitingFor)
            {
                case RebindAction.Left:
                    pending.moveLeft = key;
                    break;

                case RebindAction.LeftAlt:
                    pending.moveLeftAlt = key;
                    break;

                case RebindAction.Right:
                    pending.moveRight = key;
                    break;

                case RebindAction.RightAlt:
                    pending.moveRightAlt = key;
                    break;

                case RebindAction.Jump:
                    pending.jump = key;
                    break;

                case RebindAction.JumpAlt:
                    pending.jumpAlt = key;
                    break;

                case RebindAction.JumpAlt2:
                    pending.jumpAlt2 = key;
                    break;

                case RebindAction.Dash:
                    pending.dash = key;
                    break;

                case RebindAction.DashAlt:
                    pending.dashAlt = key;
                    break;

                case RebindAction.DashAlt2:
                    pending.dashAlt2 = key;
                    break;

                case RebindAction.DashAlt3:
                    pending.dashAlt3 = key;
                    break;
            }

            waitingFor = RebindAction.None;

            UpdateControlTexts();

            if (rebindInstructionText != null)
                rebindInstructionText.text =
                    "Controle alterado. Clique em Aplicar para salvar.";

            return;
        }
    }

    private bool IsAllowedKey(KeyCode key)
    {
        if (key == KeyCode.None ||
            key == KeyCode.Escape)
            return false;

        // Permite teclado e botões principais do mouse.
        if (key >= KeyCode.Mouse0 &&
            key <= KeyCode.Mouse6)
            return true;

        // Ignora entradas de joystick neste sistema.
        if (key >= KeyCode.JoystickButton0)
            return false;

        return true;
    }

    private bool IsDuplicateKey(KeyCode key)
    {
        return
            (waitingFor != RebindAction.Left &&
             pending.moveLeft == key) ||

            (waitingFor != RebindAction.LeftAlt &&
             pending.moveLeftAlt == key) ||

            (waitingFor != RebindAction.Right &&
             pending.moveRight == key) ||

            (waitingFor != RebindAction.RightAlt &&
             pending.moveRightAlt == key) ||

            (waitingFor != RebindAction.Jump &&
             pending.jump == key) ||

            (waitingFor != RebindAction.JumpAlt &&
             pending.jumpAlt == key) ||

            (waitingFor != RebindAction.JumpAlt2 &&
             pending.jumpAlt2 == key) ||

            (waitingFor != RebindAction.Dash &&
             pending.dash == key) ||

            (waitingFor != RebindAction.DashAlt &&
             pending.dashAlt == key) ||

            (waitingFor != RebindAction.DashAlt2 &&
             pending.dashAlt2 == key) ||

            (waitingFor != RebindAction.DashAlt3 &&
             pending.dashAlt3 == key);
    }

    private void CancelRebind()
    {
        waitingFor = RebindAction.None;

        if (rebindInstructionText != null)
            rebindInstructionText.text =
                "Remapeamento cancelado.";
    }

    private void UpdateControlTexts()
    {
        if (moveLeftText != null)
            moveLeftText.text =
                pending.moveLeft + " / " + pending.moveLeftAlt;

        if (moveRightText != null)
            moveRightText.text =
                pending.moveRight + " / " + pending.moveRightAlt;

        if (jumpText != null)
            jumpText.text =
                pending.jump + " / " +
                pending.jumpAlt + " / " +
                pending.jumpAlt2;

        if (dashText != null)
            dashText.text =
                pending.dash + " / " +
                pending.dashAlt + " / " +
                pending.dashAlt2 + " / " +
                pending.dashAlt3;
    }

}
