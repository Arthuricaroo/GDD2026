using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Vela")]
    public Animator velaAnimator;           
    public GameObject luzDaVela;       
    public float atrasoLuz = 0.4f;      

    [Header("Respawn")]
    public Transform pontoRespawn;          

    private bool activated = false;

    private void Start()
    {
        if (luzDaVela != null) luzDaVela.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        KillPlayer.SetRespawnPoint(pontoRespawn != null ? pontoRespawn : transform);
        activated = true;

        
        if (velaAnimator != null) velaAnimator.SetTrigger("Acender");
        if (luzDaVela != null) Invoke(nameof(AcenderLuz), atrasoLuz);
    }

    private void AcenderLuz()
    {
        luzDaVela.SetActive(true);
    }
}