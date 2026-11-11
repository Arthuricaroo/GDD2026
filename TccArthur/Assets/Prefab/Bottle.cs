using UnityEngine;

public class Bottle : MonoBehaviour
{
    public float lifeTime = 3f;
    public float throwSpeed = 7f;
    public float throwForceY = 3f;

    public int damage = 20;

    // Tempo logo após nascer em que bater em algo não conta como "desvio"
    public float ignoreCollisionTime = 0.15f;

    // Preenchidos pelo boss quando ele cria a garrafa
    [HideInInspector] public FinalBossController boss;
    [HideInInspector] public float dodgeWeight = 1f;

    private float timer = 0f;
    private bool resolved = false;

    // Jogada horizontal (a que já existia)
    public void Throw(int direction)
    {
        Throw(new Vector2(direction * throwSpeed, throwForceY));
    }

    // Jogada com velocidade livre (mirada, chuva, etc.)
    public void Throw(Vector2 velocity)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
    }

    private void Update()
    {
        if (resolved) return;

        timer += Time.deltaTime;

        // Passou o tempo sem acertar o player: foi desviada
        if (timer >= lifeTime)
        {
            Dodged();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (resolved) return;

        PlayerHealth player = collision.gameObject.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Hit();
            return;
        }

        // Ignora colisões logo ao nascer
        if (timer < ignoreCollisionTime) return;

        // Ignora o próprio boss e outras garrafas
        if (collision.gameObject.GetComponentInParent<FinalBossController>() != null) return;
        if (collision.gameObject.GetComponent<Bottle>() != null) return;

        // Bateu no chão ou na parede sem acertar o player: foi desviada
        Dodged();
    }

    private void Hit()
    {
        resolved = true;

        if (boss != null)
            boss.OnPlayerHit();

        Destroy(gameObject);
    }

    private void Dodged()
    {
        resolved = true;

        if (boss != null)
            boss.OnBottleDodged(dodgeWeight);

        Destroy(gameObject);
    }
}