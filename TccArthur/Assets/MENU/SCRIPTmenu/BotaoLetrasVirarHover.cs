using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Efeito dos botoes do menu: quando o mouse passa por cima, cada letra gira 180 graus (de cabeca
/// pra baixo) em cascata, da esquerda pra direita. Ao sair, gira de volta.
/// Tambem serve para o botao de configuracoes: com UMA imagem so (a engrenagem) ela gira 90 graus.
///
/// Como usar:
///  1. No botao (Jogar, Sair ou Config), apague o filho "Text (TMP)".
///  2. Image do botao = placa normal (placa_jogar). Button > Transition = Sprite Swap:
///     Highlighted = placa_jogar_hover, Pressed = placa_jogar_pressionado. Clique em Set Native Size.
///  3. Adicione este script ao botao.
///  4. Letras Inicial = as imagens jogar_1_J ... jogar_5_R, na ordem.
///     Letras Hover   = jogar_1_J_hover ... jogar_5_R_hover, na ordem.
///     (Config: uma imagem em cada lista - config_icone e config_icone_hover - e Angulo Final = 90.)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class BotaoLetrasVirarHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Sprites (na ordem das letras)")]
    public Sprite[] letrasInicial;
    public Sprite[] letrasHover;

    [Header("Layout (pixels do Canvas 1920x1080)")]
    [Tooltip("Distancia entre o centro de uma letra e o da proxima (Jogar e Sair: 48).")]
    public float passo = 48f;
    [Tooltip("Ajuste fino do conjunto, em pixels. Os valores padrao reproduzem o botao desenhado.")]
    public Vector2 deslocamento = new Vector2(-4f, 10f);
    [Tooltip("Quanto o texto desce quando o botao e pressionado (a placa desce 12 px).")]
    public float descidaPressionado = 12f;

    [Header("Animacao")]
    public float duracao = 0.35f;
    public float atrasoEntreLetras = 0.05f;
    [Tooltip("Quanto a letra cresce no meio do giro (0 desliga).")]
    public float aumentoNoGiro = 0.12f;
    [Tooltip("Quanto o giro passa do ponto antes de assentar (0 desliga).")]
    public float sobra = 1.2f;
    [Tooltip("Letras: 180. Engrenagem: 90.")]
    public float anguloFinal = 180f;

    Image[] _letras;
    float[] _angulo;
    Coroutine[] _anim;
    bool _pressionado;
    bool _sobre;

    void Awake()
    {
        int n = Mathf.Min(letrasInicial.Length, letrasHover.Length);
        _letras = new Image[n];
        _angulo = new float[n];
        _anim = new Coroutine[n];

        for (int i = 0; i < n; i++)
        {
            var go = new GameObject("Letra_" + (i + 1), typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = letrasInicial[i];
            img.raycastTarget = false;          // quem recebe o mouse e a placa do botao
            img.preserveAspect = true;
            img.SetNativeSize();
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            _letras[i] = img;
        }
        Posicionar();
    }

    void Posicionar()
    {
        int n = _letras.Length;
        float y = deslocamento.y - (_pressionado ? descidaPressionado : 0f);
        for (int i = 0; i < n; i++)
        {
            float x = (i - (n - 1) * 0.5f) * passo + deslocamento.x;
            _letras[i].rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }

    void OnDisable()
    {
        // se o botao for desativado no meio da animacao, volta tudo ao normal
        if (_letras == null) return;
        _sobre = false; _pressionado = false;
        for (int i = 0; i < _letras.Length; i++)
        {
            _anim[i] = null;
            _angulo[i] = 0f;
            _letras[i].sprite = letrasInicial[i];
            _letras[i].rectTransform.localRotation = Quaternion.identity;
            _letras[i].rectTransform.localScale = Vector3.one;
        }
        Posicionar();
    }

    public void OnPointerEnter(PointerEventData e) { _sobre = true; Virar(true); }
    public void OnPointerExit(PointerEventData e) { _sobre = false; _pressionado = false; Posicionar(); Virar(false); }
    public void OnPointerDown(PointerEventData e) { _pressionado = true; Posicionar(); }
    public void OnPointerUp(PointerEventData e) { _pressionado = false; Posicionar(); }

    void Virar(bool virado)
    {
        for (int i = 0; i < _letras.Length; i++)
        {
            if (_anim[i] != null) StopCoroutine(_anim[i]);
            _anim[i] = StartCoroutine(Animar(i, virado, i * atrasoEntreLetras));
        }
    }

    IEnumerator Animar(int i, bool virado, float atraso)
    {
        if (atraso > 0f) yield return new WaitForSecondsRealtime(atraso);

        var img = _letras[i];
        var rt = img.rectTransform;
        // no giro usa sempre o sprite "inicial"; no fim troca pelo sprite certo
        // (o hover das letras e o inicial girado 180 graus, com a luz refeita)
        img.sprite = letrasInicial[i];

        float de = _angulo[i];
        float para = virado ? anguloFinal : 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, duracao);
            float k = Mathf.Clamp01(t);
            float e = EaseOutBack(k, sobra);
            _angulo[i] = Mathf.LerpUnclamped(de, para, e);
            rt.localRotation = Quaternion.Euler(0f, 0f, _angulo[i]);
            float s = 1f + aumentoNoGiro * Mathf.Sin(Mathf.PI * k);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        _angulo[i] = para;
        img.sprite = virado ? letrasHover[i] : letrasInicial[i];
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
        _anim[i] = null;
    }

    static float EaseOutBack(float x, float s)
    {
        float c3 = s + 1f;
        float p = x - 1f;
        return 1f + c3 * p * p * p + s * p * p;
    }
}
