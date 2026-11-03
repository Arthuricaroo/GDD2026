using UnityEngine;

public class GravityInverter : MonoBehaviour
{
    private bool isGravityInverted = false;
    private bool isRotating = false;
    public float rotationDuration = 1f;

    [Header("Áudio")]
    public AudioClip gravityUpSound;
    public AudioClip gravityDownSound;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    private Animator anim;
    private AudioSource audioSource;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isRotating)
        {
            isGravityInverted = !isGravityInverted;
            Physics2D.gravity = -Physics2D.gravity;

            anim.SetTrigger("FlipGravity");
            PlayGravitySound();

            Quaternion targetRotation = isGravityInverted
                ? Quaternion.Euler(0f, 180f, 180f)
                : Quaternion.Euler(0f, 0f, 0f);

            StartCoroutine(RotateOverTime(targetRotation));
        }
    }

    private void PlayGravitySound()
    {
        if (audioSource == null) return;

        AudioClip clip = isGravityInverted ? gravityUpSound : gravityDownSound;

        if (clip == null) clip = gravityUpSound != null ? gravityUpSound : gravityDownSound;
        if (clip == null) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, sfxVolume);
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
