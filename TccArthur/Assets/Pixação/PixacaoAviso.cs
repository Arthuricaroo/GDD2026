using UnityEngine;

/// <summary>
/// Pixacao de tutorial na parede. Fica sempre visivel como parte do cenario (meio apagada,
/// como tinta velha) e acende de leve quando o jogador chega perto.
///
/// Como usar:
///  1. Arraste o PNG da pixacao para a cena (vira um objeto com SpriteRenderer) e coloque na parede.
///  2. Sprite Renderer > Sorting Layer / Order in Layer: acima da parede, abaixo do jogador.
///  3. Adicione um Box Collider 2D, marque "Is Trigger" e deixe-o maior que o desenho
///     (a area onde o jogador "le" a pixacao).
///  4. Adicione este script. O jogador precisa ter a Tag "Player".
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PixacaoAviso : MonoBehaviour
{
    [Range(0f, 1f)] public float opacidadeLonge = 0.55f;
    [Range(0f, 1f)] public float opacidadePerto = 1f;
    public float velocidade = 4f;
    public string tagDoJogador = "Player";

    SpriteRenderer _sr;
    float _alvo;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _alvo = opacidadeLonge;
        SetAlpha(opacidadeLonge);
    }

    void Update()
    {
        float a = Mathf.MoveTowards(_sr.color.a, _alvo, velocidade * Time.deltaTime);
        SetAlpha(a);
    }

    void OnTriggerEnter2D(Collider2D o) { if (o.CompareTag(tagDoJogador)) _alvo = opacidadePerto; }
    void OnTriggerExit2D(Collider2D o) { if (o.CompareTag(tagDoJogador)) _alvo = opacidadeLonge; }

    void SetAlpha(float a)
    {
        var c = _sr.color; c.a = a; _sr.color = c;
    }
}
