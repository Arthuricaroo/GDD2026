using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Titulo "upside down" do menu. Monta as 10 letras sozinho e, quando o mouse passa por cima
/// do titulo, gira cada letra 180 graus (de cabeca pra baixo), em cascata, da esquerda pra direita.
/// Quando o mouse sai, as letras giram de volta.
///
/// Como usar (menu em Canvas 1920x1080):
///  1. Crie um objeto vazio dentro do Canvas chamado "Titulo" e adicione este script.
///  2. Em "Letras Inicial" arraste as 10 imagens letra_XX_x.png e em "Letras Hover" as 10 letra_XX_x_hover.png,
///     na ordem numerica (01 a 10).
///  3. RectTransform do "Titulo": ancora Top Center, Pivot (0.5, 1), Pos X = 0, Pos Y = -104.
///     (O script ajusta o tamanho sozinho.)
///  4. A cena precisa de um EventSystem (o Unity cria junto com o Canvas) e o Canvas de um Graphic Raycaster.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TituloVirarHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sprites (ordem: u p s i d e | d o w n)")]
    public Sprite[] letrasInicial = new Sprite[10];
    public Sprite[] letrasHover = new Sprite[10];

    [Header("Layout (pixels do Canvas 1920x1080)")]
    [Tooltip("Distancia entre o centro de uma letra e o da proxima.")]
    public float passo = 120f;
    [Tooltip("Espaco extra entre 'upside' e 'down'.")]
    public float espacoEntrePalavras = 60f;
    [Tooltip("Quantas letras vem antes do espaco.")]
    public int letrasAntesDoEspaco = 6;

    [Header("Animacao")]
    [Tooltip("Tempo que cada letra leva para girar, em segundos.")]
    public float duracao = 0.40f;
    [Tooltip("Atraso entre uma letra e a seguinte (efeito onda).")]
    public float atrasoEntreLetras = 0.04f;
    [Tooltip("Quanto a letra cresce no meio do giro (0 desliga).")]
    public float aumentoNoGiro = 0.12f;
    [Tooltip("Quanto o giro passa do ponto antes de assentar (0 desliga).")]
    public float sobra = 1.2f;

    Image[] _letras;
    float[] _angulo;          // angulo atual do sprite "inicial": 0 = normal, 180 = invertido
    Coroutine[] _anim;

    void Awake()
    {
        int n = Mathf.Min(letrasInicial.Length, letrasHover.Length);
        _letras = new Image[n];
        _angulo = new float[n];
        _anim = new Coroutine[n];

        // area que recebe o mouse: uma imagem transparente do tamanho do titulo
        var area = GetComponent<Image>();
        if (area == null) area = gameObject.AddComponent<Image>();
        area.color = new Color(0f, 0f, 0f, 0f);
        area.raycastTarget = true;

        float larguraSprite = 0f, alturaSprite = 0f;
        for (int i = 0; i < n; i++)
        {
            var go = new GameObject("Letra_" + (i + 1), typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = letrasInicial[i];
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.SetNativeSize();
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            larguraSprite = Mathf.Max(larguraSprite, rt.sizeDelta.x);
            alturaSprite = Mathf.Max(alturaSprite, rt.sizeDelta.y);
            _letras[i] = img;
            _angulo[i] = 0f;
        }

        float vao = (n - 1) * passo + (n > letrasAntesDoEspaco ? espacoEntrePalavras : 0f);
        for (int i = 0; i < n; i++)
        {
            float x = -vao * 0.5f + i * passo + (i >= letrasAntesDoEspaco ? espacoEntrePalavras : 0f);
            _letras[i].rectTransform.anchoredPosition = new Vector2(x, 0f);
        }

        var raiz = (RectTransform)transform;
        raiz.sizeDelta = new Vector2(vao + larguraSprite, alturaSprite);
    }

    public void OnPointerEnter(PointerEventData eventData) { Virar(true); }
    public void OnPointerExit(PointerEventData eventData) { Virar(false); }

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
        // durante o giro usa sempre o sprite "inicial" girando; no fim troca pelo sprite certo
        // (o hover e o inicial girado 180 graus, so que com a luz do desenho refeita)
        img.sprite = letrasInicial[i];

        float de = _angulo[i];
        float para = virado ? 180f : 0f;
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