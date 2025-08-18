using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : CharacterBase
{
    public enum MovementMode { Locked, Battle, Free }
    public MovementMode movementMode = MovementMode.Free;

    public GameObject hammerPrefab;
    public GameObject shieldPrefab;
    public Transform handTransform;
    public HealthBarUI healthBarUI;

    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float jumpPower = 80f;
    private bool isJumping = false;
    private static readonly string[] PlayerHBNames 
            = { "PlayerHealthBar", "P_HealthBar", "HealthBar_Player" };
    protected override void Awake()
    {
        base.Awake();
        GameObject bar = GameObject.Find("PlayerHealthBar");
        if (bar != null)
        {
            healthBarUI = bar.GetComponent<HealthBarUI>();
            healthBarUI.Setup(maxHealth);
        }
    }

    private void OnEnable()
    {
        TryBindHealthBar();
    }
    void Start()
    {
        TryBindHealthBar();
        Transform anchor = FindDeepChild(transform, "HeadAnchor");
        if (anchor != null)
        {
            GameEvents.SetPlayerAnchor(anchor);
        }

        Transform weaponPos = FindDeepChild(transform, "WeaponPos");
        if (weaponPos != null)
            handTransform = weaponPos;

        transform.rotation = Quaternion.Euler(0, 90f, 0);
    }

    void Update()
    {
        Jump();
        HandleInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void TryBindHealthBar()
    {
        if (healthBarUI != null) return;

        GameObject uiObj = null;
        foreach (var n in PlayerHBNames)
        {
            uiObj = GameObject.Find(n);
            if (uiObj != null) break;
        }
        // (태그를 쓰면 아래도 가능) uiObj = uiObj ?? GameObject.FindGameObjectWithTag("PlayerHealthBar");

        if (uiObj != null)
        {
            healthBarUI = uiObj.GetComponent<HealthBarUI>();
            if (healthBarUI != null)
            {
                healthBarUI.Setup(maxHealth);
                healthBarUI.UpdateHealth(currentHealth);
            }
        }
    }
    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = Vector3.zero;

        switch (movementMode)
        {
            case MovementMode.Locked:
                input = Vector3.zero;
                animator.SetBool("isWalk", false);
                break;
            case MovementMode.Battle:
                input = new Vector3(h, 0, v);
                break;
            case MovementMode.Free:
                input = new Vector3(h, 0, v);
                break;
        }

        if (input.magnitude < 0.01f)
        {
            animator.SetBool("isWalk", false);
            return;
        }

        animator.SetBool("isWalk", true);

        if (movementMode != MovementMode.Locked && input != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        Vector3 move = input.normalized * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            animator.SetTrigger("isJump");
            isJumping = true;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isJumping = false;
    }

    void HandleInput()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Battle) return;

        if (Input.GetMouseButtonDown(0) && !isBlocking)
        {
            PlayAttackAnimation();
        }

        if (Input.GetMouseButtonDown(1) && !isBlocking)
        {
            StartCoroutine(BlockRoutine());
        }
    }

    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        animator.SetBool("isBlock", true);

        yield return new WaitForSeconds(1f); // 1.5초 동안 방어 유지

        isBlocking = false;
        animator.SetBool("isBlock", false);
    }

    public void EquipWeapon(bool isHammer)
    {
        if (handTransform == null) return;

        DestroyWeapon();

        GameObject prefab = isHammer ? hammerPrefab : shieldPrefab;
        currentWeapon = Instantiate(prefab, handTransform.position, handTransform.rotation, handTransform);

        // 모든 히트박스의 소유자 지정 + 기본 비활성(공격 타이밍에만 켜짐)
        var hitboxes = currentWeapon.GetComponentsInChildren<WeaponHitbox>(true);
        if (hitboxes != null && hitboxes.Length > 0)
        {
            foreach (var hb in hitboxes)
            {
                hb.owner = WeaponHitbox.OwnerType.Player;
                hb.SetActive(false); // 기본 OFF
            }
        }
    }

    protected override void UpdateHealthUI()
    {
        if (healthBarUI == null) TryBindHealthBar();
        if (healthBarUI != null)
            healthBarUI.UpdateHealth(currentHealth);
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name) return t;
        }
        return null;
    }

    public override void PlayAttackAnimation()
    {
        int random = Random.Range(0, 2);
        animator.SetTrigger(random == 0 ? "Attack1" : "Attack2");

        // 공격 타이밍: 예) 0.08초 준비 → 0.35초 동안 ON
        StartCoroutine(AttackHitboxWindow(0.35f, 0.08f));
    }

    private IEnumerator AttackHitboxWindow(float activeSeconds, float windup = 0f)
    {
        if (windup > 0f) yield return new WaitForSeconds(windup);

        var boxes = currentWeapon ? currentWeapon.GetComponentsInChildren<WeaponHitbox>(true) : null;
        if (boxes != null && boxes.Length > 0)
        {
            foreach (var b in boxes) b.SetActive(true);
            yield return new WaitForSeconds(activeSeconds);
            foreach (var b in boxes) b.SetActive(false);
        }
    }

    public override void Die()
    {
        base.Die(); // 부모 Die() 호출

        GameManager.Instance.currentState = GameManager.GameState.RoundEnd;
        GameManager.Instance.ShowGameOver();
    }

    public void ReviveForRespawn(Vector3 pos)
    {
        transform.position = pos;

        // 공통 리셋
        ReviveCommon();

        // 플레이어 전용 초기화
        GameManager.Instance?.SetPlayerMovement(MovementMode.Locked);
    }


}
