using UnityEngine;
using UnityEngine.SceneManagement;

// Tela de Game Over do Upside Down.
// Coloque este script num objeto vazio da cena GameOver (ex.: "GameOverManager")
// e ligue cada botão a uma das funções abaixo no campo On Click () do componente Button.
public class GameOverMenu : MonoBehaviour
{
    [Header("Nomes das cenas (exatamente como aparecem no Build Settings)")]
    [SerializeField] private string cenaMenu = "Menu";
    [SerializeField] private string cenaFase1 = "Fase1";

    // Botão MENU
    public void VoltarAoMenu()
    {
        Time.timeScale = 1f; // garante que o jogo não continue pausado
        SceneManager.LoadScene(cenaMenu);
    }

    // Botão RECOMEÇAR
    public void RecomecarFase1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaFase1);
    }
}
