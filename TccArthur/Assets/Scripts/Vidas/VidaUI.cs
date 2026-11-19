using UnityEngine;
using UnityEngine.UI;

public class VidaUI : MonoBehaviour
{
    public Image[] coracoes;                         // arraste os 3 coracoes, do primeiro ao ultimo
    public bool resetarVidasAoIniciar = false;       // marque SO na Fase 1

    void Awake()
    {
        if (resetarVidasAoIniciar)
            VidaJogador.Resetar();
    }

    void OnEnable()
    {
        if (coracoes == null || coracoes.Length == 0)
            Debug.LogWarning("VidaUI: arraste os 3 coracoes para a lista Coracoes.");

        VidaJogador.AoMudar += Atualizar;
        Atualizar(VidaJogador.Vidas);
    }

    void OnDisable()
    {
        VidaJogador.AoMudar -= Atualizar;
    }

    // Some o coracao ao perder vida e volta a aparecer ao ganhar
    void Atualizar(int vidas)
    {
        if (coracoes == null) return;

        for (int i = 0; i < coracoes.Length; i++)
        {
            if (coracoes[i] != null)
                coracoes[i].enabled = i < vidas;
        }
    }
}