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

    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.ai = this;
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
        if (GameManager.Instance.currentState != GameManager.GameState.Battle)
            return;

        if (currentState == AIState.MoveToPlayer && currentWeapon != null)
        {
            MoveToPlayer();
        }
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

        if (Random.value < 0.3f)
        {
            isBlocking = true;
            animator.SetTrigger("isBlock");

            float blockDuration = 1f;
            float elapsed = 0f;

            while (elapsed < blockDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            isBlocking = false;
        }

        currentState = AIState.MoveToPlayer;
        isAttacking = false;
    }

    public void SelectWeaponBasedOnResult(bool isWinner)
    {
        currentState = AIState.ChoosingWeapon;
        StartCoroutine(DelayedWeaponSelect(isWinner));
    }

    IEnumerator DelayedWeaponSelect(bool isWinner)
    {
        float delay = Random.Range(0f, 1f);
        yield return new WaitForSeconds(delay);

        GameObject selected = isWinner ? hammerPrefab : bakPrefab;

        if (currentWeapon != null)
            Destroy(currentWeapon);

        if (weaponPosTransform != null)
        {
            currentWeapon = Instantiate(
                selected,
                weaponPosTransform.position,
                weaponPosTransform.rotation,
                weaponPosTransform
            );

            WeaponHitbox hitbox = currentWeapon.GetComponentInChildren<WeaponHitbox>();
            if (hitbox != null)
            {
                hitbox.owner = WeaponHitbox.OwnerType.AI;
            }
        }

        StartChase(); // 추격 시작은 무기 선택이 끝난 다음
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

    protected override void Die()
    {
        base.Die();
        GateHandler.TriggerAIDeath();
    }
}
