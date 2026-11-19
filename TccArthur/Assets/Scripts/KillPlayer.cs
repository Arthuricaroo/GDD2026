using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour
{
    public GameObject Player;
    public Transform respawnPoint;          // ponto inicial (arraste no Inspector)
    public float respawnDelay = 0.5f;       // tempo com o Ubi sumido, depois da animacao de morte

    // Compartilhado por TODOS os obstaculos
    public static Transform checkpointAtual;
    private static bool respawning = false;

    private GravityInverter gravityScript;
    private PlayerMovement movimento;
    private Rigidbody2D playerRb;
    private GameOverUI gameOver;

    void Start()
    {
        respawning = false;   // cena nova: libera de novo

        gravityScript = Player.GetComponent<GravityInverter>();
        movimento = Player.GetComponent<PlayerMovement>();
        playerRb = Player.GetComponent<Rigidbody2D>();
        gameOver = FindFirstObjectByType<GameOverUI>();

        // Define o ponto inicial so se ainda nao existir um valido
        if (checkpointAtual == null)
            checkpointAtual = respawnPoint;
    }

    // Colisao normal (Rato, Refugo, espinhos com collider solido)
    private void OnCollisionEnter2D(Collision2D other)
    {
        TentarMatar(other.gameObject);
    }

    // Collider marcado como Is Trigger (zonas de morte, buracos)
    private void OnTriggerEnter2D(Collider2D other)
    {
        TentarMatar(other.gameObject);
    }

    private void TentarMatar(GameObject alvo)
    {
        if (respawning) return;
        if (!alvo.CompareTag("Player")) return;

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        respawning = true;
        Player.transform.SetParent(null);

        // Perde 1 vida
        int vidasRestantes = VidaJogador.PerderVida();

        // Toca a animacao de morte (o Ubi fica parado e sem colisao)
        float duracaoMorte = 0f;
        if (movimento != null)
            duracaoMorte = movimento.IniciarMorte();

        // Avisa os mobs (Refugo e Rato) para voltarem ao ponto inicial
        EventosJogo.AvisarMorteDoPlayer();

        // Espera a animacao terminar
        yield return new WaitForSeconds(duracaoMorte);

        // Acabaram as vidas: game over e volta para a fase 1
        if (vidasRestantes <= 0)
        {
            if (gameOver != null)
            {
                gameOver.Mostrar();
                yield break;   // respawning continua true: nada mais mata o Ubi
            }

            // Sem GameOverUI na cena: so devolve as vidas e renasce normalmente
            Debug.LogWarning("KillPlayer: nao ha GameOverUI na cena. Vidas devolvidas.");
            VidaJogador.Resetar();
        }

        Player.SetActive(false);

        if (gravityScript != null)
            gravityScript.ResetGravity();

        yield return new WaitForSeconds(respawnDelay);

        Player.transform.position = checkpointAtual.position;

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        Player.SetActive(true);

        // Liga tudo de novo e volta o Animator para o Idle
        if (movimento != null)
            movimento.Renascer();

        respawning = false;
    }

    public static void SetRespawnPoint(Transform newPoint)
    {
        checkpointAtual = newPoint;
        Debug.Log("Checkpoint atualizado: " + newPoint.position);
    }
}