using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : CharacterBase
{
    public enum AIState { Idle, ChoosingWeapon, MoveToPlayer, Attack, Defend, Dead }
    public AIState currentState = AIState.Idle;

    public HealthBarUI healthBarUI;
    public GameObject hammerPrefab;
    public GameObject bakPrefab;
    public float moveSpeed = 2f;
    public float stoppingDistance = 0.5f;

    private Transform weaponPosTransform;
    private Transform playerTransform;
    private bool isAttacking = false;
    private Coroutine aiChooseWeaponRoutine;

    [Header("Defense (AI Block)")]
    [Range(0f, 1f)] public float blockChance = 0.12f; // ���� 0.30 �� 0.12�� ����
    public float blockDuration = 0.6f;                // ���� 1.0 �� 0.6��
    public float blockCooldown = 2.0f;                // ��� �� �ּ� 2�� �� ���� ����
    private float lastBlockTime = -999f;              // �ֱ� ��� �ð�

    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.ai = this;
    }

    private void OnEnable()
    {
        GameEvents.OnShowWeaponPanel += OnShowWeaponPanel_AI;
    }

    private void OnDisable()
    {
        GameEvents.OnShowWeaponPanel -= OnShowWeaponPanel_AI;
    }

  
    void Start()
    {
        GameObject h_barObj = GameObject.Find("AIHealthBar");
        if (h_barObj != null)
        {
            healthBarUI = h_barObj.GetComponent<HealthBarUI>();
            healthBarUI.Setup(maxHealth);
        }

        // ���� ��ġ
        weaponPosTransform = FindDeepChild(transform, "WeaponPos");

        // HeadAnchor ����
        Transform anchor = FindDeepChild(transform, "HeadAnchor");
        if (anchor != null)
        {
            GameEvents.SetAIAnchor(anchor);
        }

        // �÷��̾� ���� ����
        StartCoroutine(WaitForPlayer());
    }

    void Update()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Battle)
            return;

        if (currentState == AIState.MoveToPlayer && currentWeapon != null)
        {
            MoveToPlayer();
        }
    }

    private void OnShowWeaponPanel_AI(bool isPlayerWinner)
    {
        bool aiShouldUseHammer = !isPlayerWinner; // �÷��̾�� �ݴ��

        if (aiChooseWeaponRoutine != null)
            StopCoroutine(aiChooseWeaponRoutine);

        aiChooseWeaponRoutine = StartCoroutine(ChooseWeaponAfterDelay(aiShouldUseHammer));
    }

    private System.Collections.IEnumerator ChooseWeaponAfterDelay(bool useHammer)
    {
        float delay = UnityEngine.Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(delay);

        // �̹� ��� �ִ� ���� ����
        if (currentWeapon != null)
            Destroy(currentWeapon);

        GameObject selected = useHammer ? hammerPrefab : bakPrefab;

        if (weaponPosTransform != null && selected != null)
        {
            currentWeapon = Instantiate(
                selected,
                weaponPosTransform.position,
                weaponPosTransform.rotation,
                weaponPosTransform
            );

            WeaponHitbox hitbox = currentWeapon.GetComponentInChildren<WeaponHitbox>();
            if (hitbox != null)
                hitbox.owner = WeaponHitbox.OwnerType.AI;
        }

        StartChase(); // ������ �� ������ ������ �߰� ����
        aiChooseWeaponRoutine = null;
    }
    private void MoveToPlayer()
    {
        if (playerTransform == null || rb == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist > stoppingDistance)
        {
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            Vector3 targetPos = transform.position + dir * moveSpeed * Time.deltaTime;
            rb.MovePosition(targetPos);
            transform.LookAt(playerTransform);

            animator.SetBool("isWalk", true);
        }
        else
        {
            animator.SetBool("isWalk", false);
            if (!isAttacking)
                StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = AIState.Attack;

        PlayAttackAnimation();

        yield return new WaitForSeconds(0.5f);

        if (Time.time - lastBlockTime >= blockCooldown && Random.value < blockChance)
        {
            yield return StartCoroutine(BlockForSeconds(blockDuration));
            lastBlockTime = Time.time;
        }

        currentState = AIState.MoveToPlayer;
        isAttacking = false;
    }

    public void SelectWeaponBasedOnResult(bool isWinner)
    {
        bool aiShouldUseHammer = !isWinner;

        if (aiChooseWeaponRoutine != null)
            StopCoroutine(aiChooseWeaponRoutine);

        aiChooseWeaponRoutine = StartCoroutine(ChooseWeaponAfterDelay(aiShouldUseHammer));
    }

   

    public void StartChase()
    {
        if (playerTransform == null || currentWeapon == null) return;

        currentState = AIState.MoveToPlayer;
    }

    IEnumerator WaitForPlayer()
    {
        while (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            yield return null;
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }
        return null;
    }

    protected override void UpdateHealthUI()
    {
        if (healthBarUI != null)
            healthBarUI.UpdateHealth(currentHealth);
    }

    public override void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    private IEnumerator BlockForSeconds(float secs)
    {
        if (isBlocking) yield break; // �ߺ� ��� ����

        isBlocking = true;
        animator.SetTrigger("isBlock");

        float elapsed = 0f;
        while (elapsed < secs)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        isBlocking = false;
    }

    protected override void Die()
    {
        base.Die();
        GateHandler.TriggerAIDeath();
    }
}
