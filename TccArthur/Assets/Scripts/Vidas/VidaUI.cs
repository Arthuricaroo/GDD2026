using UnityEngine;
using UnityEngine.UI;

public class VidaUI : MonoBehaviour
{
    [Header("Coracoes (um campo para cada)")]
    public GameObject coracao1;   // o da esquerda (ultimo a sumir)
    public GameObject coracao2;
    public GameObject coracao3;
    public GameObject coracao4;   // o da direita (primeiro a sumir)

    [Header("Fase")]
    public bool resetarVidasAoIniciar = false;   // marque SO na Fase 1

    private GameObject[] coracoes;

    void Awake()
    {
        coracoes = new GameObject[] { coracao1, coracao2, coracao3, coracao4 };

        if (resetarVidasAoIniciar)
            VidaJogador.Resetar();
    }

    void OnEnable()
    {
        if (coracoes == null)
            coracoes = new GameObject[] { coracao1, coracao2, coracao3, coracao4 };

        VidaJogador.AoMudar += Atualizar;
        Atualizar(VidaJogador.Vidas);
    }

    void OnDisable()
    {
        VidaJogador.AoMudar -= Atualizar;
    }

    // 4 vidas = os 4 aparecem | 3 vidas = some o 4 | ... | 0 = nenhum
    void Atualizar(int vidas)
    {
        for (int i = 0; i < coracoes.Length; i++)
        {
            if (coracoes[i] != null)
                Mostrar(coracoes[i], i < vidas);
        }
    }

    // Liga/desliga so o desenho, sem desativar o objeto
    void Mostrar(GameObject coracao, bool visivel)
    {
        foreach (Graphic g in coracao.GetComponentsInChildren<Graphic>(true))
            g.enabled = visivel;

        foreach (SpriteRenderer sr in coracao.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = visivel;
    }
}