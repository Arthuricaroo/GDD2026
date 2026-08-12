using System.Collections;
using UnityEngine;

public class FinalBossController : MonoBehaviour
{
    public GameObject bottlePrefab;
    public Transform bottleSpawnPoint;
    public Animator animator;

    [Header("Tempo entre ataques")]
    public float minWaitTime = 2f;
    public float maxWaitTime = 4f;

    [Header("Movimento")]
    public float moveSpeed = 3f;

    public float leftLimit = -5f;
    public float rightLimit = 5f;

    public LayerMask groundLayer;

    private bool attacking = false;
    private int direction = 1;

    private void Start()
    {
        StartCoroutine(BossRoutine());
    }

    private void Update()
    {
        if (attacking)
        {
            MoveBoss();
        }
    }

    private IEnumerator BossRoutine()
    {
        while (true)
        {
            // Fica parado
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Escolhe aleatoriamente esquerda ou direita
            direction = Random.value < 0.5f ? -1 : 1;

            // Vira o personagem
            FlipBoss();

            // Começa o ataque
            attacking = true;
            animator.Play("FinalBoss_Walk");

            // Espera a animação terminar
            yield return new WaitForSeconds(
                GetAnimationLength("FinalBoss_Walk")
            );

            // Para de andar
            attacking = false;

            // Volta para Idle
            animator.Play("FinalBoss_Idle");
        }
    }

    private void MoveBoss()
    {
        Collider2D bossCollider = GetComponent<Collider2D>();

        if (bossCollider == null)
            return;

        Bounds bounds = bossCollider.bounds;

        // Ponto na frente dos pés do boss
        Vector2 checkPosition = new Vector2(
            bounds.center.x + direction * (bounds.extents.x + 0.1f),
            bounds.min.y + 0.05f
        );

        // Procura chão abaixo desse ponto
        RaycastHit2D groundHit = Physics2D.Raycast(
            checkPosition,
            Vector2.down,
            0.5f,
            groundLayer
        );

        // Se não houver chão, para de andar
        if (groundHit.collider == null)
        {
            attacking = false;
            animator.Play("FinalBoss_Idle");
            return;
        }

        // Se houver chão, continua andando
        Vector3 position = transform.position;

        position.x += direction * moveSpeed * Time.deltaTime;

        position.x = Mathf.Clamp(position.x, leftLimit, rightLimit);

        transform.position = position;
    }

    private void FlipBoss()
    {
        Vector3 scale = transform.localScale;

        scale.x = Mathf.Abs(scale.x) * direction;

        transform.localScale = scale;
    }

    public void ThrowBottle()
    {
        GameObject bottle = Instantiate(
            bottlePrefab,
            bottleSpawnPoint.position,
            Quaternion.identity
        );

        Bottle bottleScript = bottle.GetComponent<Bottle>();

        if (bottleScript != null)
        {
            bottleScript.Throw(direction);
        }
    }

    private float GetAnimationLength(string animationName)
    {
        RuntimeAnimatorController controller =
            animator.runtimeAnimatorController;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        return 1f;
    }
}