using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Pulo")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;

    [Header("Ground")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 2f;
    
    [SerializeField] private SpriteRenderer sr; 

    private Rigidbody2D rb;
    private IInputProvider inputProvider;

    private float moveInput;
    private bool jumpRequested;
    private bool isGrounded;
    private float defaultGravityScale;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;
    public float HorizontalVelocity => rb.velocity.x;
    public float VerticalVelocity => rb.velocity.y;
    public float DashDuration => dashDuration;


    public float ExternalVelocityX { get; set; }
    public float JumpForceMultiplier { get; set; } = 1f;
    public float GravityMultiplier { get; set; } = 1f;

    private bool dashRequested;
    private bool isDashing;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private int facingDirection = 1; // 1 = direita, -1 = esquerda

    private void Awake()
    {
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>(); 
        
        rb = GetComponent<Rigidbody2D>();

        inputProvider = GetComponent<IInputProvider>();
        
        defaultGravityScale = rb.gravityScale; // guarda o valor original definido no Inspector
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            moveInput = 0f;
            jumpRequested = false;
            dashRequested = false;
            return;
        }

        Moviment();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        HandleDashState();

        if (isDashing)
        {
            // Enquanto dashando, ignora movimento normal e gravidade: vai reto
            rb.velocity = new Vector2(facingDirection * dashSpeed, 0f);
        }
        else
        {
            // Movimento
            rb.velocity = new Vector2(moveInput * moveSpeed + ExternalVelocityX, rb.velocity.y);

            // Pulo
            if (jumpRequested && isGrounded)
                rb.AddForce(Vector2.up * jumpForce * JumpForceMultiplier, ForceMode2D.Impulse);

            ApplyFallGravity(); //Aplicacao da gravidad
        }

        jumpRequested = false;
        dashRequested = false;

    }

    private void Moviment()
    {
        // Input do Movimento
        moveInput = inputProvider.GetHorizontal();

        // Input do Pulo
        if (inputProvider.JumpPressed())
            jumpRequested = true; // buffer simples ata o proximo FixedUpdate

        // Input do Dash
        if (inputProvider.DashPressed())
            dashRequested = true;

        // Posi��o do Ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void FlipSprite()
    {
        if (isDashing)
            return;

        // Flip do sprite
        if (moveInput < 0)
        {
            sr.flipX = true;
            facingDirection = -1;
        }
        else if (moveInput > 0)
        {
            sr.flipX = false;
            facingDirection = 1;
        }
    }

    private void ApplyFallGravity()
    {
        if (rb.velocity.y < 0f)
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier * GravityMultiplier; // Caindo: aumenta a gravidade para uma descida mais r�pida e "seca"
        else
            rb.gravityScale = defaultGravityScale;  // Subindo ou parado: gravidade normal
    }

    private void HandleDashState()
    {
        // Reduz o cooldown com o tempo
        if (dashCooldownRemaining > 0f)
            dashCooldownRemaining -= Time.fixedDeltaTime;

        // Inicia o dash se solicitado e disponivel
        if (dashRequested && !isDashing && dashCooldownRemaining <= 0f)
        {
            isDashing = true;
            dashTimeRemaining = dashDuration;
            dashCooldownRemaining = dashCooldown;
            rb.gravityScale = 0f; // sem gravidade durante o dash, pra trajet�ria reta
        }

        // Controla a dura��o do dash em andamento
        if (isDashing)
        {
            dashTimeRemaining -= Time.fixedDeltaTime;
            if (dashTimeRemaining <= 0f)
            {
                isDashing = false;
                rb.gravityScale = defaultGravityScale;
            }
        }
    }
}