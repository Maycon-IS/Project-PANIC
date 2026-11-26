using UnityEngine;
using System;
using System.Collections;

public class PlayerElimination : MonoBehaviour
{
    public static event Action<GameObject> OnPlayerEliminated;

    [Header("Eliminacao")]
    [SerializeField] private float deathAnimationTimeout = 3f;

    public bool IsDead { get; private set; }

    private PlayerAnimation playerAnimation;
    private PlayerController playerController;
    private Rigidbody2D rb;

    private bool controllerWasEnabled;
    private bool physicsWasSimulated;

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (IsDead)
        {
            if (playerController != null)
                playerController.enabled = controllerWasEnabled;

            if (rb != null)
                rb.simulated = physicsWasSimulated;
        }

        IsDead = false;
    }

    public void Kill()
    {
        if (IsDead)
            return;

        IsDead = true;

        // Impede a movimentacao
        if (playerController != null)
        {
            controllerWasEnabled = playerController.enabled;
            playerController.enabled = false;
        }

        // Interrompe a fisica e as colisoes
        if (rb != null)
        {
            physicsWasSimulated = rb.simulated;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Reproduz a animacao de morte
        if (playerAnimation != null)
            playerAnimation.PlayDeath();

        // Informa os outros sistemas imediatamente
        OnPlayerEliminated?.Invoke(gameObject);

        // Aguarda a animacao antes de desativar
        if (gameObject.activeInHierarchy)
            StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        if (playerAnimation != null)
        {
            float elapsed = 0f;

            // Permite que o Animator atualize seu estado
            yield return null;

            while (!playerAnimation.IsDeathAnimationFinished()
                   && elapsed < deathAnimationTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        gameObject.SetActive(false);
    }
}