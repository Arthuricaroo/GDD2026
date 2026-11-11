using UnityEngine;

public class Refugo : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Velocidades")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;

    [Header("Area de atuacao (distancia a partir do ponto inicial)")]
    public float leftLimit = 3f;   // quanto pode ir pra esquerda
    public float rightLimit = 3f;  // quanto pode ir pra direita

    [Header("Perseguicao")]
    public float detectRange = 4f; // distancia pra comecar a seguir
    public bool patrol = true;     // false = fica parada ate o player chegar

    [Header("Direcao do sprite")]
    public bool idleOlhaPraDireita = true;      // marque se o sprite do idle foi desenhado olhando pra direita
    public bool perseguirOlhaPraDireita = true; // marque se o sprite de perseguir foi desenhado olhando pra direita

    Rigidbody2D rb;
    Animator anim;
    float minX, maxX;
    Vector2 posicaoInicial;
    int dir = 1;
    bool chasing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        posicaoInicial = transform.position;
        minX = posicaoInicial.x - leftLimit;
        maxX = posicaoInicial.x + rightLimit;
    }

    void OnEnable()
    {
        EventosJogo.PlayerMorreu += VoltarAoCentro;
    }

    void OnDisable()
    {
        EventosJogo.PlayerMorreu -= VoltarAoCentro;
    }

    // Chamado quando o player morre: volta pro ponto inicial e para de perseguir
    void VoltarAoCentro()
    {
        rb.position = posicaoInicial;
        transform.position = posicaoInicial;
        rb.linearVelocity = Vector2.zero;
        dir = 1;
        SetChasing(false);
        UpdateFacing();
    }

    void FixedUpdate()
    {
        UpdateChaseState();

        float x = rb.position.x;
        float vx = 0f;

        if (chasing)
        {
            float dx = player.position.x - x;
            if (Mathf.Abs(dx) > 0.05f)
            {
                dir = dx > 0 ? 1 : -1;
                vx = dir * chaseSpeed;
            }
        }
        else if (patrol)
        {
            if (x >= maxX) dir = -1;
            else if (x <= minX) dir = 1;
            vx = dir * patrolSpeed;
        }

        // nunca sai da area definida
        if ((x >= maxX && vx > 0) || (x <= minX && vx < 0))
            vx = 0f;

        rb.linearVelocity = new Vector2(vx, 0f);
        UpdateFacing();
    }

    void UpdateChaseState()
    {
        // sem player, ou com o player morto (desativado), nao persegue
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            SetChasing(false);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        bool playerInArea = player.position.x >= minX && player.position.x <= maxX;

        // quando ja esta perseguindo, tolera um pouco mais de distancia
        float range = chasing ? detectRange * 1.3f : detectRange;

        SetChasing(dist <= range && playerInArea);
    }

    void SetChasing(bool value)
    {
        chasing = value;
        anim.SetBool("IsChasing", chasing);
    }

    void UpdateFacing()
    {
        // usa a direcao do sprite da animacao que esta tocando agora
        bool spriteOlhaDireita = chasing ? perseguirOlhaPraDireita : idleOlhaPraDireita;
        int lado = spriteOlhaDireita ? dir : -dir;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * lado;
        transform.localScale = s;
    }

    // mostra a area no Scene view
    void OnDrawGizmosSelected()
    {
        float baseX = Application.isPlaying ? posicaoInicial.x : transform.position.x;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(baseX - leftLimit, transform.position.y, 0), new Vector3(baseX + rightLimit, transform.position.y, 0));
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}