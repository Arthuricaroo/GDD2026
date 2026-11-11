using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour
{
    public GameObject Player;
    public Transform respawnPoint;          // ponto inicial (arraste no Inspector)
    public float respawnDelay = 1.5f;

    // Compartilhado por TODOS os obstaculos
    public static Transform checkpointAtual;
    private static bool respawning = false;

    private GravityInverter gravityScript;
    private Rigidbody2D playerRb;

    void Start()
    {
        gravityScript = Player.GetComponent<GravityInverter>();
        playerRb = Player.GetComponent<Rigidbody2D>();

        // Define o ponto inicial so se ainda nao existir um valido
        if (checkpointAtual == null)
            checkpointAtual = respawnPoint;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (respawning) return;
        if (!other.gameObject.CompareTag("Player")) return;

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        respawning = true;
        Player.transform.SetParent(null);

        Player.SetActive(false);

        // Avisa os mobs (Refugo e Rato) para voltarem ao ponto inicial
        EventosJogo.AvisarMorteDoPlayer();

        if (gravityScript != null)
            gravityScript.ResetGravity();

        yield return new WaitForSeconds(respawnDelay);

        Player.transform.position = checkpointAtual.position;

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        Player.SetActive(true);
        respawning = false;
    }

    public static void SetRespawnPoint(Transform newPoint)
    {
        checkpointAtual = newPoint;
        Debug.Log("Checkpoint atualizado: " + newPoint.position);
    }
}