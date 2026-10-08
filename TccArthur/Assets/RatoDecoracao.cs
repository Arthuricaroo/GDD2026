using UnityEngine;

public class RatoAndador : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 1.5f;
    public bool comecaPraDireita = true;
    public bool spriteOlhaPraDireita = false;   // marque se o seu sprite foi desenhado olhando pra direita

    [Header("Detecção de parede")]
    public Transform checagemParede;            // objeto filho posicionado NA FRENTE do rato
    public float raioChecagem = 0.1f;
    public LayerMask camadaParede;              // layer do Tilemap das paredes

    private Rigidbody2D rb;
    private int direcao;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        direcao = comecaPraDireita ? 1 : -1;
        AplicarFlip();
    }

    void FixedUpdate()
    {
        // Anda sempre na direção atual
        rb.linearVelocity = new Vector2(direcao * velocidade, rb.linearVelocity.y);

        // Bateu na parede? Vira.
        if (Physics2D.OverlapCircle(checagemParede.position, raioChecagem, camadaParede))
        {
            Virar();
        }
    }

    void Virar()
    {
        direcao *= -1;
        AplicarFlip();
    }

    void AplicarFlip()
    {
        Vector3 escala = transform.localScale;
        int lado = spriteOlhaPraDireita ? direcao : -direcao;
        escala.x = Mathf.Abs(escala.x) * lado;
        transform.localScale = escala;
    }

    void OnDrawGizmosSelected()
    {
        if (checagemParede == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checagemParede.position, raioChecagem);
    }
}