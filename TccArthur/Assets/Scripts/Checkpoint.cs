using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Vela")]
    public Animator velaAnimator;
    public GameObject luzDaVela;
    public float atrasoLuz = 0.4f;

    [Header("Respawn")]
    public Transform pontoRespawn;

    [Header("Áudio - Clipes (arraste os 3 aqui)")]
    public AudioClip somFosforo;
    public AudioClip somSting;
    public AudioClip somCrepitar;

    [Header("Áudio - Volumes e Fades")]
    [Range(0f, 1f)] public float volumeFosforo = 0.8f;
    [Range(0f, 1f)] public float volumeSting = 0.6f;
    [Range(0f, 1f)] public float volumeCrepitar = 0.3f;
    public float fadeOutSting = 1.5f;
    public float fadeInCrepitar = 1f;
    public float crossfadeLoop = 1.5f;   // quanto o fim e o começo do loop se misturam

    [Header("Áudio - Distância do crepitar")]
    public float distanciaMinima = 10f;
    public float distanciaMaxima = 30f;

    private bool activated = false;
    private AudioSource fosforoSource;
    private AudioSource stingSource;
    private AudioSource crepitarA;
    private AudioSource crepitarB;

    private void Awake()
    {
        fosforoSource = CriarSource(somFosforo, false, 0f);
        stingSource = CriarSource(somSting, false, 0f);

        // Dois sources para o loop com crossfade (loop desligado: o script faz o loop)
        crepitarA = CriarSource(somCrepitar, false, 1f);
        crepitarB = CriarSource(somCrepitar, false, 1f);
        ConfigurarDistancia(crepitarA);
        ConfigurarDistancia(crepitarB);
    }

    private AudioSource CriarSource(AudioClip clip, bool loop, float spatialBlend)
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.clip = clip;
        src.loop = loop;
        src.playOnAwake = false;
        src.spatialBlend = spatialBlend;
        return src;
    }

    private void ConfigurarDistancia(AudioSource src)
    {
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = distanciaMinima;
        src.maxDistance = distanciaMaxima;
    }

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

        if (somFosforo != null)
        {
            fosforoSource.volume = volumeFosforo;
            fosforoSource.Play();
        }
        else Debug.LogWarning("Checkpoint: falta o áudio do fósforo", this);

        Invoke(nameof(AcenderLuz), atrasoLuz);
    }

    private void AcenderLuz()
    {
        if (luzDaVela != null) luzDaVela.SetActive(true);

        if (somSting != null) StartCoroutine(TocarStingComFade());
        else Debug.LogWarning("Checkpoint: falta o áudio do sting", this);

        if (somCrepitar != null) StartCoroutine(LoopCrossfade());
        else Debug.LogWarning("Checkpoint: falta o áudio do crepitar", this);
    }

    private IEnumerator TocarStingComFade()
    {
        stingSource.volume = volumeSting;
        stingSource.Play();

        float espera = Mathf.Max(0f, somSting.length - fadeOutSting);
        yield return new WaitForSeconds(espera);

        yield return FadeOut(stingSource, fadeOutSting);
    }

    // Loop do crepitar: o fim de um se mistura com o começo do outro
    private IEnumerator LoopCrossfade()
    {
        // O crossfade nunca passa de 40% do tamanho do áudio
        float fade = Mathf.Min(crossfadeLoop, somCrepitar.length * 0.4f);

        AudioSource atual = crepitarA;
        AudioSource outro = crepitarB;

        // Fade in inicial
        atual.volume = 0f;
        atual.Play();

        float t0 = 0f;
        while (t0 < fadeInCrepitar)
        {
            t0 += Time.deltaTime;
            atual.volume = Mathf.Lerp(0f, volumeCrepitar, t0 / fadeInCrepitar);
            yield return null;
        }
        atual.volume = volumeCrepitar;

        while (true)
        {
            // Espera chegar perto do fim do áudio atual
            while (atual.isPlaying && atual.time < somCrepitar.length - fade)
                yield return null;

            // Começa o outro do zero e mistura os dois (curva de potência constante)
            outro.volume = 0f;
            outro.time = 0f;
            outro.Play();

            float t = 0f;
            while (t < fade)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fade);
                atual.volume = Mathf.Cos(k * Mathf.PI * 0.5f) * volumeCrepitar;
                outro.volume = Mathf.Sin(k * Mathf.PI * 0.5f) * volumeCrepitar;
                yield return null;
            }

            atual.Stop();
            outro.volume = volumeCrepitar;

            // Troca os papéis e repete
            AudioSource temp = atual;
            atual = outro;
            outro = temp;
        }
    }

    private IEnumerator FadeOut(AudioSource source, float duracao)
    {
        float volumeInicial = source.volume;
        float t = 0f;

        while (t < duracao)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(volumeInicial, 0f, t / duracao);
            yield return null;
        }

        source.Stop();
        source.volume = volumeInicial;
    }
}