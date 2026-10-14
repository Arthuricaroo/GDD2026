using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target;

    [Header("Eixo X (segue o player)")]
    public float offsetX = 0f;
    public float smoothSpeed = 5f;

    [Header("Eixo Y")]
    public bool followY = false;      // desmarcado = Y fixo (vê teto e chão)
    public float fixedY = 0f;         // altura central da sala (ajuste no Inspector)
    public float offsetY = 0f;        // usado só se followY estiver marcado

    [Header("Profundidade")]
    public float cameraZ = -10f;

    void LateUpdate()
    {
        if (target == null) return;

        float desiredX = target.position.x + offsetX;
        float desiredY = followY ? target.position.y + offsetY : fixedY;

        Vector3 desiredPosition = new Vector3(desiredX, desiredY, cameraZ);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}