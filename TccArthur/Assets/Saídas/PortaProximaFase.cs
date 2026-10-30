using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Porta / saida de fase. Quando o jogador encosta, a porta abre (se tiver sprite de porta aberta),
/// a tela escurece e a proxima fase e carregada.
///
/// Como usar:
///  1. Arraste o sprite (porta_fechada ou saida_porao) para a cena e posicione no chao, junto da parede.
///  2. Adicione um Box Collider 2D, marque "Is Trigger" e ajuste para cobrir a porta (ou so o vao dela).
///  3. Adicione este script.
///  4. Escreva o nome da proxima cena em "Nome Da Proxima Fase". Se deixar vazio, vai para a
///     cena seguinte da lista (File > Build Profiles > Scene List). As cenas PRECISAM estar nessa lista.
///  5. (Opcional) Porta: arraste porta_aberta em "Sprite Aberta". Saida do porao: deixe vazio.
///  6. (Opcional) Fade: crie um Image preto em tela cheia dentro do Canvas, adicione um CanvasGroup
///     (Alpha 0, desmarque Interactable e Blocks Raycasts) e arraste em "Fade Tela".
///  O jogador precisa estar com a Tag "Player".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PortaProximaFase : MonoBehaviour
{
    [Header("Destino")]
    [Tooltip("Nome exato da cena. Vazio = proxima cena da lista de Build.")]
    public string nomeDaProximaFase = "";

    [Header("Jogador")]
    public string tagDoJogador = "Player";
    [Tooltip("Para o jogador no lugar enquanto a porta abre e a tela escurece.")]
    public bool congelarJogador = true;

    [Header("Efeitos (opcionais)")]
    public Sprite spriteAberta;
    [Tooltip("Tempo entre encostar e carregar a fase, em segundos.")]
    public float atraso = 0.6f;
    public CanvasGroup fadeTela;
    public UnityEvent aoEntrar;

    bool _usada;

    void Reset()
    {
        // ao adicionar o script, ja deixa o collider como trigger
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (_usada || !outro.CompareTag(tagDoJogador)) return;
        _usada = true;
        StartCoroutine(Entrar(outro));
    }

    IEnumerator Entrar(Collider2D jogador)
    {
        aoEntrar?.Invoke();

        if (congelarJogador)
        {
            var rb = jogador.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
        }

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && spriteAberta != null) sr.sprite = spriteAberta;

        float t = 0f;
        while (t < atraso)
        {
            t += Time.unscaledDeltaTime;
            if (fadeTela != null) fadeTela.alpha = Mathf.Clamp01(t / Mathf.Max(0.01f, atraso));
            yield return null;
        }
        if (fadeTela != null) fadeTela.alpha = 1f;

        if (!string.IsNullOrEmpty(nomeDaProximaFase))
        {
            if (Application.CanStreamedLevelBeLoaded(nomeDaProximaFase))
                SceneManager.LoadScene(nomeDaProximaFase);
            else
                Debug.LogError("PortaProximaFase: a cena '" + nomeDaProximaFase + "' nao esta na lista de Build (File > Build Profiles).");
        }
        else
        {
            int proxima = SceneManager.GetActiveScene().buildIndex + 1;
            if (proxima < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(proxima);
            else
                Debug.LogWarning("PortaProximaFase: nao existe uma cena depois desta na lista de Build.");
        }
    }
}
