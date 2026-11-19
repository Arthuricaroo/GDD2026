using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float velocidade = 5f;
    public float jumpForce = 10f;

    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Morte")]
    public string nomeClipeMorte = "Player_Death";   // nome do clipe de morte no Animator
    public float duracaoMorteReserva = 1f;           // usado se o clipe nao for encontrado

    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded;
    private bool isFacingRight = true;

    public float gravityCooldown = 0.2f;

    // true enquanto o Ubi esta morrendo (usado tambem pelo GravityInverter)
    public bool estaMorto { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Morto: nao anda, nao pula, nao vira
        if (estaMorto) return;

        // Movimento horizontal
        float moveX = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            moveX * velocidade,
            rb.linearVelocity.y
        );

        // Verifica chao (ou teto, quando a gravidade esta invertida)
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

        // Animacao de corrida
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

    // Chamado pelo KillPlayer: toca a animacao de morte e congela o Ubi.
    // Retorna quanto tempo a animacao dura.
    public float IniciarMorte()
    {
        estaMorto = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;   // sem fisica e sem colisao enquanto morre

        anim.SetBool("isRunning", false);
        anim.ResetTrigger("FlipGravity");
        anim.SetTrigger("Die");

        return DuracaoDoClipe(nomeClipeMorte, duracaoMorteReserva);
    }

    // Chamado pelo KillPlayer depois que o Ubi foi reativado no checkpoint.
    public void Renascer()
    {
        estaMorto = false;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        // Volta o Animator ao estado inicial (Idle) e limpa os gatilhos
        anim.Rebind();
        anim.Update(0f);
        anim.ResetTrigger("Die");
        anim.ResetTrigger("Respawn");
        anim.ResetTrigger("FlipGravity");
    }

    private float DuracaoDoClipe(string nome, float reserva)
    {
        if (anim.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name == nome) return clip.length;
            }
        }
        return reserva;
    }
}