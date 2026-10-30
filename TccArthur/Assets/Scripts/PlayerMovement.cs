using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float velocidade = 5f;
    public float jumpForce = 10f;

    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded;
    private bool isFacingRight = true;

    public float gravityCooldown = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Movimento horizontal
        float moveX = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            moveX * velocidade,
            rb.linearVelocity.y
        );

        // Verifica chão (ou teto, quando a gravidade está invertida)
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );

        // Pulo
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Gravidade para baixo: pula para cima (+1). Invertida: pula para baixo (-1).
            float direcaoPulo = Physics2D.gravity.y < 0 ? 1f : -1f;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce * direcaoPulo
            );
        }

        // Animação de corrida
        anim.SetBool("isRunning", moveX != 0);

        // Flip do personagem
        if (moveX > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveX < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        transform.localScale = new Vector3(
            transform.localScale.x * -1,
            transform.localScale.y,
            transform.localScale.z
        );
    }
}