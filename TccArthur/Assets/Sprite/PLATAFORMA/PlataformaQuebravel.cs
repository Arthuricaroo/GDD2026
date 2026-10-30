using System.Collections;
using UnityEngine;

public class PlataformaQuebravel : MonoBehaviour
{
    [SerializeField] string triggerTremer = "Tremer";
    [SerializeField] float tempoTremendo = 0.6f;     // quanto tempo treme antes de cair
    [SerializeField] float aceleracaoQueda = 25f;
    [SerializeField] float velocidadeMaxima = 15f;
    [SerializeField] float tempoCaindo = 2f;         // tempo caindo até sumir
    [SerializeField] float tempoParaVoltar = 3f;     // tempo sumida até voltar
    [SerializeField] bool desativarColisorAoCair = true;

    bool ativada;
    Vector3 posicaoInicial;
    Animator animator;
    Collider2D[] colisores;
    SpriteRenderer[] sprites;

    void Awake()
    {
        posicaoInicial = transform.position;
        animator = GetComponentInChildren<Animator>();
        colisores = GetComponents<Collider2D>();
        sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (ativada || !col.gameObject.CompareTag("Player")) return;
        ativada = true;
        StartCoroutine(Quebrar());
    }

    IEnumerator Quebrar()
    {
        // Treme
        if (animator) animator.SetTrigger(triggerTremer);
        yield return new WaitForSeconds(tempoTremendo);

        // Cai
        if (desativarColisorAoCair)
            foreach (var c in colisores) c.enabled = false;

        float vel = 0f;
        float t = 0f;
        while (t < tempoCaindo)
        {
            vel = Mathf.Min(vel + aceleracaoQueda * Time.deltaTime, velocidadeMaxima);
            transform.position += Vector3.down * vel * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        // Some
        foreach (var s in sprites) s.enabled = false;
        foreach (var c in colisores) c.enabled = false;
        yield return new WaitForSeconds(tempoParaVoltar);

        // Volta
        transform.position = posicaoInicial;
        if (animator)
        {
            animator.ResetTrigger(triggerTremer);
            animator.Rebind();
            animator.Update(0f);
        }
        foreach (var s in sprites) s.enabled = true;
        foreach (var c in colisores) c.enabled = true;
        ativada = false;
    }
}