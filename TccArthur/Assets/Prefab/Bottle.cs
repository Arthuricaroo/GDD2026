using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
    public float lifeTime = 3f;
    public float throwSpeed = 7f;
    public float throwForceY = 3f;

    public int damage = 20;

    // Giro da garrafa quando voa reta (0 desliga). Cuidado: gira o collider junto.
    public float spinSpeed = 0f;

    // Tempo logo após nascer em que bater em algo não conta como "desvio"
    public float ignoreCollisionTime = 0.15f;

    // Preenchidos pelo boss quando ele cria a garrafa
    [HideInInspector] public FinalBossController boss;
    [HideInInspector] public float dodgeWeight = 1f;

    // Lista de todas as garrafas vivas (para elas ignorarem umas às outras)
    private static List<Bottle> allBottles = new List<Bottle>();

    private Rigidbody2D rb;
    private Collider2D col;

    private float timer = 0f;
    private bool resolved = false;
    private bool straightFlight = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Ignora colisão física com todas as outras garrafas
        if (col != null)
        {
            for (int i = allBottles.Count - 1; i >= 0; i--)
            {
                Bottle other = allBottles[i];

                if (other == null)
                {
                    allBottles.RemoveAt(i);
                    continue;
                }

                Collider2D otherCol = other.GetComponent<Collider2D>();

                if (otherCol != null)
                    Physics2D.IgnoreCollision(col, otherCol);
            }
        }

        allBottles.Add(this);
    }

    private void OnDestroy()
    {
        allBottles.Remove(this);
    }

    // Jogada horizontal (a que já existia)
    public void Throw(int direction)
    {
        Throw(new Vector2(direction * throwSpeed, throwForceY), false);
    }

    // Jogada com velocidade livre, afetada pela gravidade do prefab
    public void Throw(Vector2 velocity)
    {
        Throw(velocity, false);
    }

    // Jogada com velocidade livre. Se straight for true, a garrafa voa reta (sem gravidade, Y travado)
    public void Throw(Vector2 velocity, bool straight)
    {
        if (rb == null) return;

        // Ignora colisão física com o boss (para ele não empurrar a garrafa)
        if (boss != null && col != null)
        {
            Collider2D bossCol = boss.GetComponent<Collider2D>();

            if (bossCol != null)
                Physics2D.IgnoreCollision(col, bossCol);
        }

        straightFlight = straight;

        if (straight)
        {
            rb.gravityScale = 0f;

            // Trava o Y e a rotação: nada empurra a garrafa para cima ou para baixo
            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

            rb.linearVelocity = new Vector2(velocity.x, 0f);
        }
        else
        {
            rb.linearVelocity = velocity;
        }
    }

    private void Update()
    {
        if (resolved) return;

        timer += Time.deltaTime;

        // Giro só visual (opcional)
        if (straightFlight && spinSpeed != 0f)
        {
            float dir = (rb != null && rb.linearVelocity.x >= 0f) ? -1f : 1f;
            transform.Rotate(0f, 0f, dir * spinSpeed * Time.deltaTime);
        }

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

        // Garrafa reta: ignora contato com chão e teto (só parede e player contam)
        if (straightFlight && collision.contactCount > 0)
        {
            Vector2 normal = collision.GetContact(0).normal;

            if (Mathf.Abs(normal.y) > 0.5f) return;
        }

        // Bateu em algo sem acertar o player: foi desviada
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