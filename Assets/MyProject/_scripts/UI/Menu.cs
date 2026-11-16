using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Dados para a Cena de Gameplay: ")]
    [SerializeField] private string sceneNamePlayOff;
    [SerializeField] private string sceneNamePlayOn;

    [Header("Texto Pusalndo: Escala e Velociada")]
    [SerializeField] private GameObject textStart;
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 1.2f;
    public float pulseSpeed = 1.5f;

    [Header("Paineis Prinicipais: ")]
    [SerializeField] private GameObject gameObjetPE;
    [SerializeField] private GameObject gameObjectPS;
    [SerializeField] private GameObject panelPlay;
    [SerializeField] private GameObject panelSettings;
    [SerializeField] private GameObject panelCretids;


    private readonly Dictionary<RectTransform, Vector3> buttons = new Dictionary<RectTransform, Vector3>();
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private PointerEventData pointerData;
    private Vector3 baseScale;
    private bool canPress = true;


    private void Awake()
    {
        if (textStart != null)
            baseScale = textStart.transform.localScale;
    }

    void Start()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        InitializationPaneis();
    }

    void Update()
    {

        if (canPress)
        {
            InputCanPress();
            PulseText();
        }    
        
    }

    public void btPlay()
    {
        gameObjectPS.SetActive(false);
        panelPlay.SetActive(true);
    }

    public void btPlayOff() { SceneManager.LoadScene(sceneNamePlayOff); }
    public void btPlayOn() { SceneManager.LoadScene(sceneNamePlayOn); }

    public void btSettings()
    {
        gameObjectPS.SetActive(false);
        panelSettings.SetActive(true);
    }

    public void btCredits()
    {
        gameObjectPS.SetActive(false);
        panelCretids.SetActive(true);
    }

    public void btReturn()
    {
        panelPlay.SetActive(false);
        panelCretids.SetActive(false);
        panelSettings.SetActive(false);

        gameObjectPS.SetActive(true);
    }

    public void btExit()
    {
        Application.Quit();
        Debug.Log("Saida");
    }

    private void InputCanPress()
    {
        if (Input.anyKeyDown)
        {
            canPress = false;
            gameObjetPE.SetActive(false);
            gameObjectPS.SetActive(true);
        }
    }

    private void PulseText() 
    {
        if (textStart == null || !textStart.activeInHierarchy)
            return;

        float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f;
        pulse = Mathf.SmoothStep(0f, 1f, pulse);

        float scaleValue = Mathf.Lerp(minScale, maxScale, pulse);
        textStart.transform.localScale = baseScale * scaleValue;
    }

    private void InitializationPaneis() 
    {
        gameObjetPE.SetActive(true);
        gameObjectPS.SetActive(false);
    }
}
