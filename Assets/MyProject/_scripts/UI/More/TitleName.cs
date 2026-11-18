using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleName : MonoBehaviour
{

    [Header("Texto do Botão")]
    [SerializeField] private TMP_Text buttonText;

    [Header("Texto por Plataforma")]
    [SerializeField] private string desktopText = "SAIR";
    [SerializeField] private string webGLText = "VOLTAR À PÁGINA DO JOGO";

    private void Start()
    {
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);
        }

        if (buttonText == null)
        {
            Debug.LogWarning("Texto do botão não encontrado!");
            return;
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            buttonText.text = webGLText;
        }
        else
        {
            buttonText.text = desktopText;
        }
    }
}