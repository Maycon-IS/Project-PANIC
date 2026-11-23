using UnityEngine;
using System;

public class PlayerElimination : MonoBehaviour
{

    public static event Action<GameObject> OnPlayerEliminated;

    public bool IsDead { get; private set; }

    private void OnEnable()
    {
        IsDead = false;
    }

    public void Kill()
    {
        if (IsDead)
            return;

        IsDead = true;

        OnPlayerEliminated?.Invoke(gameObject);

        gameObject.SetActive(false);
    }
}