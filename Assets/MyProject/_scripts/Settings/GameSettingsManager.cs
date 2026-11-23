
using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class GameSettingsData
{
    public float musicVolume = 0.8f;
    public float sfxVolume = 0.85f;
    public float rainIntensity = 0.5f;

    public bool showFPS = true;
    public bool onlineEnabled = false;
    public bool showPing = false;

    public int serverRegion = 0;

    public int resolutionWidth = 0;
    public int resolutionHeight = 0;

    [Header("Movimentação")]
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveLeftAlt = KeyCode.LeftArrow;

    public KeyCode moveRight = KeyCode.D;
    public KeyCode moveRightAlt = KeyCode.RightArrow;

    [Header("Pulo")]
    public KeyCode jump = KeyCode.Space;
    public KeyCode jumpAlt = KeyCode.W;
    public KeyCode jumpAlt2 = KeyCode.UpArrow;

    [Header("Dash")]
    public KeyCode dash = KeyCode.LeftShift;
    public KeyCode dashAlt = KeyCode.Mouse0;
    public KeyCode dashAlt2 = KeyCode.Q;
    public KeyCode dashAlt3 = KeyCode.R;

    public GameSettingsData Copy()
    {
        return (GameSettingsData)MemberwiseClone();
    }
}

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Parâmetros expostos")]
    [SerializeField] private string musicParameter = "MusicVolume";
    [SerializeField] private string sfxParameter = "SFXVolume";

    private const string SaveKey = "LastFuseSettings_V2";

    public GameSettingsData Current { get; private set; }

    public event Action OnSettingsApplied;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    private void Start()
    {
        ApplyRuntimeSettings();
    }

    public void Load()
    {
        Current = new GameSettingsData();

        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            JsonUtility.FromJsonOverwrite(json, Current);
        }

        if (Current.resolutionWidth <= 0 ||
            Current.resolutionHeight <= 0)
        {
            Current.resolutionWidth = Screen.width;
            Current.resolutionHeight = Screen.height;
        }
    }

    public void Apply(GameSettingsData newSettings)
    {
        if (newSettings == null)
            return;

        Current = newSettings.Copy();

        Save();
        ApplyRuntimeSettings();

        OnSettingsApplied?.Invoke();
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(Current);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void ApplyRuntimeSettings()
    {
        ApplyAudio();

        if (Current.resolutionWidth > 0 &&
            Current.resolutionHeight > 0 &&
            !Application.isEditor)
        {
            Screen.SetResolution(
                Current.resolutionWidth,
                Current.resolutionHeight,
                Screen.fullScreenMode
            );
        }
    }

    private void ApplyAudio()
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("AudioMixer não configurado.");
            return;
        }

        audioMixer.SetFloat(
            musicParameter,
            VolumeToDecibels(Current.musicVolume)
        );

        audioMixer.SetFloat(
            sfxParameter,
            VolumeToDecibels(Current.sfxVolume)
        );
    }

    private float VolumeToDecibels(float volume)
    {
        return Mathf.Log10(
            Mathf.Max(0.0001f, volume)
        ) * 20f;
    }
}
