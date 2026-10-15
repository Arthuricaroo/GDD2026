using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movimento")]
    public Transform posA, posB;
    public float speed = 3f;

    [Header("Detecção")]
    public float margemContato = 0.1f;   // tolerância da checagem "player está apoiado"

    private Vector3 targetPos;

    void Start()
    {
        targetPos = posB.position;
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, posA.position) < 0.05f)
            targetPos = posB.position;

        if (Vector2.Distance(transform.position, posB.position) < 0.05f)
            targetPos = posA.position;

        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    // Enquanto o player estiver em contato, confere se ele está apoiado e gruda/solta
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Transform player = collision.transform;
        bool apoiado = PlayerApoiado(player);

        if (apoiado && player.parent != transform)
            player.SetParent(transform);
        else if (!apoiado && player.parent == transform)
            player.SetParent(null);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (collision.transform.parent == transform)
            collision.transform.SetParent(null);
    }

    bool PlayerApoiado(Transform player)
    {
        // +1 com gravidade normal (player acima), -1 com gravidade invertida (player abaixo)
        float lado = Mathf.Sign(-Physics2D.gravity.y);
        float diferencaY = player.position.y - transform.position.y;
        return diferencaY * lado > margemContato;
    }
}