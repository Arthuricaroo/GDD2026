using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Patrulha")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;

    [Header("Perseguicao")]
    public float chaseSpeed = 3.5f;
    public float detectionRange = 5f;
    public Transform player;

    [Header("Gravidade")]
    public GravityInverter gravityInverter;

    [Header("Chao")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    [Header("Visual")]
    public bool spriteOlhaParaDireita = true;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private bool isGrounded;
    private Transform currentTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        currentTarget = pointB;
    }

    void Update()
    {
        CheckGround();
        UpdateGravityScale();

        bool gravityInverted = gravityInverter != null && gravityInverter.IsInverted();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!gravityInverted && distanceToPlayer <= detectionRange)
            ChasePlayer();
        else
            Patrol();
    }

    void CheckGround()
    {
       
        bool invertida = rb.gravityScale < 0f;

        Vector2 origin = new Vector2(
            col.bounds.center.x,
            invertida ? col.bounds.max.y : col.bounds.min.y
        );
        Vector2 dir = invertida ? Vector2.up : Vector2.down;

        isGrounded = Physics2D.Raycast(origin, dir, groundCheckDistance, groundLayer);
    }

    void Patrol()
    {
        if (!isGrounded || pointA == null || pointB == null) return;

        float distanceX = Mathf.Abs(transform.position.x - currentTarget.position.x);

        if (distanceX < 0.3f)
            currentTarget = (currentTarget == pointB) ? pointA : pointB;

        MoveTowards(currentTarget.position, patrolSpeed);
    }

    void ChasePlayer()
    {
        if (!isGrounded) return;

        MoveTowards(player.position, chaseSpeed);
    }

    void MoveTowards(Vector3 target, float speed)
    {
        float direction = target.x - transform.position.x;
        float dir = Mathf.Sign(direction);

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        // espelha so o sprite, sem mexer na escala do objeto
        if (sr != null)
            sr.flipX = spriteOlhaParaDireita ? dir < 0 : dir > 0;
    }

    void UpdateGravityScale()
    {
        if (gravityInverter == null) return;

        rb.gravityScale = gravityInverter.IsInverted() ? -1f : 1f;
    }

    void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pointA.position, 0.2f);
            Gizmos.DrawSphere(pointB.position, 0.2f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Collider2D c = GetComponent<Collider2D>();
        if (c != null)
        {
            bool invertida = Application.isPlaying && rb != null && rb.gravityScale < 0f;
            Vector3 origin = new Vector3(
                c.bounds.center.x,
                invertida ? c.bounds.max.y : c.bounds.min.y,
                0f
            );
            Vector3 dir = invertida ? Vector3.up : Vector3.down;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + dir * groundCheckDistance);
        }
    }
}