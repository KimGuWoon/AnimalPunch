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
    [Range(0f, 1f)] public float blockChance = 0.12f; // 기존 0.30 → 0.12로 하향
    public float blockDuration = 0.6f;                // 기존 1.0 → 0.6초
    public float blockCooldown = 2.0f;                // 방어 후 최소 2초 간 재사용 금지
    private float lastBlockTime = -999f;              // 최근 방어 시각

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

        // 무기 위치
        weaponPosTransform = FindDeepChild(transform, "WeaponPos");

        // HeadAnchor 세팅
        Transform anchor = FindDeepChild(transform, "HeadAnchor");
        if (anchor != null)
        {
            GameEvents.SetAIAnchor(anchor);
        }

        // 플레이어 추적 시작
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
        bool aiShouldUseHammer = !isPlayerWinner; // 플레이어와 반대로

        if (aiChooseWeaponRoutine != null)
            StopCoroutine(aiChooseWeaponRoutine);

        aiChooseWeaponRoutine = StartCoroutine(ChooseWeaponAfterDelay(aiShouldUseHammer));
    }

    private System.Collections.IEnumerator ChooseWeaponAfterDelay(bool useHammer)
    {
        float delay = UnityEngine.Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(delay);

        // 이미 들고 있던 무기 제거
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
                Debug.LogWarning($"[AI Equip] {selected.name} 안에 WeaponHitbox가 없습니다.");
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    boxes[i].owner = WeaponHitbox.OwnerType.AI;
                    boxes[i].SetActive(false); // 기본 OFF (공격 타이밍에만 ON)
                    Debug.Log($"[AI Equip] box[{i}] obj='{boxes[i].gameObject.name}', damage={boxes[i].damage}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[AI Equip] weaponPosTransform 또는 selected 프리팹이 비어있습니다.");
        }

        // 무기 장착 완료 → 즉시 배틀 진입 & 추격 시작
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

            // AI는 플레이어와 마주치면, Battle 상태일 때 바로 공격
            if (!isAttacking && GameManager.Instance.currentState == GameManager.GameState.Battle)
                StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        currentState = AIState.Attack;

        PlayAttackAnimation();

        // 윈드업
        yield return new WaitForSeconds(0.08f);

        var boxes = currentWeapon ? currentWeapon.GetComponentsInChildren<WeaponHitbox>(true) : null;
        if (boxes != null) foreach (var b in boxes) b.SetActive(true);

        // 유효 타임
        yield return new WaitForSeconds(0.35f);

        if (boxes != null) foreach (var b in boxes) b.SetActive(false);

        // 방어 시도
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
        if (isBlocking) yield break; // 중복 방어 방지

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
        // 중복 방지
        if (currentState == AIState.Dead) return;

        // 1) 상태 고정 + 루틴 완전 정지
        currentState = AIState.Dead;
        StopAllCoroutines();
        isAttacking = false;

        // 2) 이동 완전 정지(물리 값도 0)
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3) 공통 죽음 처리 (애니메이션 트리거, 무기 파괴, RoundEnd 등)
        base.Die();

        // 4) 게이트 생성
        GateHandler.TriggerAIDeath();

        // 5) 한 프레임 뒤 이 컴포넌트 비활성(안전장치)
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

        // 공통 리셋
        ReviveCommon();

        // AI 전용 초기화
        currentState = AIState.Idle;
        StopAllCoroutines();
    }
}

