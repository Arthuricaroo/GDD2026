using UnityEngine;

public class Refugo : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;

    [Header("Velocidades")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;

    [Header("Área de atuação (distância a partir do ponto inicial)")]
    public float leftLimit = 3f;   // quanto pode ir pra esquerda
    public float rightLimit = 3f;  // quanto pode ir pra direita

    [Header("Perseguição")]
    public float detectRange = 4f; // distância pra começar a seguir
    public bool patrol = true;     // false = fica parada até o player chegar

    Rigidbody2D rb;
    Animator anim;
    float minX, maxX;
    int dir = 1;
    bool chasing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        minX = transform.position.x - leftLimit;
        maxX = transform.position.x + rightLimit;
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

        // nunca sai da área definida
        if ((x >= maxX && vx > 0) || (x <= minX && vx < 0))
            vx = 0f;

        rb.linearVelocity = new Vector2(vx, 0f);
        UpdateFacing();
    }

    void UpdateChaseState()
    {
        if (player == null) { SetChasing(false); return; }

        float dist = Vector2.Distance(transform.position, player.position);
        bool playerInArea = player.position.x >= minX && player.position.x <= maxX;

        // quando já está perseguindo, tolera um pouco mais de distância
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
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }

    // mostra a área no Scene view
    void OnDrawGizmosSelected()
    {
        float baseX = Application.isPlaying ? (minX + leftLimit) : transform.position.x;
        Vector3 y = new Vector3(0, transform.position.y, 0);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(baseX - leftLimit, y.y, 0), new Vector3(baseX + rightLimit, y.y, 0));
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}