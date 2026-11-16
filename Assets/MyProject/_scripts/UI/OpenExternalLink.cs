using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenExternalLink : MonoBehaviour
{
    [SerializeField] private string url;

    public void OpenLink()
    {
        if (!string.IsNullOrEmpty(url))
        {
            Application.OpenURL(url);
        }
    }
}