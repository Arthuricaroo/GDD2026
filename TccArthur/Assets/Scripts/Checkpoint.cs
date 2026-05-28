using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            // Busca o KillPlayer na cena e atualiza o respawn
            KillPlayer killPlayer = FindFirstObjectByType<KillPlayer>();
            if (killPlayer != null)
            {
                killPlayer.SetRespawnPoint(transform);
                activated = true;

                
            }
        }
    }
}