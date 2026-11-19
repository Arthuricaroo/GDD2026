using UnityEngine;
using UnityEngine.SceneManagement;   // necessario para trocar de cena
using System.Collections;

public class KillPlayer : MonoBehaviour
{
    public GameObject Player;               // se estiver vazio, acha sozinho pela tag Player
    public Transform respawnPoint;          // ponto inicial (opcional)
    public float respawnDelay = 0.5f;       // tempo com o Ubi sumido, depois da animacao de morte

    // Compartilhado por TODOS os obstaculos
    public static Transform checkpointAtual;
    private static bool respawning = false;

    // Executa a rotina de respawn mesmo se este obstaculo for desativado ou destruido
    private class Executor : MonoBehaviour { }
    private static Executor executor;

    private GravityInverter gravityScript;
    private PlayerMovement movimento;
    private Rigidbody2D playerRb;
    private Vector3 posicaoInicial;

    void Start()
    {
        respawning = false;   // cena nova: libera de novo

        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player");

        if (Player == null)
        {
            Debug.LogError("KillPlayer em '" + gameObject.name + "': nao achei nenhum objeto com a tag Player.");
            return;
        }

        gravityScript = Player.GetComponent<GravityInverter>();
        movimento = Player.GetComponent<PlayerMovement>();
        playerRb = Player.GetComponent<Rigidbody2D>();

        // Posicao onde o Ubi comeca a fase (ultimo recurso do respawn)
        posicaoInicial = Player.transform.position;

        // Define o ponto inicial so se ainda nao existir um valido
        if (checkpointAtual == null && respawnPoint != null)
            checkpointAtual = respawnPoint;
    }

    // Chamado pelo Bottle (garrafa do boss) quando acerta o Ubi
    public void MatarPorGarrafa()
    {
        if (respawning) return;

        if (Player == null)
        {
            Debug.LogError("KillPlayer: nao achei o Player (confira a tag Player).");
            return;
        }

        Debug.Log("KillPlayer: uma garrafa do boss matou o Ubi", gameObject);

        Rodar(RespawnRoutine());
    }

    // Colisao normal (Rato, Refugo, espinhos, garrafas com collider solido)
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
        if (Player == null) return;
        if (!alvo.CompareTag("Player")) return;

        Debug.Log("KillPlayer: o objeto '" + gameObject.name + "' matou o Ubi", gameObject);

        Rodar(RespawnRoutine());
    }

    private void Rodar(IEnumerator rotina)
    {
        if (executor == null)
        {
            GameObject go = new GameObject("KillPlayerExecutor");
            executor = go.AddComponent<Executor>();
        }
        executor.StartCoroutine(rotina);
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

        // Acabaram as vidas: vai para a tela de Game Over
        if (vidasRestantes <= 0)
        {
            // A gravidade e global: se o Ubi morreu invertido, volta ao normal
            Physics2D.gravity = new Vector2(0f, -9.81f);

            // Proxima partida comeca do zero
            checkpointAtual = null;
            VidaJogador.Resetar();

            SceneManager.LoadScene("GameOver");   // no momento do game over
            yield break;                          // respawning continua true: nada mais mata o Ubi
        }

        Player.SetActive(false);

        if (gravityScript != null)
            gravityScript.ResetGravity();

        yield return new WaitForSeconds(respawnDelay);

        // Destino: ultimo checkpoint > respawnPoint > posicao inicial do Ubi
        Vector3 destino = posicaoInicial;
        if (checkpointAtual != null)
            destino = checkpointAtual.position;
        else if (respawnPoint != null)
            destino = respawnPoint.position;

        Player.transform.position = destino;

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