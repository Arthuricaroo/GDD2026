using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [Header("Configurações")]
    public float fallDelay = 0.5f;     // tempo até começar a cair após o player pisar
    public float respawnTime = 0f;     // 0 = não respawna; >0 = ressurge após X segundos
    public float fallGravity = 3f;     // quão rápido cai
    
    [Header("Limpeza")]
    public float destroyAfterFall = 3f;

    private Rigidbody2D rb;
    private Vector3 originalPosition;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // começa estática
        originalPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isFalling && other.gameObject.CompareTag("Player"))
        {
            bool playerAbove = other.transform.position.y > transform.position.y + 0.1f;
            if (playerAbove)
                StartCoroutine(FallRoutine());
            if (respawnTime <= 0f)
                Destroy(gameObject, destroyAfterFall);
        }
    }

    System.Collections.IEnumerator FallRoutine()
    {
        isFalling = true;

        yield return new WaitForSeconds(fallDelay);

        // Solta a física para cair
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravity;

        // Respawn opcional
        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);
            Reset();
        }
    }

    void Reset()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.position = originalPosition;
        isFalling = false;
    }
}