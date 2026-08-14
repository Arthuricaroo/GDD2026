using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Patrulha")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;

    [Header("Perseguição")]
    public float chaseSpeed = 3.5f;
    public float detectionRange = 5f;
    public Transform player;

    [Header("Gravidade")]
    public GravityInverter gravityInverter;   

    [Header("Chão")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.6f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Transform currentTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
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
        rb.linearVelocity = new Vector2(Mathf.Sign(direction) * speed, rb.linearVelocity.y);
        transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
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

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}