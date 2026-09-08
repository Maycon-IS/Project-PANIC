using TMPro;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine;

public class MainGameView : MonoBehaviour
{
    public static MainGameView Instance { get; private set; } 

    [Header("Configuracao do Tempo da Bomba")]
    [SerializeField] private TMP_Text timeBombText;
    [SerializeField] private TMP_Text timeDelayBombText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateTimeBomb(float timeBomb)
    {
        timeBombText.text = "Time da Bomba: " + 
            timeBomb.ToString("F1" , CultureInfo.InvariantCulture);
    }

    public void UpdateDelayBomb(float time)
    {
        timeDelayBombText.enabled = (time > 0);

        if (time > 0)
        {
            timeDelayBombText.text = "Proxima bomba em: " +
                time.ToString("F1", CultureInfo.InvariantCulture);
        }

    }
}
