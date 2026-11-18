using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VoidKillZone : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerElimination>(out var elimination))
        {
            elimination.Kill();
        }
    }
}