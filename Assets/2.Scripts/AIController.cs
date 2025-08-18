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
        if (IsDead || currentState == AIState.Dead)
        {
            animator.SetBool("isWalk", false);
            return;
        }

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

            var boxes = currentWeapon.GetComponentsInChildren<WeaponHitbox>(true);
            int count = boxes != null ? boxes.Length : 0;
            Debug.Log($"[AI Equip] {selected.name} hitbox count = {count}");

            if (count == 0)
            {
                Debug.LogWarning($"[AI Equip] {selected.name} �ȿ� WeaponHitbox�� �����ϴ�.");
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    boxes[i].owner = WeaponHitbox.OwnerType.AI;
                    boxes[i].SetActive(false); // �⺻ OFF (���� Ÿ�ֿ̹��� ON)
                    Debug.Log($"[AI Equip] box[{i}] obj='{boxes[i].gameObject.name}', damage={boxes[i].damage}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[AI Equip] weaponPosTransform �Ǵ� selected �������� ����ֽ��ϴ�.");
        }

        // ���� ���� �Ϸ� �� ��� ��Ʋ ���� & �߰� ����
        GameManager.Instance.ForceEnterBattleNow();
        StartChase();

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

            // AI�� �÷��̾�� ����ġ��, Battle ������ �� �ٷ� ����
            if (!isAttacking && GameManager.Instance.currentState == GameManager.GameState.Battle)
                StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = AIState.Attack;

        PlayAttackAnimation();

        // �����
        yield return new WaitForSeconds(0.08f);

        var boxes = currentWeapon ? currentWeapon.GetComponentsInChildren<WeaponHitbox>(true) : null;
        if (boxes != null) foreach (var b in boxes) b.SetActive(true);

        // ��ȿ Ÿ��
        yield return new WaitForSeconds(0.35f);

        if (boxes != null) foreach (var b in boxes) b.SetActive(false);

        // ��� �õ�
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

    public override void Die()
    {
        // �ߺ� ����
        if (currentState == AIState.Dead) return;

        // 1) ���� ���� + ��ƾ ���� ����
        currentState = AIState.Dead;
        StopAllCoroutines();
        isAttacking = false;

        // 2) �̵� ���� ����(���� ���� 0)
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3) ���� ���� ó�� (�ִϸ��̼� Ʈ����, ���� �ı�, RoundEnd ��)
        base.Die();

        // 4) ����Ʈ ����
        GateHandler.TriggerAIDeath();

        // 5) �� ������ �� �� ������Ʈ ��Ȱ��(������ġ)
        StartCoroutine(DisableThisAfterSeconds(2.5f));
    }

    private IEnumerator DisableThisAfterSeconds(float secs)
    {
        yield return new WaitForSeconds(secs);
        this.enabled = false;
    }

    public void ReviveForRespawn(Vector3 pos)
    {
        transform.position = pos;

        // ���� ����
        ReviveCommon();

        // AI ���� �ʱ�ȭ
        currentState = AIState.Idle;
        StopAllCoroutines();
    }
}

