using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        anim.SetFloat("Speed", movement.magnitude);

        if (movement.x > 0)
            sr.flipX = false;

        if (movement.x < 0)
            sr.flipX = true;
    }

    void FixedUpdate()
    {
        rb.MovePosition(
            rb.position +
            movement * speed * Time.fixedDeltaTime
        );
    }
}