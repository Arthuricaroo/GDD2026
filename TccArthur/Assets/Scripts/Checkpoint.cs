using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Vela")]
    public Animator velaAnimator;           // Animator do objeto Vela
    public GameObject luzDaVela;            // opcional: Light2D, comeca desligado
    public float atrasoLuz = 0.4f;          // tempo ate a luz acender

    [Header("Respawn")]
    public Transform pontoRespawn;          // opcional: se vazio, usa a posicao da mesa

    private bool activated = false;

    private void Start()
    {
        if (luzDaVela != null) luzDaVela.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        // Busca o KillPlayer na cena e atualiza o respawn
        KillPlayer killPlayer = FindFirstObjectByType<KillPlayer>();
        if (killPlayer == null) return;

        killPlayer.SetRespawnPoint(pontoRespawn != null ? pontoRespawn : transform);
        activated = true;

        // Acende a vela
        if (velaAnimator != null) velaAnimator.SetTrigger("Acender");
        if (luzDaVela != null) Invoke(nameof(AcenderLuz), atrasoLuz);
    }

    private void AcenderLuz()
    {
        luzDaVela.SetActive(true);
    }
}