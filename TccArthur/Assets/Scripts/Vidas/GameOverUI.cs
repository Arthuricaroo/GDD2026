using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    public GameObject painelGameOver;              // painel com o texto GAME OVER
    public float tempoNaTela = 3f;                 // segundos mostrando o game over
    public string nomeDaPrimeiraFase = "Fase 1";    // nome exato da cena da fase 1

    void Awake()
    {
        // Esconde o painel no inicio (menos se este script estiver no proprio painel)
        if (painelGameOver != null && painelGameOver != gameObject)
            painelGameOver.SetActive(false);
    }

    public void Mostrar()
    {
        // Liga o painel antes de iniciar a rotina (funciona mesmo se o script estiver nele)
        if (painelGameOver != null)
            painelGameOver.SetActive(true);

        StartCoroutine(Rotina());
    }

    IEnumerator Rotina()
    {
        yield return new WaitForSecondsRealtime(tempoNaTela);

        // A gravidade e estatica: se o Ubi morreu invertido, volta ao normal
        Physics2D.gravity = new Vector2(0f, -9.81f);
        VidaJogador.Resetar();

        SceneManager.LoadScene(nomeDaPrimeiraFase);
    }
}