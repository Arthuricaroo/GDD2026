using UnityEngine;
using System.Collections;

public class KillPlayer : MonoBehaviour
{
    public GameObject Player;
    public Transform respawnPoint;
    public float respawnDelay = 1.5f;

    private GravityInverter gravityScript;

    void Start()
    {
        
        gravityScript = Player.GetComponent<GravityInverter>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        Player.SetActive(false);

        
        gravityScript.ResetGravity();

        yield return new WaitForSeconds(respawnDelay);

        Player.transform.position = respawnPoint.position;
        Player.SetActive(true);
    }

    public void SetRespawnPoint(Transform newPoint)
    {
        respawnPoint = newPoint;
        Debug.Log("Checkpoint atualizado: " + newPoint.position);
    }
}