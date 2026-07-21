using UnityEngine;

public class PlayerElimination : MonoBehaviour
{
    public void Eliminate()
    {
        // TODO: partícula de explosão, som, shake de câmera (conforme seu GDD > Feedback Visual)
        gameObject.SetActive(false);
    }
}