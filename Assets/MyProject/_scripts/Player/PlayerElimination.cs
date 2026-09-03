using UnityEngine;
using System;

public class PlayerElimination : MonoBehaviour
{

    public static event Action<GameObject> OnPlayerEliminated;

    public void Kill()
    {
        gameObject.SetActive(false);
        OnPlayerEliminated?.Invoke(gameObject);
    }
}