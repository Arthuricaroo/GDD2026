using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public Transform player;

    public float speed = 2f;

    public float detectionRadius = 5f;

    private Animator anim;

    private SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();

        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position);

        if (distance <= detectionRadius)
        {
            transform.position =
                Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime);

            anim.SetFloat("Speed", 1);

            if (player.position.x > transform.position.x)
                sr.flipX = false;
            else
                sr.flipX = true;
        }
        else
        {
            anim.SetFloat("Speed", 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius);
    }
}