using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

    [Header("Áudio")]                                      // NOVO
    public AudioClip somSting;                             // NOVO
    [Range(0f, 1f)] public float volumeSting = 0.6f;       // NOVO

    bool _usada;
    AudioSource _audio;                                    // NOVO

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    void Awake()                                           // NOVO
    {
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.loop = false;
        _audio.spatialBlend = 0f; // 2D: toca inteiro, independente da distância
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

        if (somSting != null) _audio.PlayOneShot(somSting, volumeSting);   // NOVO

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