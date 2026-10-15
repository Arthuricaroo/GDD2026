using UnityEngine;
using System.Collections;

public class BreakablePlatform : MonoBehaviour
{
    [Header("Configurações")]
    public float breakDelay = 1.5f;
    public float respawnTime = 0f;          // 0 = não volta / >0 = volta após X segundos

    [Header("Detecção")]
    public float margemContato = 0.1f;      // tolerância da checagem "player está apoiado"

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
        if (isCrumbling) return;
        if (!other.gameObject.CompareTag("Player")) return;

        // +1 com gravidade normal (player acima), -1 com gravidade invertida (player abaixo)
        float lado = Mathf.Sign(-Physics2D.gravity.y);

        float diferencaY = other.transform.position.y - transform.position.y;
        bool playerApoiado = diferencaY * lado > margemContato;

        if (playerApoiado)
            StartCoroutine(CrumbleRoutine());
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

        // Some visualmente e desliga a colisão
        // (se usar Animation Event, remova estas duas linhas)
        sr.enabled = false;
        col.enabled = false;

        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);
            Respawn();
        }
    }

    // Opcional: chame via Animation Event no último frame do clip "Break"
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
