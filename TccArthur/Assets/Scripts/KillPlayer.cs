using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour
{
    public GameObject Player;
    public Transform respawnPoint;   // checkpoint inicial (arraste no Inspector)
    public float respawnDelay = 1.5f;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        Player.SetActive(false);
        yield return new WaitForSeconds(respawnDelay);
        Player.transform.position = respawnPoint.position;
        Player.SetActive(true);
    }

    // Chamado pelo script Checkpoint quando o player toca num novo checkpoint
    public void SetRespawnPoint(Transform newPoint)
    {
        respawnPoint = newPoint;
        Debug.Log("Checkpoint atualizado: " + newPoint.position);
    }
}