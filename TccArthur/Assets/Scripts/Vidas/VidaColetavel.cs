using UnityEngine;

public class VidaColetavel : MonoBehaviour
{
    public AudioClip somColeta;
    [Range(0f, 1f)] public float volume = 0.7f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Com as 3 vidas cheias nao coleta, fica la para depois
        if (!VidaJogador.GanharVida()) return;

        if (somColeta != null)
            AudioSource.PlayClipAtPoint(somColeta, transform.position, volume);

        Destroy(gameObject);
    }
}