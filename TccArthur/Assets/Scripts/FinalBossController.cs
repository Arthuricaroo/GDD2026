using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FinalBossController : MonoBehaviour
{
    public GameObject bottlePrefab;
    public Transform bottleSpawnPoint;
    public Animator animator;

    [Header("Tempo entre ataques (com raiva zerada)")]
    public float minWaitTime = 2f;
    public float maxWaitTime = 4f;

    [Header("Movimento")]
    public float moveSpeed = 3f;

    public float leftLimit = -5f;
    public float rightLimit = 5f;

    public LayerMask groundLayer;

    [Header("Perseguir o player")]
    public bool followPlayer = true;      // desmarque para voltar ao sorteio de esquerda/direita
    public float stopDistance = 1.5f;     // distância em que ele para de andar perto do player

    [Header("Vida (cai quando o player desvia)")]
    public float maxHealth = 30f;
    public float dodgeDamage = 1f;        // vida que cada arremesso desviado tira (dividida entre as garrafas dele)
    public float healOnPlayerHit = 1f;    // vida que o boss recupera se acertar o player (0 desliga)
    public Slider healthBar;              // opcional

    [Header("Raiva (cresce conforme o boss perde vida)")]
    [Range(0f, 1f)] public float phase2Rage = 0.5f;
    [Range(0f, 1f)] public float rainUnlockRage = 0.35f;
    public float waitMultiplierAtMaxRage = 0.4f;   // espera entre ataques (menor = mais rápido)
    public float speedMultiplierAtMaxRage = 1.6f;
    public float animSpeedAtMaxRage = 1.3f;
    public Color angryTint = new Color(1f, 0.45f, 0.45f);

    [Header("Garrafas laterais (voam retas, dos dois lados e nas duas alturas)")]
    public float straightSpeed = 6f;          // velocidade das garrafas retas
    public float straightLifeTime = 4f;       // quanto tempo elas voam (distância = velocidade x tempo)
    public float sideSpawnOffsetX = 1.5f;     // distância do centro do boss de onde elas nascem
    public Transform ceilingBottlePoint;      // objeto vazio na altura do player quando ele está no teto

    [Header("Ataques especiais")]
    [Range(0f, 1f)] public float specialChanceMin = 0.2f;  // com raiva zerada
    [Range(0f, 1f)] public float specialChanceMax = 0.7f;  // com raiva no máximo

    [Header("Rajada de garrafas")]
    public int burstCountMin = 3;
    public int burstCountMax = 7;
    public float burstInterval = 0.8f;

    [Header("Garrafa mirada no player")]
    public float aimedSpeed = 9f;
    public int aimedCountMin = 1;
    public int aimedCountMax = 4;
    public float aimedInterval = 0.3f;

    [Header("Chuva de garrafas")]
    public Transform rainSpawnPoint;         // opcional: objeto vazio na altura de onde as garrafas caem
    public float rainHeightAbovePlayer = 7f; // usado se rainSpawnPoint estiver vazio
    public float rainSpread = 8f;
    public int rainAmountMin = 4;
    public int rainAmountMax = 12;
    public float rainInterval = 0.25f;
    public float rainFallSpeed = 3f;
    public float rainDodgeWeight = 0.4f;     // cada garrafa da chuva tira só uma fração da vida

    [Header("Aviso antes do ataque")]
    public float telegraphTime = 0.5f;
    public Color warningColor = Color.white;

    [Header("Eventos")]
    public UnityEvent onPhase2;
    public UnityEvent onDeath;

    private bool attacking = false;
    private int direction = 1;

    private float currentHealth;
    private bool phase2 = false;
    private bool dead = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Transform player;

    // 0 = calmo, 1 = furioso (é a vida que o boss já perdeu)
    private float Rage
    {
        get { return 1f - Mathf.Clamp01(currentHealth / maxHealth); }
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }

        StartCoroutine(BossRoutine());
    }

    private void Update()
    {
        if (dead) return;

        if (attacking)
        {
            MoveBoss();
        }
    }

    // Procura o player (de novo, se ele foi recriado ao morrer)
    private Transform GetPlayer()
    {
        if (player == null)
        {
            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
                player = ph.transform;
        }

        return player;
    }

    // Direção (-1 esquerda, 1 direita) até o player. Se não achar o player, mantém a atual.
    private int DirectionToPlayer()
    {
        Transform target = GetPlayer();

        if (target == null)
            return direction;

        return target.position.x >= transform.position.x ? 1 : -1;
    }

    // ---------- Chamados pelas garrafas ----------
    public void OnBottleDodged(float weight)
    {
        if (dead) return;

        ChangeHealth(-dodgeDamage * weight);
    }

    public void OnPlayerHit()
    {
        if (dead) return;

        ChangeHealth(healOnPlayerHit);
    }

    private void ChangeHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (currentHealth <= 0f)
        {
            StopAllCoroutines();
            StartCoroutine(Die());
            return;
        }

        UpdateRageVisuals();

        if (!phase2 && Rage >= phase2Rage)
        {
            StartCoroutine(EnterPhase2());
        }
    }

    private void UpdateRageVisuals()
    {
        if (animator != null)
            animator.speed = Mathf.Lerp(1f, animSpeedAtMaxRage, Rage);

        if (spriteRenderer != null)
            spriteRenderer.color = CurrentBaseColor();
    }

    // ---------- Loop principal ----------
    private IEnumerator BossRoutine()
    {
        while (!dead)
        {
            // Fica parado (quanto mais raiva, menos tempo)
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            waitTime *= Mathf.Lerp(1f, waitMultiplierAtMaxRage, Rage);
            yield return new WaitForSeconds(waitTime);

            // Escolhe a direção: para o player, ou sorteia se followPlayer estiver desligado
            if (followPlayer)
                direction = DirectionToPlayer();
            else
                direction = Random.value < 0.5f ? -1 : 1;

            // Vira o personagem
            FlipBoss();

            // Aviso (pisca e treme) antes de atacar
            yield return StartCoroutine(Telegraph());

            // Começa o ataque
            attacking = true;
            animator.Play("FinalBoss_Walk");

            // Espera a animação terminar
            float speed = Mathf.Max(animator.speed, 0.01f);
            yield return new WaitForSeconds(
                GetAnimationLength("FinalBoss_Walk") / speed
            );

            // Para de andar
            attacking = false;

            // Volta para Idle
            animator.Play("FinalBoss_Idle");

            // Chance de fazer um ataque especial (cresce com a raiva)
            float chance = Mathf.Lerp(specialChanceMin, specialChanceMax, Rage);
            if (Random.value < chance)
            {
                yield return new WaitForSeconds(0.3f);
                yield return StartCoroutine(SpecialAttack());
                animator.Play("FinalBoss_Idle");
            }
        }
    }

    // ---------- Sorteia o ataque especial ----------
    private IEnumerator SpecialAttack()
    {
        // rajada e mirada sempre. a chuva só aparece com raiva suficiente.
        int options = Rage >= rainUnlockRage ? 3 : 2;
        int choice = Random.Range(0, options);

        yield return StartCoroutine(Telegraph());

        switch (choice)
        {
            case 0:
                yield return StartCoroutine(BottleBurst());
                break;
            case 1:
                yield return StartCoroutine(AimedThrow());
                break;
            case 2:
                yield return StartCoroutine(RainAttack());
                break;
        }
    }

    // ---------- Ataque: rajada de garrafas pelos dois lados ----------
    private IEnumerator BottleBurst()
    {
        int count = Mathf.RoundToInt(Mathf.Lerp(burstCountMin, burstCountMax, Rage));

        for (int i = 0; i < count; i++)
        {
            if (dead) yield break;

            ThrowSidePair();

            yield return new WaitForSeconds(burstInterval);
        }
    }

    // ---------- Ataque: garrafa mirada no player ----------
    private IEnumerator AimedThrow()
    {
        int count = Mathf.RoundToInt(Mathf.Lerp(aimedCountMin, aimedCountMax, Rage));

        for (int i = 0; i < count; i++)
        {
            if (dead) yield break;

            Transform target = GetPlayer();

            if (target != null)
            {
                // vira o boss para o lado do player
                direction = DirectionToPlayer();
                FlipBoss();

                Vector2 dir = (target.position - bottleSpawnPoint.position).normalized;
                SpawnBottle(bottleSpawnPoint.position, dir * aimedSpeed, 1f, true);
            }
            else
            {
                ThrowSidePair();
            }

            yield return new WaitForSeconds(aimedInterval);
        }
    }

    // ---------- Ataque: chuva de garrafas ----------
    private IEnumerator RainAttack()
    {
        int count = Mathf.RoundToInt(Mathf.Lerp(rainAmountMin, rainAmountMax, Rage));

        for (int i = 0; i < count; i++)
        {
            if (dead) yield break;

            Transform target = GetPlayer();

            float centerX = target != null ? target.position.x : transform.position.x;
            float x = centerX + Random.Range(-rainSpread, rainSpread);
            x = Mathf.Clamp(x, leftLimit, rightLimit);

            float y;
            if (rainSpawnPoint != null)
                y = rainSpawnPoint.position.y;
            else if (target != null)
                y = target.position.y + rainHeightAbovePlayer;
            else
                y = transform.position.y + rainHeightAbovePlayer;

            SpawnBottle(new Vector3(x, y, 0f), new Vector2(0f, -rainFallSpeed), rainDodgeWeight, false);

            yield return new WaitForSeconds(rainInterval);
        }
    }

    // ---------- Aviso antes do ataque ----------
    private IEnumerator Telegraph()
    {
        float t = 0f;
        Vector3 basePos = transform.position;

        while (t < telegraphTime)
        {
            t += Time.deltaTime;

            transform.position = basePos + (Vector3)(Random.insideUnitCircle * 0.04f);

            if (spriteRenderer != null)
            {
                bool on = Mathf.FloorToInt(t * 20f) % 2 == 0;
                spriteRenderer.color = on ? warningColor : CurrentBaseColor();
            }

            yield return null;
        }

        transform.position = basePos;

        if (spriteRenderer != null)
            spriteRenderer.color = CurrentBaseColor();
    }

    // ---------- Fase 2 ----------
    private IEnumerator EnterPhase2()
    {
        phase2 = true;

        StartCoroutine(CameraShake(0.4f, 0.15f));

        if (onPhase2 != null)
            onPhase2.Invoke();

        yield return null;
    }

    // ---------- Morte ----------
    private IEnumerator Die()
    {
        dead = true;
        attacking = false;
        animator.speed = 1f;
        animator.Play("FinalBoss_Idle");

        if (onDeath != null)
            onDeath.Invoke();

        StartCoroutine(CameraShake(0.8f, 0.25f));

        float t = 0f;
        Vector3 basePos = transform.position;

        while (t < 1.5f)
        {
            t += Time.deltaTime;

            transform.position = basePos + (Vector3)(Random.insideUnitCircle * 0.1f);

            if (spriteRenderer != null)
            {
                bool on = Mathf.FloorToInt(t * 15f) % 2 == 0;
                spriteRenderer.color = on ? warningColor : Color.red;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    // ---------- Efeitos ----------
    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 lastOffset = Vector3.zero;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            Vector2 random = Random.insideUnitCircle * magnitude;
            Vector3 newOffset = new Vector3(random.x, random.y, 0f);

            // aplica só a diferença, para não "puxar" a câmera de volta
            cam.transform.position += newOffset - lastOffset;
            lastOffset = newOffset;

            yield return null;
        }

        cam.transform.position -= lastOffset;
    }

    // Cor normal do boss: vai ficando avermelhada conforme a raiva sobe
    private Color CurrentBaseColor()
    {
        return Color.Lerp(originalColor, angryTint, Rage);
    }

    // ---------- Movimento ----------
    private void MoveBoss()
    {
        Collider2D bossCollider = GetComponent<Collider2D>();

        if (bossCollider == null)
            return;

        // Perseguindo o player: acompanha o lado dele e para quando chega perto
        if (followPlayer)
        {
            Transform target = GetPlayer();

            if (target != null)
            {
                float distance = Mathf.Abs(target.position.x - transform.position.x);

                // Perto demais: não anda (a animação continua, as garrafas continuam saindo)
                if (distance <= stopDistance)
                    return;

                int newDirection = DirectionToPlayer();

                if (newDirection != direction)
                {
                    direction = newDirection;
                    FlipBoss();
                }
            }
        }

        Bounds bounds = bossCollider.bounds;

        // Ponto na frente dos pés do boss
        Vector2 checkPosition = new Vector2(
            bounds.center.x + direction * (bounds.extents.x + 0.1f),
            bounds.min.y + 0.05f
        );

        // Procura chão abaixo desse ponto
        RaycastHit2D groundHit = Physics2D.Raycast(
            checkPosition,
            Vector2.down,
            0.5f,
            groundLayer
        );

        // Se não houver chão, para de andar
        if (groundHit.collider == null)
        {
            attacking = false;
            animator.Play("FinalBoss_Idle");
            return;
        }

        // Se houver chão, continua andando (mais rápido com raiva)
        float currentSpeed = moveSpeed * Mathf.Lerp(1f, speedMultiplierAtMaxRage, Rage);

        Vector3 position = transform.position;

        position.x += direction * currentSpeed * Time.deltaTime;

        position.x = Mathf.Clamp(position.x, leftLimit, rightLimit);

        transform.position = position;
    }

    private void FlipBoss()
    {
        Vector3 scale = transform.localScale;

        scale.x = Mathf.Abs(scale.x) * direction;

        transform.localScale = scale;
    }

    // ---------- Garrafas ----------
    // Chamado pelo evento da animação de andar
    public void ThrowBottle()
    {
        if (dead) return;

        ThrowSidePair();

        // Na fase 2, joga um segundo arremesso logo depois
        if (phase2)
            StartCoroutine(ExtraPair());
    }

    private IEnumerator ExtraPair()
    {
        yield return new WaitForSeconds(0.4f);

        if (!dead)
            ThrowSidePair();
    }

    // Um arremesso: esquerda e direita ao mesmo tempo, no chão e no teto (se o ponto do teto existir)
    private void ThrowSidePair()
    {
        if (dead) return;

        bool hasCeiling = ceilingBottlePoint != null;

        // 4 garrafas por arremesso (2 se não houver ponto do teto).
        // O dano ao boss é dividido entre elas, então um arremesso inteiro desviado tira "dodgeDamage".
        float weight = hasCeiling ? 0.25f : 0.5f;

        float groundY = bottleSpawnPoint.position.y;

        ThrowSideBottle(-1, groundY, weight);
        ThrowSideBottle(1, groundY, weight);

        if (hasCeiling)
        {
            float ceilingY = ceilingBottlePoint.position.y;

            ThrowSideBottle(-1, ceilingY, weight);
            ThrowSideBottle(1, ceilingY, weight);
        }
    }

    private void ThrowSideBottle(int side, float y, float weight)
    {
        Vector3 pos = new Vector3(
            transform.position.x + side * sideSpawnOffsetX,
            y,
            bottleSpawnPoint.position.z
        );

        SpawnBottle(pos, new Vector2(side * straightSpeed, 0f), weight, true);
    }

    // Cria uma garrafa. straight = true: voa reta, sem gravidade
    private void SpawnBottle(Vector3 position, Vector2 velocity, float weight, bool straight)
    {
        if (dead) return;

        GameObject bottle = Instantiate(
            bottlePrefab,
            position,
            Quaternion.identity
        );

        Bottle bottleScript = bottle.GetComponent<Bottle>();

        if (bottleScript != null)
        {
            bottleScript.boss = this;
            bottleScript.dodgeWeight = weight;

            if (straight)
                bottleScript.lifeTime = straightLifeTime;

            bottleScript.Throw(velocity, straight);
        }
    }

    private float GetAnimationLength(string animationName)
    {
        RuntimeAnimatorController controller =
            animator.runtimeAnimatorController;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        return 1f;
    }
}