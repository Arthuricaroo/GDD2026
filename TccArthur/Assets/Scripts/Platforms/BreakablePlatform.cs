using UnityEngine;
using System.Collections;

public class BreakablePlatform : MonoBehaviour
{
    [Header("Configurações")]
    public float breakDelay = 1.5f;
    public float respawnTime = 0f;

    [Header("Feedback Visual")]
    public bool shakeOnBreak = true;
    public float shakeMagnitude = 0.05f;

    [Header("Animação")]
    public string breakTrigger = "Break";
    public string resetTrigger = "Reset";

    private Vector3 originalPosition;
    private bool isCrumbling = false;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;

    void Start()
    {
        originalPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isCrumbling && other.gameObject.CompareTag("Player"))
        {
            bool playerAbove = other.transform.position.y > transform.position.y + 0.1f;
            if (playerAbove)
                StartCoroutine(CrumbleRoutine());
        }
    }

    IEnumerator CrumbleRoutine()
    {
        isCrumbling = true;

        if (anim != null)
            anim.SetTrigger(breakTrigger);

        if (shakeOnBreak)
        {
            float elapsed = 0f;
            while (elapsed < breakDelay)
            {
                transform.position = originalPosition + (Vector3)Random.insideUnitCircle * shakeMagnitude;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(breakDelay);
        }

        transform.position = originalPosition;

        // Se não houver Animator, ou você preferir controlar tudo por código,
        // a plataforma some aqui mesmo. Se estiver usando Animation Event
        // (OnBreakAnimationEnd), pode remover as duas linhas abaixo,
        // pois o Event já vai desativar sr/col no momento certo.
        sr.enabled = false;
        col.enabled = false;

        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);
            Respawn();
        }
    }

    // Chame este método via Animation Event, no último frame do clip "Break",
    // caso queira que o sumiço visual siga exatamente o tempo da animação
    // em vez do breakDelay. Nesse caso, remova sr.enabled/col.enabled
    // do fim do CrumbleRoutine acima para não haver conflito.
    public void OnBreakAnimationEnd()
    {
        sr.enabled = false;
        col.enabled = false;
    }

    void Respawn()
    {
        transform.position = originalPosition;
        sr.enabled = true;
        col.enabled = true;
        isCrumbling = false;

        if (anim != null)
            anim.SetTrigger(resetTrigger);
    }
}