using UnityEngine;

public class Bottle : MonoBehaviour
{
    public float lifeTime = 3f;
    public float throwSpeed = 7f;
    public float throwForceY = 3f;

    public int damage = 20;

    public void Throw(int direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                direction * throwSpeed,
                throwForceY
            );
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerHealth player = collision.gameObject.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}