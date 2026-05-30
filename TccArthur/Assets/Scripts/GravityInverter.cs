using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private bool isGravityInverted = false;
    private bool isRotating = false;
    public float rotationDuration = 1f; // Duração em segundos

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isRotating)
        {
            isGravityInverted = !isGravityInverted;
            Physics2D.gravity = -Physics2D.gravity;

            Quaternion targetRotation = isGravityInverted
                ? Quaternion.Euler(0f, 180f, 180f)
                : Quaternion.Euler(0f, 0f, 0f);

            StartCoroutine(RotateOverTime(targetRotation));
        }
    }

    public void ResetGravity()
    {
        // Para qualquer rotação em andamento
        StopAllCoroutines();
        isRotating = false;

        // Só reseta se estiver invertida
        if (isGravityInverted)
        {
            isGravityInverted = false;
            Physics2D.gravity = new Vector2(0f, -9.81f); // valor padrão da Unity
        }

        // Reseta a rotação do personagem instantaneamente
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private System.Collections.IEnumerator RotateOverTime(Quaternion target)
    {
        isRotating = true;
        Quaternion startRotation = transform.localRotation;
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;

            // Suaviza o movimento (opcional, remova para velocidade constante)
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localRotation = Quaternion.Lerp(startRotation, target, t);
            yield return null;
        }

        transform.localRotation = target; // Garante que chegue exatamente no alvo
        isRotating = false;
    }
}