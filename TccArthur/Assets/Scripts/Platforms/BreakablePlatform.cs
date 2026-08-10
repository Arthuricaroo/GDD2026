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

    private Vector3 originalPosition;
    private bool isCrumbling = false;

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        originalPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
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

        // "Quebra" — some visualmente e desliga a colisão, mas o script continua vivo
        transform.position = originalPosition;
        sr.enabled = false;
        col.enabled = false;

        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);

            sr.enabled = true;
            col.enabled = true;
            isCrumbling = false;
        }
    }
}