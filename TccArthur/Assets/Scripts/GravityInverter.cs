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