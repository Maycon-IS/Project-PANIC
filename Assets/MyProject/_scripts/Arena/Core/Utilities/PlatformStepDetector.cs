using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlatformStepDetector : MonoBehaviour
{
    public event Action OnPlayerStepped;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
            OnPlayerStepped?.Invoke();
    }

}
