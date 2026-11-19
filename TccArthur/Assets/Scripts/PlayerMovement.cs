using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float velocidade = 5f;
    public float jumpForce = 10f;

    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Morte")]
    public string nomeClipeMorte = "Player_Death";   // nome do clipe/estado de morte no Animator
    public string nomeEstadoIdle = "Idleprota";      // nome EXATO do estado parado no Animator
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

        // Vivo: a animacao de morte nao pode tocar
        ProtegerContraMorteFalsa();

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

    // Se o Animator cair na morte com o Ubi vivo, volta para o estado parado
    private void ProtegerContraMorteFalsa()
    {
        anim.ResetTrigger("Die");

        if (anim.GetCurrentAnimatorStateInfo(0).IsName(nomeClipeMorte) ||
            anim.GetNextAnimatorStateInfo(0).IsName(nomeClipeMorte))
        {
            Debug.LogWarning("PlayerMovement: a animacao de morte tocou com o Ubi vivo. Voltando ao estado parado.");
            ForcarIdle();
        }
    }

    private void ForcarIdle()
    {
        int estadoIdle = Animator.StringToHash(nomeEstadoIdle);
        if (anim.HasState(0, estadoIdle))
        {
            anim.Play(estadoIdle, 0, 0f);
        }
        else
        {
            Debug.LogWarning("PlayerMovement: nao existe o estado '" + nomeEstadoIdle +
                             "' no Animator. Escreva o nome exato do estado parado em Nome Estado Idle.");
        }
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

        // Reinicia o Animator, limpa os gatilhos e forca o estado parado
        anim.Rebind();
        LimparGatilhos();
        ForcarIdle();
        anim.SetBool("isRunning", false);
        anim.Update(0f);
        LimparGatilhos();
    }

    private void LimparGatilhos()
    {
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