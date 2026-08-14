using UnityEngine;

public class GravityInverter : MonoBehaviour
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
        StopAllCoroutines();
        isRotating = false;

        if (isGravityInverted)
        {
            isGravityInverted = false;
            Physics2D.gravity = new Vector2(0f, -9.81f);
        }

        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    // NOVO: expõe o estado da gravidade pra outros scripts (como o Enemy.cs)
    public bool IsInverted()
    {
        return isGravityInverted;
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
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.localRotation = Quaternion.Lerp(startRotation, target, t);
            yield return null;
        }

        transform.localRotation = target;
        isRotating = false;
    }
}